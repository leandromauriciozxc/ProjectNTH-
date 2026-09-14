using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Attributes;

public class DialogueSystemTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField]
    private DialogueSystemController dialogueSystem;

    [SerializeField, HideInInspector]
    private YarnProject yarnProject;

    [YarnNode(nameof(yarnProject))]
    [SerializeField]
    private string yarnNode;

    [Header("Trigger")]
    [SerializeField]
    private bool triggerOnce = true;

    private bool hasTriggered;

    private void OnValidate()
    {
        if (dialogueSystem != null && dialogueSystem.DialogueRunner != null)
        {
            yarnProject = dialogueSystem.DialogueRunner.YarnProject;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        TriggerDialogue();
    }

    public void TriggerDialogue()
    {
        if (triggerOnce && hasTriggered)
            return;

        if (dialogueSystem == null)
        {
            Debug.LogWarning("Dialogue System is not assigned.",this);

            return;
        }

        if (string.IsNullOrEmpty(yarnNode))
        {
            Debug.LogWarning("No Yarn node has been selected.",this);

            return;
        }

        hasTriggered = true;

        dialogueSystem.StartDialogue(yarnNode);
    }
}