using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerRegistry : MonoBehaviour
{
    protected Dictionary<ViewName, Controller> Registry = new Dictionary<ViewName, Controller>();

    public Controller getController(ViewName e)
    {
        return Registry[e];
    }

    public void addController(ViewName v, Controller c)
    {
        Registry.Add(v, c);
    }

}