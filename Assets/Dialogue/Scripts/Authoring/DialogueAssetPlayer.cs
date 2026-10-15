using System;
using System.Collections.Generic;
using ProjectNTH.Settings;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

namespace ProjectNTH.Dialogue.Authoring
{
    [Serializable]
    public sealed class DialogueSceneAction
    {
        [HideInInspector] public string stepId;
        [HideInInspector] public string label;
        public UnityEvent action = new UnityEvent();
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Project NTH/Dialogue/Dialogue Asset Player")]
    public sealed class DialogueAssetPlayer : MonoBehaviour
    {
        [SerializeField] private DialogueConversation conversation;
        [SerializeField] private global::DialogueSystemController dialogueSystem;
        [Tooltip("Optional. Leave empty to create a dedicated 2D voice source at runtime.")]
        [SerializeField] private AudioSource voiceSource;
        [Tooltip("Optional, separate from voice. Leave empty to create a dedicated 2D effects source.")]
        [SerializeField] private AudioSource effectsSource;
        [SerializeField] private bool triggerOnPlayerEnter;
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private List<DialogueSceneAction> sceneActions = new List<DialogueSceneAction>();
        private bool hasPlayed;
        private AuthoredDialogueBridge bridge;
        private YarnTaskCompletionSource waitCompletion;
        private float waitRemaining;
        public DialogueConversation Conversation => conversation;
        public global::DialogueSystemController DialogueSystem => dialogueSystem;

#if UNITY_EDITOR
        public void SynchronizeSceneActions()
        {
            var updated = new List<DialogueSceneAction>();
            if (conversation != null)
                foreach (var step in conversation.steps)
                {
                    if (step == null || step.kind != DialogueStepKind.SceneAction) continue;
                    var binding = sceneActions.Find(entry => entry.stepId == step.id)
                        ?? new DialogueSceneAction { stepId = step.id };
                    binding.label = step.actionLabel;
                    updated.Add(binding);
                }
            sceneActions = updated;
        }
#endif

        public void PlayConversation() => TryPlayConversation();

        public bool TryPlayConversation()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || (triggerOnce && hasPlayed)) return false;
            var errors = DialogueYarnText.Validate(conversation);
            if (errors.Count > 0) { Debug.LogWarning(string.Join("\n", errors), this); return false; }
            if (conversation.GeneratedHash != DialogueYarnText.Fingerprint(conversation))
            {
                Debug.LogWarning("Conversation has changed since its Yarn was generated. Select it and click Generate Yarn before playing.", this);
                return false;
            }
            if (dialogueSystem == null || dialogueSystem.DialogueRunner == null || !dialogueSystem.DialogueRunner.isActiveAndEnabled)
            {
                Debug.LogWarning("Assign an enabled Dialogue System Controller with a Dialogue Runner.", this);
                return false;
            }
            var runner = dialogueSystem.DialogueRunner;
            if (runner.IsDialogueRunning) return false;
            if (runner.YarnProject == null || Array.IndexOf(runner.YarnProject.NodeNames, conversation.NodeName) < 0)
            {
                Debug.LogWarning("The runner's Yarn Project does not contain this generated conversation. Use Assets/Dialogue/Yarn Project and generate the conversation first.", this);
                return false;
            }
            if (voiceSource != null && voiceSource == effectsSource)
            {
                Debug.LogWarning("Voice and sound effects need separate Audio Sources so their volume settings and playback stay independent.", this);
                return false;
            }
            foreach (var step in conversation.steps)
            {
                if (step.kind != DialogueStepKind.SceneAction) continue;
                var binding = sceneActions.Find(entry => entry.stepId == step.id);
                if (binding == null || !HasAction(binding.action))
                {
                    Debug.LogWarning("Connect the scene action '" + step.actionLabel + "' in this player's Inspector before starting.", this);
                    return false;
                }
            }
            bridge = runner.GetComponent<AuthoredDialogueBridge>();
            if (bridge == null) bridge = runner.gameObject.AddComponent<AuthoredDialogueBridge>();
            if (!bridge.TryBegin(this)) return false;
            hasPlayed = true;
            return true;
        }

        private static bool HasAction(UnityEvent action)
        {
            if (action == null) return false;
            for (int i = 0; i < action.GetPersistentEventCount(); i++)
                if (action.GetPersistentTarget(i) != null && !string.IsNullOrEmpty(action.GetPersistentMethodName(i))
                    && action.GetPersistentListenerState(i) != UnityEventCallState.Off) return true;
            return false;
        }

        public void StopConversation() { if (bridge != null) bridge.StopOwnedConversation(this); }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnPlayerEnter && other.CompareTag("Player")) PlayConversation();
        }

        private void OnDisable() { StopConversation(); StopAudio(); }

        internal async YarnTask ExecuteStep(string id)
        {
            var step = conversation.FindStep(id);
            if (step == null) { Debug.LogError("Generated dialogue contains an outdated step. Regenerate the conversation.", this); StopConversation(); return; }
            if (voiceSource != null) voiceSource.Stop();
            if (step.kind == DialogueStepKind.Line)
            {
                PlayClip(step.voice, step.character, DialogueAudioKind.Voice);
                PlayClip(step.soundEffect, step.character, DialogueAudioKind.SoundEffect);
            }
            else if (step.kind == DialogueStepKind.SoundEffect) PlayClip(step.soundEffect, step.character, DialogueAudioKind.SoundEffect);
            else if (step.kind == DialogueStepKind.SceneAction)
            {
                var binding = sceneActions.Find(entry => entry.stepId == step.id);
                binding?.action?.Invoke();
            }
            else if (step.kind == DialogueStepKind.Wait && step.waitSeconds > 0f)
            {
                waitRemaining = step.waitSeconds;
                waitCompletion = new YarnTaskCompletionSource();
                await waitCompletion.Task;
            }
        }

        private void Update()
        {
            if (waitCompletion == null) return;
            waitRemaining -= Time.deltaTime;
            if (waitRemaining <= 0f) CompleteWait();
        }

        internal void CompleteWait()
        {
            // Complete synchronously on interruption, before a scene unload destroys the runner.
            var completion = waitCompletion;
            waitCompletion = null;
            completion?.TrySetResult();
        }

        private void PlayClip(DialogueClipSelection selection, DialogueCharacter character, DialogueAudioKind kind)
        {
            var clip = selection.Resolve(character, kind, out float volume);
            if (clip == null) return;
            bool voice = kind == DialogueAudioKind.Voice;
            var source = voice ? voiceSource : effectsSource;
            if (source == null)
            {
                var child = new GameObject(voice ? "Dialogue Voice" : "Dialogue Effects");
                child.transform.SetParent(transform, false);
                source = child.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                if (voice) voiceSource = source; else effectsSource = source;
            }
            GameSettings.Route(source, voice ? AudioCategory.Dialogue : AudioCategory.SoundEffects);
            if (voice)
            {
                source.Stop();
                source.PlayOneShot(clip, volume * (character != null ? character.voiceVolume : 1f));
            }
            else source.PlayOneShot(clip, volume);
        }

        internal void StopAudio()
        {
            if (voiceSource != null) voiceSource.Stop();
            if (effectsSource != null) effectsSource.Stop();
        }
    }
}
