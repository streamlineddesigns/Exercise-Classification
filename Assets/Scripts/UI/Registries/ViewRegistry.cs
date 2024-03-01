using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewRegistry : MonoBehaviour
{
    protected Dictionary<ViewName, View> Registry = new Dictionary<ViewName, View>();
    public List<View> Views = new List<View>();

    public void Awake() 
    {
        for (int i = 0; i < Views.Count; i++) {
            View v = Views[i].GetComponent<View>();
            ViewName e = v.ViewName;
            addView(e, v);
        }
    }

    public View getView(ViewName e)
    {
        return Registry[e];
    }

    public void addView(ViewName e, View v)
    {
        Registry.Add(e, v);
    }
}