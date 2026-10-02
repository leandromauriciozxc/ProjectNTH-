using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectActivator : MonoBehaviour
{
    
    public void HideObject()
    {
        this.gameObject.SetActive(false);
    }
}
