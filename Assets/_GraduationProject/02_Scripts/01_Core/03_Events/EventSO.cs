using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스크립터블 오브젝트 제네릭 이벤트 
/// </summary>
/// <typeparam name="T">전달할 데이터 타입</typeparam>
public class EventSO<T> : ScriptableObject
{
    private List<IEventListener<T>> _listeners = new List<IEventListener<T>>();

    /// <summary>
    /// 구독할 리스너 추가
    /// </summary>
    /// <param name="listener">이벤트를 구독할 리스너</param>
    public void Subscribe(IEventListener<T> listener)
    {
        // 중복 방지
        if(!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
    }

    /// <summary>
    /// 구독 중인 리스너 제거.
    /// </summary>
    /// <param name="listener">구독 취소할 리스너</param>
    public void Unsubscribe(IEventListener<T> listener)
    {
        if (_listeners.Contains(listener))
        {
            _listeners.Remove(listener);
        }
    }

    /// <summary>
    /// 구독 중인 모든 리스너에 이벤트를 즉시 발행
    /// </summary>
    /// <param name="value">전달할 데이터</param>
    public void Publish(T value)
    {
        foreach (var listener in _listeners)
        {
            listener.OnEventTrigger(value);
        }
    }

    /// <summary>
    /// 지정된 시간 후에 이벤트를 발행.
    /// </summary>
    /// <param name="value">전달할 데이터</param>
    /// <param name="delay">지연 시간(초)</param>
    public void Publish(T value, float delay)
    {
        if (delay > 0)
        {
            // delay 시간만큼 대기한 후 Publish
            DOVirtual.DelayedCall(delay, () => Publish(value));
        }
        else
        {
            // 즉시 발행
            Publish(value);
        }
    }
}