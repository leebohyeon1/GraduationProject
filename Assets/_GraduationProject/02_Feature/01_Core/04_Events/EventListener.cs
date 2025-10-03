using UnityEngine;
using UnityEngine.Events;

public class EventListener : MonoBehaviour
{
    public EventSO Event;

    public UnityEvent EventMessage;

    private void OnEnable()
    {
        Event?.Subscribe(this);
    }

    private void OnDisable()
    {
        Event?.Unsubscribe(this);
    }

    public void OnEventTrigger()
    {
        EventMessage.Invoke();
    }
}