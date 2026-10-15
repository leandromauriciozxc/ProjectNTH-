using System;
using UnityEngine;
using Yarn.Unity;

namespace ProjectNTH.Dialogue.Authoring
{
    /// <summary>One runtime command registration per runner, shared by every authored conversation.</summary>
    [DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class AuthoredDialogueBridge : MonoBehaviour
    {
        private DialogueRunner runner;
        private DialogueAssetPlayer owner;
        private global::DialogueSystemController gameplay;
        private bool gameplayDisabled;
        private int revision;
        private bool stepRunning;
        private bool stopRequested;

        private void Awake()
        {
            runner = GetComponent<DialogueRunner>();
            if (runner != null) runner.AddCommandHandler<string>(DialogueYarnText.Command, RunStep);
        }

        internal bool TryBegin(DialogueAssetPlayer player)
        {
            if (!isActiveAndEnabled || runner == null || owner != null || runner.IsDialogueRunning) return false;
            owner = player;
            gameplay = player.DialogueSystem;
            stopRequested = false;
            Run(player.Conversation.NodeName, ++revision).Forget();
            return true;
        }

        private async YarnTask Run(string node, int runRevision)
        {
            try
            {
                // Yarn's cancelled line callbacks finish on the next player-loop update.
                // Reserve this runner immediately, then let those callbacks drain before starting.
                int frame = Time.frameCount;
                do { await YarnTask.Yield(); } while (Time.frameCount == frame);
                if (stopRequested || owner == null || !owner.isActiveAndEnabled) return;
                gameplay.DisableGameplay();
                gameplayDisabled = true;
                await runner.StartDialogue(node);
                await runner.DialogueTask;
            }
            catch (Exception exception)
            {
                if (!(exception is OperationCanceledException)) Debug.LogException(exception, this);
                if (runner != null && runner.IsDialogueRunning) await runner.Stop();
            }
            finally { if (runRevision == revision) Release(); }
        }

        private async YarnTask RunStep(string id)
        {
            stepRunning = true;
            try
            {
                if (owner == null || !owner.isActiveAndEnabled)
                {
                    if (!stopRequested) Debug.LogError("Start generated conversations through Dialogue Asset Player.PlayConversation(), not a legacy node trigger.", this);
                    stopRequested = true;
                }
                if (!stopRequested) await owner.ExecuteStep(id);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                stopRequested = true;
            }
            finally
            {
                if (runner != null) runner.VariableStorage.SetValue(DialogueYarnText.StopVariable, stopRequested);
                stepRunning = false;
            }
        }

        internal void StopOwnedConversation(DialogueAssetPlayer player)
        {
            if (owner != player) return;
            stopRequested = true;
            owner.StopAudio();
            owner.CompleteWait();
            // An active command finishes through the generated stop guard. Stopping the VM while
            // an async command is pending can otherwise resume an obsolete command in Yarn 3.2.
            if (!stepRunning && runner != null && runner.IsDialogueRunning) runner.Stop().Forget();
        }

        private void Release()
        {
            if (owner != null) owner.StopAudio();
            if (gameplayDisabled && gameplay != null) gameplay.EnableGameplay();
            gameplayDisabled = false;
            gameplay = null;
            owner = null;
        }

        private void OnDisable()
        {
            if (owner == null) return;
            StopOwnedConversation(owner);
            revision++;
            Release();
        }

        private void OnDestroy()
        {
            if (runner != null) runner.RemoveCommandHandler(DialogueYarnText.Command);
        }
    }
}
