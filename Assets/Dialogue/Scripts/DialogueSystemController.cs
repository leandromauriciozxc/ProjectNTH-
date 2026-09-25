using UnityEngine;
using Yarn.Unity;

public class DialogueSystemController : MonoBehaviour
{
    [Header("Yarn")]
    [SerializeField]
    private DialogueRunner dialogueRunner;

    [Header("Components Disabled During Dialogue")]
    [SerializeField]
    private MonoBehaviour[] componentsToDisable;

    [Header("GameObjects Hidden During Dialogue")]
    [SerializeField]
    private GameObject[] objectsToHide;

    private bool[] previousComponentStates;
    private bool[] previousObjectStates;

    public DialogueRunner DialogueRunner => dialogueRunner;

    public void StartDialogue(string nodeName)
    {
        if (dialogueRunner.IsDialogueRunning)
            return;

        DisableGameplay();

        dialogueRunner.StartDialogue(nodeName);
    }

    public void DisableGameplay()
    {
        previousComponentStates = new bool[componentsToDisable.Length];

        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            if (componentsToDisable[i] == null)
                continue;

            previousComponentStates[i] = componentsToDisable[i].enabled;

            componentsToDisable[i].enabled = false;
        }

        previousObjectStates = new bool[objectsToHide.Length];

        for (int i = 0; i < objectsToHide.Length; i++)
        {
            if (objectsToHide[i] == null)
                continue;

            previousObjectStates[i] = objectsToHide[i].activeSelf;

            objectsToHide[i].SetActive(false);
        }
    }

    public void EnableGameplay()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            if (componentsToDisable[i] == null)
                continue;

            componentsToDisable[i].enabled = previousComponentStates[i];
        }

        for (int i = 0; i < objectsToHide.Length; i++)
        {
            if (objectsToHide[i] == null)
                continue;

            objectsToHide[i].SetActive(previousObjectStates[i]);
        }
    }
}