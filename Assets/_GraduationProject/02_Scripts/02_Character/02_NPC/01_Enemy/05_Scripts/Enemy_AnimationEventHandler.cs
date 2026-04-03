using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// Handles animation events, flags, and feedback playback for enemies.
/// </summary>
public class Enemy_AnimationEventHandler : MonoBehaviour
{
    /// <summary>
    /// True while an action is active.
    /// </summary>
    public bool IsActive { get; private set; }
    /// <summary>
    /// True while hit window is open.
    /// </summary>
    public bool IsHitWindowOpen { get; private set; }
    /// <summary>
    /// True when the action is finished.
    /// </summary>
    public bool IsActionFinished { get; private set; }
    /// <summary>
    /// True while sound window is active.
    /// </summary>
    public bool IsSound { get; private set; }
    /// <summary>
    /// True while super armor is active.
    /// </summary>
    public bool IsSuperArmor { get; private set; }
    /// <summary>
    /// True while ActionSO window is active.
    /// </summary>
    public bool IsActionSO { get; private set; }

    private Enemy _owner;
    private const int DefaultPlayerLayerMask = 1 << 9;

    public event Action KsanteAtk;


    /// <summary>
    /// Initializes owner reference and feedback dictionary.
    /// </summary>
    public void Initialize()
    {
        _owner = GetComponent<Enemy>();
        
        foreach (var feedbackPlayer in _feedbacks)
        {
            _feedbackDictionary[feedbackPlayer.name] = feedbackPlayer.feedback;
        }
    }

    public void KsanteKnockback()
    {
        KsanteAtk?.Invoke();
    }
    /// <summary>
    /// Marks action as active.
    /// </summary>
    public void ActivateAction()
    {
        IsActive = true;
    }
    /// <summary>
    /// Marks action as inactive.
    /// </summary>
    public void DeactivateAction()
    {
        IsActive = false;
    }
    /// <summary>
    /// Opens hit window if current phase allows it.
    /// </summary>
    public void OpenHitWindow(int phase = 0)
    {
        if(phase <= _owner._aiController._aiBrain.blackboard.GetValue<int>(EnemyBlackboardKeys.Phase))
        {
            IsHitWindowOpen = true;
        }
    }
    /// <summary>
    /// Closes hit window.
    /// </summary>
    public void CloseHitWindow()
    {
        IsHitWindowOpen = false;
    }
    /// <summary>
    /// Marks action as finished.
    /// </summary>
    public void FinishAction()
    {
        IsActionFinished = true;
    }
    /// <summary>
    /// Marks ActionSO window as active.
    /// </summary>
    public void ActionSO()
    {
        IsActionSO = true;
    }
    /// <summary>
    /// Marks ActionSO window as inactive.
    /// </summary>
    public void EndSO()
    {
        IsActionSO = false;
    }
    /// <summary>
    /// Marks sound window as active.
    /// </summary>
    public void StartSound()
    {
        IsSound = true;
    }

    /// <summary>
    /// Marks sound window as inactive.
    /// </summary>
    public void EndSound()
    {
        IsSound = false;
    }
    /// <summary>
    /// Marks super armor as active.
    /// </summary>
    public void StartSuperArmor()
    {
        IsSuperArmor = true;
    }
    /// <summary>
    /// Marks super armor as inactive.
    /// </summary>
    public void EndSuperArmor()
    {
        IsSuperArmor = false;
    }
    /// <summary>
    /// Resets all runtime flags.
    /// </summary>
    public void ResetAllFlags()
    {
        IsActive = false;
        IsHitWindowOpen = false;
        IsActionFinished = false;
        IsSound = false;
        IsActionSO = false;
    }

    [Serializable]
    /// <summary>
    /// Feedback configuration entry.
    /// </summary>
    public struct FeedbackPlayer
    {
        public string name;
        public MMF_Player feedback;
        public Vector3 offset;
        public int Phase;
        public AttackType HitType;
        [Header("Damage")]
        public float damageRadius;
        public EnemyAttackData attackData;
        public float damageDelay;
        public LayerMask targetMask;
    }

    [Header("Feedbacks")]
    [SerializeField] private List<FeedbackPlayer> _feedbacks;
    Dictionary<string, MMF_Player> _feedbackDictionary = new Dictionary<string, MMF_Player>();

    /// <summary>
    /// Plays feedbacks by name based on current phase.
    /// </summary>
    public void PlayFeedback(string feedbackName)
    {
        if (_owner == null) return;

        // 블랙보드에서 현재 Phase를 가져옵니다.
        int currentPhase = _owner._aiController._aiBrain.blackboard.GetValueOrDefault<int>(EnemyBlackboardKeys.Phase, 0);

        // 이름이 일치하고, ID(Phase)가 현재 페이즈보다 작거나 같은 모든 피드백을 재생합니다.
        foreach (var f in _feedbacks)
        {
            if (f.name == feedbackName && f.Phase <= currentPhase && f.feedback != null)
            {
                f.feedback.PlayFeedbacks(transform.position + f.offset);
            }
        }
    }

