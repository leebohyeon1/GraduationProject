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
    public bool IsActionSO { get; private set; }

    public void ActivateAction()
    {
        IsActive = true;
    }
    public void DeactivateAction()
    {
        IsActive = false;
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
    public void ActionSO()
    {
        IsActionSO = true;
    }
    public void EndSO()
    {
        IsActionSO = false;
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
        IsActionSO = false; // [추가]
    }
    public void Initalize()
    {
         foreach (var feedbackPlayer in _feedbacks)
        {
            _feedbackDictionary[feedbackPlayer.name] = feedbackPlayer.feedback;
        }
    }
    [Serializable]
    public struct FeedbackPlayer
    {
        public string name;
        public MMF_Player feedback;
        public Vector3 offset;
        public int id;
        public AttackType HitType;
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
        public void PlayFeedback(string feedbackName, AttackType attackType)
    {
        var target = _feedbacks.Find(f => f.name == feedbackName && f.HitType == attackType);
        
        if (target.feedback != null)
        {
            Debug.Log($"[피드백 재생 성공] 이름: {feedbackName}, 타입: {target.HitType}");
            
            // 3. 해당 데이터의 피드백을 실행하고 설정된 오프셋을 적용합니다.
            target.feedback.PlayFeedbacks(transform.position + target.offset);
        }
        else
        {
            // 4. 리스트에 없거나 피드백이 할당되지 않은 경우에 대한 예외 처리
            Debug.LogWarning($"[피드백 실패] {feedbackName} (타입: {attackType})를 찾을 수 없거나 피드백이 null입니다.");
        }
    }

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
