using UnityEngine;
using UnityEngine.Events;

public interface IEventListener<T>
{
    void OnEventTrigger(T eventName);
}

public class EventListener<T> : MonoBehaviour, IEventListener<T>
{
    public EventSO<T> Event;

    public UnityEvent<T> EventMessage;

    private void OnEnable()
    {
        Event?.Subscribe(this);
    }

    private void OnDisable()
    {
        Event?.Unsubscribe(this);
    }

    public void OnEventTrigger(T value)
    {
        EventMessage.Invoke(value);
    }
}