    /// <summary>
    /// 지정 위치에서 피드백을 재생하고, damageDelay 후 범위 데미지를 적용합니다.
    /// </summary>
    internal void PlayFeedbackAtPosition(string feedbackName, Vector3 position)
    {
        if (_owner == null) return;

        int currentPhase = _owner._aiController._aiBrain.blackboard.GetValueOrDefault<int>(EnemyBlackboardKeys.Phase, 0);

        for (int i = 0; i < _feedbacks.Count; i++)
        {
            FeedbackPlayer f = _feedbacks[i];
            if (f.name == feedbackName && f.Phase <= currentPhase && f.feedback != null)
            {
                Vector3 spawnPos = position + f.offset;
                f.feedback.PlayFeedbacks(spawnPos);

                if (f.attackData != null && f.damageDelay >= 0f)
                {
                    StartCoroutine(DealDamageAfterDelay(spawnPos, f));
                }
            }
        }
    }



    /// <summary>
    /// damageDelay 후 OverlapSphere로 범위 내 IDamageable에게 데미지를 적용합니다.
    /// </summary>
    private IEnumerator DealDamageAfterDelay(Vector3 position, FeedbackPlayer feedbackPlayer)
    {
        if (feedbackPlayer.damageDelay > 0f)
        {
            yield return new WaitForSeconds(feedbackPlayer.damageDelay);
        }

        Collider[] hitColliders = ResolveHitTargets(position, feedbackPlayer);
        if (hitColliders.Length == 0)
        {
            yield break;
        }

        DamageData baseDamage = feedbackPlayer.attackData.damageData;
        for (int i = 0; i < hitColliders.Length; i++)
        {
            IDamageable damageable = hitColliders[i].GetComponent<IDamageable>();
            if (damageable == null)
            {
                continue;
            }

            DamageData dmg = baseDamage;
            dmg.AttackerTransform = _owner.transform;
            damageable.TakeDamage(dmg);
        }
    }

    private Collider[] ResolveHitTargets(Vector3 position, FeedbackPlayer feedbackPlayer)
    {
        EnemyAttackData attackData = feedbackPlayer.attackData;
        LayerMask mask = GetEffectiveTargetMask(feedbackPlayer.targetMask);
        float radius = GetEffectiveRadius(feedbackPlayer, attackData);

        if (attackData == null)
        {
            return radius > 0f ? Physics.OverlapSphere(position, radius, mask) : Array.Empty<Collider>();
        }

        switch (attackData.shape)
        {
            case AttackShape.Box:
                return attackData.boxSize == Vector3.zero ? Array.Empty<Collider>() : Physics.OverlapBox(position, attackData.boxSize * 0.5f, _owner.transform.rotation, mask);
            case AttackShape.Fan:
                return ResolveFanHits(position, mask, radius, attackData.fanAngle);
            default:
                return radius > 0f ? Physics.OverlapSphere(position, radius, mask) : Array.Empty<Collider>();
        }
    }

    private LayerMask GetEffectiveTargetMask(LayerMask configuredMask)
    {
        return configuredMask.value == 0 ? (LayerMask)DefaultPlayerLayerMask : configuredMask;
    }

    private float GetEffectiveRadius(FeedbackPlayer feedbackPlayer, EnemyAttackData attackData)
    {
        if (feedbackPlayer.damageRadius > 0f)
        {
            return feedbackPlayer.damageRadius;
        }

        return attackData != null ? attackData.damageRadius : 0f;
    }

    private Collider[] ResolveFanHits(Vector3 position, LayerMask mask, float radius, float fanAngle)
    {
        if (radius <= 0f)
        {
            return Array.Empty<Collider>();
        }

        Collider[] rawHits = Physics.OverlapSphere(position, radius, mask);
        if (rawHits.Length == 0)
        {
            return rawHits;
        }

        List<Collider> validHits = new List<Collider>(rawHits.Length);
        float halfAngle = Mathf.Max(0f, fanAngle) * 0.5f;
        Vector3 forward = _owner.transform.forward;

        for (int i = 0; i < rawHits.Length; i++)
        {
            Collider collider = rawHits[i];
            Vector3 direction = collider.transform.position - position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                validHits.Add(collider);
                continue;
            }

            float angle = Vector3.Angle(forward, direction);
            if (angle <= halfAngle)
            {
                validHits.Add(collider);
            }
        }

        return validHits.ToArray();
    }

    /// <summary>
    /// Plays feedbacks by name and attack type based on current phase.
    /// </summary>
    public void PlayFeedback(string feedbackName, AttackType attackType)
    {
        if (_owner == null) return;

        int currentPhase = _owner._aiController._aiBrain.blackboard.GetValueOrDefault<int>("Phase", 0);

        // 이름, 타입이 일치하고 ID가 현재 페이즈 이하인 모든 피드백을 재생합니다.
        foreach (var f in _feedbacks)
        {
            if (f.name == feedbackName && f.HitType == attackType && f.Phase <= currentPhase && f.feedback != null)
            {
                // Debug.Log($"[피드백 재생 성공] 이름: {feedbackName}, 타입: {attackType}, Phase: {f.id}");
                f.feedback.PlayFeedbacks(transform.position + f.offset);
            }
        }
    }

    /// <summary>
    /// Stops feedbacks by name for all phases.
    /// </summary>
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
