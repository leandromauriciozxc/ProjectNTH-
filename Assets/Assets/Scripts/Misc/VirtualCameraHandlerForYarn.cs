using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Yarn.Unity;

public class VirtualCameraHandlerForYarn : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cinemachineCam;

    [YarnCommand("hide_gameobject")]
    public void VcamDisabler()
    {
        cinemachineCam.gameObject.SetActive(false);
    }
}
