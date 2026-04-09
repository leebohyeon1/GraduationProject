using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 스크립터블 이벤트를 구독할 인터페이스
/// </summary>
/// <typeparam name="T">전달할 데이터 타입</typeparam>
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