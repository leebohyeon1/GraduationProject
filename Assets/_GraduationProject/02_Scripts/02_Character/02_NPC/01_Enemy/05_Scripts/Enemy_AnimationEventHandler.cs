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

    private Enemy _owner;

    public void Initialize()
    {
        _owner = GetComponent<Enemy>();
        
        foreach (var feedbackPlayer in _feedbacks)
        {
            _feedbackDictionary[feedbackPlayer.name] = feedbackPlayer.feedback;
        }
    }

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
        IsActionSO = false;
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
        if (_owner == null) return;

        // 블랙보드에서 현재 Phase를 가져옵니다.
        int currentPhase = _owner._aiController._aiBrain.blackboard.GetValueOrDefault<int>("Phase", 0);

        // 이름이 일치하고, ID(Phase)가 현재 페이즈보다 작거나 같은 모든 피드백을 재생합니다.
        foreach (var f in _feedbacks)
        {
            if (f.name == feedbackName && f.id <= currentPhase && f.feedback != null)
            {
                f.feedback.PlayFeedbacks(transform.position + f.offset);
            }
        }
    }

    public void PlayFeedback(string feedbackName, AttackType attackType)
    {
        if (_owner == null) return;

        int currentPhase = _owner._aiController._aiBrain.blackboard.GetValueOrDefault<int>("Phase", 0);

        // 이름, 타입이 일치하고 ID가 현재 페이즈 이하인 모든 피드백을 재생합니다.
        foreach (var f in _feedbacks)
        {
            if (f.name == feedbackName && f.HitType == attackType && f.id <= currentPhase && f.feedback != null)
            {
                // Debug.Log($"[피드백 재생 성공] 이름: {feedbackName}, 타입: {attackType}, Phase: {f.id}");
                f.feedback.PlayFeedbacks(transform.position + f.offset);
            }
        }
    }

    public void StopFeedback(string feedbackName)
    {
        // 중지는 이름 기준으로만 처리 (모든 Phase의 해당 이름 피드백 중지)
        foreach (var f in _feedbacks)
        {
            if (f.name == feedbackName && f.feedback != null)
            {
                f.feedback.StopFeedbacks();
            }
        }
    }
}
