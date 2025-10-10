using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Event", menuName = "Events/Void Event")]
public class EventSO : ScriptableObject
{
    private List<EventListener> _listeners = new List<EventListener>();

    public void Subscribe(EventListener listener)
    {
        _listeners.Add(listener);
    }

    public void Unsubscribe(EventListener listener) 
    {
        _listeners.Remove(listener);
    }

    public void Publish(GameObject owner)
    {
        foreach (var listener in _listeners)
        {
            listener.OnEventTrigger(owner);
        }
    }
}

