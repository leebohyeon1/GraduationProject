using DG.Tweening;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Events;

public class EventSO<T> : ScriptableObject
{
    private List<IEventListener<T>> _listeners = new List<IEventListener<T>>();

    public void Subscribe(IEventListener<T> listener)
    {
        _listeners.Add(listener);
    }

    public void Unsubscribe(IEventListener<T> listener) 
    {
        _listeners.Remove(listener);
    }

   public void Publish(T value)
    {
        foreach (var listener in _listeners)
        {
            listener.OnEventTrigger(value);
        }
    }

    public void Publish(T value, float delay)
    {
        if (delay > 0)
        {
            DOVirtual.DelayedCall(delay, () => Publish(value));
        }
        else
        {
            Publish(value);
        }
    }
}

