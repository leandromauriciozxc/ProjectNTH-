using ProjectNTH.UI;
using System.Collections;
using System.Collections.Generic;
using Yarn.Unity;
using UnityEngine;

public class YarnSpinnerTaskCommandHandler : MonoBehaviour
{
    [SerializeField] private TaskPanelUI taskHandler;

    [YarnCommand("show_task")]
    public void ShowCompleteTask(string nextTask)
    {
        if(taskHandler != null) { taskHandler.CompleteAndShowNext(nextTask); }
        else { Debug.LogWarning("attach task handler first"); }
    }
}
