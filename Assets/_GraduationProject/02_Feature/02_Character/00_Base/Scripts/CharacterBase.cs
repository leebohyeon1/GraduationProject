using System;
using System.Collections.Generic;
using BH_Lib.DI;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// 모든 캐릭터의 기본 클래스
/// IDamageable 인터페이스를 구현하여 체력과 피해 시스템을 제공
/// </summary>
public class CharacterBase : DIMonoBehaviour
{
    [Serializable]
    public struct FeedbackPlayer
    {
        public string name;
        public MMF_Player feedback;
    }
    
   protected override void Awake()
    {
        base.Awake();

        foreach (var feedbackPlayer in _feedbacks)
        {
            _feedbackDictionary[feedbackPlayer.name] = feedbackPlayer.feedback;
        }
    }

    [Header("Feedbacks")]
    [SerializeField] private List<FeedbackPlayer> _feedbacks;
    Dictionary<string, MMF_Player> _feedbackDictionary = new Dictionary<string, MMF_Player>();
    public void PlayFeedback(string feedbackName, Vector3 position)
    {
        if (_feedbackDictionary.TryGetValue(feedbackName, out MMF_Player feedback))
        {
            feedback.PlayFeedbacks(position);
        }
        else
        {
            Debug.LogWarning($"피드백 등록안됨 {feedbackName}");
            Debug.Log(_feedbackDictionary.Count);
            foreach (var item in _feedbackDictionary)
            {
                Debug.Log(item.Key);
            }
        }
    }
    public void PlayFeedbackSound(string feedbackName)
    {
        if (_feedbackDictionary.TryGetValue(feedbackName, out MMF_Player feedback))
        {
            feedback.PlayFeedbacks(transform.position);
        }
        else
        {
            Debug.LogWarning($"피드백 등록안됨 {feedbackName}");
        }
    }
}
