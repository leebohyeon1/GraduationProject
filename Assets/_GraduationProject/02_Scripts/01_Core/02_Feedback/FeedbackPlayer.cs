using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 피드백 설정을 위한 구조체입니다. 타입과 MMF_Player를 연결합니다.
/// </summary>
[Serializable]
public struct FeedbackConfig<T>
{
    public T type; // 피드백 타입
    public MMF_Player Feedback; // 실행할 MMF_Player
}

/// <summary>
/// 제네릭을 사용하여 다양한 타입의 피드백을 재생하는 기본 클래스입니다.
/// </summary>
[Serializable]
public class FeedbackPlayer<T> : MonoBehaviour
{
    [Header("Feedbacks")]
    [SerializeField] private List<FeedbackConfig<T>> _feedbacks; // 피드백 설정 리스트
    private Dictionary<T, MMF_Player> _feedbackDictionary = new Dictionary<T, MMF_Player>(); // 빠른 조회를 위한 딕셔너리

    protected virtual void Start()
    {
        // 리스트를 딕셔너리로 변환하여 초기화
        foreach (var feedbackPlayer in _feedbacks)
        {
            _feedbackDictionary[feedbackPlayer.type] = feedbackPlayer.Feedback;
        }
    }

    /// <summary>
    /// 지정된 타입의 피드백을 특정 위치에서 재생합니다.
    /// </summary>
    /// <param name="feedbackType">재생할 피드백 타입</param>
    /// <param name="position">재생 위치</param>
    public virtual void PlayFeedback(T feedbackType)
    {
        if (_feedbackDictionary.TryGetValue(feedbackType, out MMF_Player feedback))
        {
            if (feedback == null)
            {
                Debug.LogWarning($"피드백이 null입니다: {feedbackType}");
                return;
            }
            feedback.PlayFeedbacks();
        }
        else
        {
            Debug.LogWarning($"피드백을 찾을 수 없습니다: {feedbackType}");
        }
    }

    /// <summary>
    /// 지정된 타입의 피드백을 특정 위치에서 재생합니다.
    /// </summary>
    /// <param name="feedbackType">재생할 피드백 타입</param>
    /// <param name="position">재생 위치</param>
    public virtual void PlayFeedback(T feedbackType, Vector3 position)
    {
        if (_feedbackDictionary.TryGetValue(feedbackType, out MMF_Player feedback))
        {
            if (feedback == null)
            {
                Debug.LogWarning($"피드백이 null입니다: {feedbackType}");
                return;
            }
            feedback.PlayFeedbacks(position);
        }
        else
        {
            Debug.LogWarning($"피드백을 찾을 수 없습니다: {feedbackType}");
        }
    }

    /// <summary>
    /// 지정된 타입의 피드백 재생을 중지합니다.
    /// </summary>
    /// <param name="feedbackType">중지할 피드백 타입</param>
    public virtual void StopFeedback(T feedbackType)
    {
        if (_feedbackDictionary.TryGetValue(feedbackType, out MMF_Player feedback))
        {
            if (feedback == null)
            {
                Debug.LogWarning($"피드백이 null입니다: {feedbackType}");
                return;
            }
            feedback.StopFeedbacks();
        }
        else
        {
            Debug.LogWarning($"피드백을 찾을 수 없습니다: {feedbackType}");
        }
    }
}