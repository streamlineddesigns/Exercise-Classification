using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    public ViewName ViewName;

    protected void OnEnable()
    {
        AppManager.Singleton.ControllerRegistry.addController(ViewName, this);
    }
}