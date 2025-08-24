using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    private Dictionary<string, Action<GameEvent>> _listeners = new();

    public void Subscribe(string eventName, Action<GameEvent> callback)
    {
        if (!_listeners.ContainsKey(eventName))
            _listeners[eventName] = delegate { };

        _listeners[eventName] += callback;
    }

    public void Unsubscribe(string eventName, Action<GameEvent> callback)
    {
        if (_listeners.ContainsKey(eventName))
            _listeners[eventName] -= callback;
    }

    public void Trigger(string eventName, object payload = null)
    {
        if (_listeners.ContainsKey(eventName))
            _listeners[eventName]?.Invoke(new GameEvent(eventName, payload));
    }
}

public class GameEvent
{
    public string Name;
    public object Payload;

    public GameEvent(string name, object payload = null)
    {
        Name = name;
        Payload = payload;
    }
}
