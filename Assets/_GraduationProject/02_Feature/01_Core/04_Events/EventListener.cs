using UnityEngine;
using UnityEngine.Events;

public class EventListener : MonoBehaviour
{
    public EventSO Event;

    public UnityEvent<GameObject> EventMessage;

    private void OnEnable()
    {
        Event?.Subscribe(this);
    }

    private void OnDisable()
    {
        Event?.Unsubscribe(this);
    }

    public void OnEventTrigger(GameObject gameObject)
    {
        EventMessage.Invoke(gameObject);
    }
}