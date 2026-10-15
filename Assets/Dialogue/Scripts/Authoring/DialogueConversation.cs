using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectNTH.Dialogue.Authoring
{
    public enum DialogueStepKind { Line, SoundEffect, SceneAction, Wait }
    public enum DialogueClipMode { None, AudioClip, CharacterLibrary }

    [Serializable]
    public sealed class DialogueClipSelection
    {
        public DialogueClipMode mode;
        public AudioClip clip;
        public string cueId;

        public AudioClip Resolve(DialogueCharacter character, DialogueAudioKind kind, out float volume)
        {
            volume = 1f;
            if (mode == DialogueClipMode.AudioClip) return clip;
            if (mode != DialogueClipMode.CharacterLibrary || character == null) return null;
            var cue = character.FindCue(cueId, kind);
            if (cue == null) return null;
            volume = cue.volume;
            return cue.clip;
        }
    }

    [Serializable]
    public sealed class DialogueStep
    {
        [HideInInspector] public string id = Guid.NewGuid().ToString("N");
        public DialogueStepKind kind;
        public DialogueCharacter character;
        [TextArea(2, 6)] public string text;
        public DialogueClipSelection voice = new DialogueClipSelection();
        public DialogueClipSelection soundEffect = new DialogueClipSelection();
        public string actionLabel = "Scene action";
        [Min(0f)] public float waitSeconds = 1f;
    }

    [CreateAssetMenu(menuName = "Project NTH/Dialogue/Conversation", fileName = "New Conversation")]
    public sealed class DialogueConversation : ScriptableObject
    {
        [SerializeField, HideInInspector] private string assetId;
        [SerializeField, HideInInspector] private string generatedHash;
        public List<DialogueStep> steps = new List<DialogueStep>();

        public string AssetId => assetId;
        public string NodeName => "Authored_" + assetId;
        public string GeneratedHash => generatedHash;
        public DialogueStep FindStep(string id) => steps.Find(step => step != null && step.id == id);

        private void OnValidate()
        {
            var ids = new HashSet<string>();
            foreach (var step in steps)
            {
                if (step == null) continue;
                if (string.IsNullOrEmpty(step.id) || !ids.Add(step.id))
                {
                    step.id = Guid.NewGuid().ToString("N");
                    ids.Add(step.id);
                }
            }
        }
    }
}
