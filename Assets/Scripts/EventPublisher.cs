using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPublisher 
{
    public delegate void NetworkEvent(string name);

    public static event NetworkEvent OnNetworkChange;

    public static void PublishNetworkChange(string name)
    {
        OnNetworkChange(name);
    }
}