using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Attributes;

namespace ProjectNTH.Dialogue
{
    [AddComponentMenu("Project NTH/Dialogue/Thought Dialogue Trigger")]
    public sealed class ThoughtDialogueTrigger : MonoBehaviour
    {
        [SerializeField] private ThoughtDialogueSystem thoughtSystem;
        [SerializeField, HideInInspector] private YarnProject yarnProject;
        [SerializeField, YarnNode(nameof(yarnProject))] private string yarnNode;
        [SerializeField] private bool triggerOnce = true;
        private bool hasTriggered;

        private void OnValidate() => yarnProject = thoughtSystem != null ? thoughtSystem.YarnProject : null;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) TriggerThought();
        }

        public void TriggerThought()
        {
            if (triggerOnce && hasTriggered) return;
            if (thoughtSystem == null)
            {
                Debug.LogWarning("Assign the Thought Dialogue system before triggering a thought.", this);
                return;
            }
            // Failed setup must not consume a one-shot trigger.
            if (thoughtSystem.TryPlayThought(yarnNode)) hasTriggered = true;
        }
    }
}
