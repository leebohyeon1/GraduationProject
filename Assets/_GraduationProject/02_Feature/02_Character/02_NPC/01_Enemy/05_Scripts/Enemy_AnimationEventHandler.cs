using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
public class Enemy_AnimationEventHandler : MonoBehaviour
{
    public bool IsActive { get; private set; }
    public bool IsHitWindowOpen { get; private set; }
    public bool IsActionFinished { get; private set; }
    public bool IsSound { get; private set; }
    public bool IsSuperArmor { get; private set; }
    public void Initalize()
    {
         foreach (var feedbackPlayer in _feedbacks)
        {
            _feedbackDictionary[feedbackPlayer.name] = feedbackPlayer.feedback;
        }
    }

    public void OpenHitWindow()
    {
        IsHitWindowOpen = true;
    }
    public void CloseHitWindow()
    {
        IsHitWindowOpen = false;
    }
    public void FinishAction()
    {
        IsActionFinished = true;
    }

    public void StartSound()
    {
        IsSound = true;
    }

    public void EndSound()
    {
        IsSound = false;
    }
    public void StartSuperArmor()
    {
        IsSuperArmor = true;
    }
    public void EndSuperArmor()
    {
        IsSuperArmor = false;
    }
    public void ResetAllFlags()
    {
        IsActive = false;
        IsHitWindowOpen = false;
        IsActionFinished = false;
        IsSound = false;
    }
    [Serializable]
    public struct FeedbackPlayer
    {
        public string name;
        public MMF_Player feedback;
        public Vector3 offset;
        public int id;
    }



    [Header("Feedbacks")]
    [SerializeField] private List<FeedbackPlayer> _feedbacks;
    Dictionary<string, MMF_Player> _feedbackDictionary = new Dictionary<string, MMF_Player>();
    public void PlayFeedback(string feedbackName)
    {
        if (_feedbackDictionary.TryGetValue(feedbackName, out MMF_Player feedback))
        {
            if( feedback == null)
            {
                Debug.LogWarning($"피드백이 null {feedbackName}");
                return;
            }
            feedback.PlayFeedbacks(transform.position + _feedbacks.Find(f => f.name == feedbackName).offset);
        }
        else
        {
            Debug.LogWarning($"피드백 등록안됨 {feedbackName}");
        }
    }
    // public void PlayFeedbackSound(string feedbackName)
    // {
    //     if (_feedbackDictionary.TryGetValue(feedbackName, out MMF_Player feedback))
    //     {
    //         if( feedback == null)
    //         {
    //             Debug.LogWarning($"피드백이 null {feedbackName}");
    //             return;
    //         }

    //         feedback.PlayFeedbacks(transform.position);
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"피드백 등록안됨 {feedbackName}");
    //     }
    // }
    
    public void StopFeedback(string feedbackName)
    {
        if (_feedbackDictionary.TryGetValue(feedbackName, out MMF_Player feedback))
        {
            if( feedback == null)
            {
               Debug.LogWarning($"피드백이 null {feedbackName}");
                return;
            }
            
            feedback.StopFeedbacks();
        }
        else
        {
            Debug.LogWarning($"피드백 등록안됨 {feedbackName}");
        }
    }
}