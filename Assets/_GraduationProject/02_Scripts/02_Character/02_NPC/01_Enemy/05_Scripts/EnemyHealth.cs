using System;
using System.Collections;
using Pathfinding;
using UnityEngine;
using FIMSpace.FProceduralAnimation;

/// <summary>
/// 몬스터의 체력 관리 및 피해 처리를 담당하는 컴포넌트입니다.
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyStat enemyStat;
    public ImmunityLevel _currentImmunityLevel = ImmunityLevel.None;

    public int CurrentHealth => curHealth;
    public int MaxHealth => enemyStat.Maxhealth;

    private int _maxHealth = 100;
    private int curHealth = 100;

    public bool IsDead => CurrentHealth <= 0;
    public event Action<int, int> OnHealthChanged;
    public Action<bool> OnRecoveryHealth;
    public int healthPerSecond = 20;

    private Coroutine _recoveryCoroutine;
    public event Action OnDied;
    private CharacterController _characterController;
    private Coroutine _KnockbackCoroutine;

    public Action<AttackType> OnDamageReceived;
    public Func<AttackType, bool> CheckStunImmunity;
  
    private Enemy _owner;
    public bool ImmunityStart = false;

    private RagdollAnimator2 _ragdollAnimator;
    private static readonly RaycastHit[] KnockbackSweepHits = new RaycastHit[32];
    private static readonly Collider[] KnockbackOverlapHits = new Collider[32];

    public void InitializeHealth(Enemy owner, EnemyStatMultiplier statMultiplier = default)
    {
        _owner = owner;
        _maxHealth = enemyStat.Maxhealth;
        _maxHealth = (int)(_maxHealth * (statMultiplier?.HealthMultiply ?? 1f));
        curHealth = _maxHealth;
        
        if (_owner.animator != null)
        {
            _owner.animator.enabled = true;
            if (_owner.animator.GetBool("Die"))
                _owner.animator.SetBool("Die", false);
        }
        
        _characterController = _owner.GetComponent<CharacterController>();
        _ragdollAnimator = GetComponentInChildren<RagdollAnimator2>();
        if (_ragdollAnimator == null)
            Debug.LogWarning($"[EnemyHealth] RagdollAnimator2 is missing on {_owner.name} or its children!");

        SetKnockbackable(true);
        _owner.tag = "Enemy";
        _currentImmunityLevel = ImmunityLevel.None;
        if(ImmunityStart) _currentImmunityLevel = ImmunityLevel.Minor;
        CheckStunImmunity = IsImmuneToHitReaction;
    }

    public void SetImmunityLevel(ImmunityLevel level)
    {
        if (level != _currentImmunityLevel)
            _currentImmunityLevel = level;
        if(level == ImmunityLevel.None && _delayCoroutine != null)
        {
            StopCoroutine(_delayCoroutine);
            _delayCoroutine = null;
        }
    }

    private bool IsImmuneToHitReaction(AttackType incomingAttackType)
    {
        if (_currentImmunityLevel == ImmunityLevel.Minor)
        {
            if (incomingAttackType <= AttackType.Normal_3)
                return true;
            
        }
        if (_currentImmunityLevel == ImmunityLevel.Major)
        {
            if (incomingAttackType == AttackType.Normal_Counter || incomingAttackType <= AttackType.Normal_3) return true;
        }
        return false;
    }

    private void OnDisable()
    {
        OnRecoveryHealth -= SetRecovery;
    }

    public void SetRecovery(bool isRecovering)
    {
        if (isRecovering)
        {
            if (_recoveryCoroutine == null) _recoveryCoroutine = StartCoroutine(RecoveryRoutine());
        }
        else
        {
            if (_recoveryCoroutine != null) { StopCoroutine(_recoveryCoroutine); _recoveryCoroutine = null; }
        }
    }

    private IEnumerator RecoveryRoutine()
    {
        while (curHealth < _maxHealth)
        {
            curHealth += (int)(healthPerSecond * Time.deltaTime);
            if (curHealth > _maxHealth) curHealth = _maxHealth;
            yield return null;
        }
        _recoveryCoroutine = null;
    }

    public void Attack(IDamageable target) {}

    /// <summary>넉백 가능 여부 프로퍼티입니다.</summary>
    public bool Knockbackable { get; private set; } = true;

    /// <summary>넉백 가능 여부를 설정합니다. (기존 API 복구)</summary>
    public void SetKnockbackable(bool value)
    {
        Knockbackable = value;
    }


    public void Die(DamageData? damageData)
    {
        OnDied?.Invoke();
        _owner.animHandler.PlayFeedback("Die");
        if (_owner.player != null && _owner.player.Money != null)
            _owner.player.Money.GiveMoney(_owner.GetMyCurrentReward());
        _owner.enemyStat.RewardSO?.RemoveMoneyFromEnemies(_owner.MonsterId);
        _owner.animator.SetBool("Die", true);
        _owner.animator.speed = 1;
        _owner.Movement.StopMovement();
    
        _owner.StateType = EnemyStateType.Dead;
        _owner.SetState(EnemyStateController.EnemyState.Die);
        _owner.groupAi.GroupRemove(_owner);
        _owner.tag = "DeadEnemy";
        
        if(TryGetComponent<LockOnTarget>(out var lockOnTarget))
            lockOnTarget.TriggerLockReleased();

        if (_characterController != null)
            _characterController.enabled = false;

        if (_ragdollAnimator != null)
        {
            _ragdollAnimator.User_SwitchFallState();
            if (damageData != null && damageData.Value.AttackerTransform != null)
            {
                // 기존 넉백 코루틴과 동일하게 수평 방향을 기본으로 하되, 
                // 렉돌이 바닥에 걸리지 않도록 아주 살짝만 위(0.15)로 띄웁니다.
                Vector3 impactDir = (transform.position - damageData.Value.AttackerTransform.position).normalized;
                impactDir.y = 0.15f; 
                impactDir.Normalize();
                
                float force = damageData.Value.DeathKnockbackForce;
                Vector3 velocity = impactDir * force;

                // 즉시 속도를 부여하고, 기존 넉백 지속시간(Duration)만큼 힘을 유지하여 동일한 느낌을 줍니다.
                _ragdollAnimator.User_SetAllBonesVelocity(velocity);
                _ragdollAnimator.User_AddAllBonesImpact(velocity, damageData.Value.DeathKnockbackDuration);
            }
        }

        StartCoroutine(ReturnToPoolRoutine(3f));
    }

    private IEnumerator ReturnToPoolRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!string.IsNullOrEmpty(_owner.MonsterPrefabName))
            MonsterPoolManager.Instance.ReleaseMonster(_owner.MonsterPrefabName, _owner);
        else
            Destroy(_owner.gameObject);
    }

    private IEnumerator ActivateImmunityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!_owner.EnemyHealth.IsDead) _owner.ParrySystem.ActivateMinorImmunity();
        Debug.Log($"[EnemyHealth] Activated minor immunity for {_owner.name} after taking damage.");
    }

    public float MinorTime = 5;
    private Coroutine _delayCoroutine;

    public void TakeDamage(DamageData damageData)
    {
        if (_owner.Interact != null && !_owner.Interact._isInteracted)
        {
            return;
        }
        if (CurrentHealth <= 0) return;
        bool isBlocked = false;
        int finalDamage = damageData.DamageAmount;
        _owner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.LastTakeHitTime, Time.time);  
        if(_owner.Shield != null && damageData.AttackerTransform != null)
        {
            if (_owner.Shield.CheckBlock(damageData.AttackerTransform.position))
            {
                isBlocked = true;
                float damageMultiplier = 1.0f - _owner.Shield.DamageReduction;
                finalDamage = Mathf.RoundToInt(damageData.DamageAmount * damageMultiplier);
            }
        }
        if (!_owner._aiController._aiBrain._isCombat)
        {
            _owner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.Engage, true);
            _owner.groupAi.EngageCombatAll();
            _owner._aiController.CombatEnter();
        }
        if (_delayCoroutine == null && !IsImmune(damageData.AttackType) && _owner.ParrySystem.CurrentStun != StunType.Full)
            _delayCoroutine = StartCoroutine(ActivateImmunityAfterDelay(MinorTime));
        
        bool isImmune = IsImmune(damageData.AttackType);
        if(!isImmune) OnDamageReceived?.Invoke(damageData.AttackType);
            // Debug.Log($"Damage Taken: {finalDamage} (Blocked: {isBlocked}, Immune: {isImmune})");
        
        if (!isImmune && !isBlocked )
        {
            // 강스턴(Full)이 아닐 때만 Hit 상태 전환 허용
            if (!_owner._aiController.IsActionable() || _owner.ParrySystem.CurrentStun == StunType.Weak)
            {
                _owner.SetState(EnemyStateController.EnemyState.Hit);
                _owner._aiController._aiBrain.blackboard.SetValue("OnTaskHit", Time.time);
                _owner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, true);
                
                // 약스턴 중이라면 애니메이터 트리거를 명시적으로 호출하여 WeakStun 애니메이션을 끊어줌
                if (_owner.ParrySystem.CurrentStun == StunType.Weak)
                {
                    _owner.animator.SetTrigger("Hit");
                }
                
                Debug.Log($"[EnemyHealth] Enemy hit during {_owner.ParrySystem.CurrentStun} stun! Current Health: {CurrentHealth}");
            }
        }

        int previousHealth = curHealth;
        _owner.animHandler.PlayFeedback(isBlocked ? "Block_FB" : "Damage_FB", damageData.AttackType);

        curHealth -= finalDamage;
        OnHealthChanged?.Invoke(previousHealth, curHealth);
        _owner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.SelfHealth, curHealth);
        OnRecoveryHealth?.Invoke(false);

        if (Knockbackable && !isBlocked || curHealth <= 0)
        {
            Vector3 knockbackDir = damageData.AttackerTransform != null
                ? (transform.position - damageData.AttackerTransform.position).normalized
                : -transform.forward;
            knockbackDir.y = 0;
            if (_KnockbackCoroutine != null)
            {
                StopCoroutine(_KnockbackCoroutine);
                _KnockbackCoroutine = null;
            }
            
            if (curHealth <= 0 && _ragdollAnimator != null)
            {
                // Ragdoll will handle the physics impact
            }
            else if (CanStartKnockback(knockbackDir, damageData.KnockbackForce, damageData.KnockbackDuration, damageData.KnockbackCurve))
            {
                _KnockbackCoroutine = StartCoroutine(KnockbackCoroutine(knockbackDir, damageData));
            }
            else
            {
                _KnockbackCoroutine = null;
            }
        }
        if (CurrentHealth <= 0) Die(damageData);
    }

    private void OnEnable() { OnRecoveryHealth += SetRecovery; }
    private bool IsImmune(AttackType attackType) => CheckStunImmunity?.Invoke(attackType) ?? false;

    private void GetCharacterControllerCapsule(out Vector3 top, out Vector3 bottom, out float radius)
    {
        Vector3 scale = transform.lossyScale;
        float horizontalScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        float verticalScale = Mathf.Abs(scale.y);

        radius = Mathf.Max(0.01f, _characterController.radius * horizontalScale);
        float height = Mathf.Max(_characterController.height * verticalScale, radius * 2f);
        Vector3 worldCenter = transform.TransformPoint(_characterController.center);
        float halfSegment = Mathf.Max(0f, (height * 0.5f) - radius);

        top = worldCenter + transform.up * halfSegment;
        bottom = worldCenter - transform.up * halfSegment;
    }

    private LayerMask GetKnockbackBlockingMask()
    {
        LayerMask fallbackMask = LayerMask.GetMask("Ground", "Wall", "Enemy", "HitObject");
        if (_owner?.Movement == null)
        {
            return fallbackMask;
        }

        LayerMask movementMask = _owner.Movement.obstacleMask;
        return movementMask.value != 0
            ? movementMask | LayerMask.GetMask("Ground", "Enemy", "HitObject")
            : fallbackMask;
    }

    private bool IsBlockingKnockbackCollider(Collider otherCollider, LayerMask blockingMask)
    {
        if (otherCollider == null || otherCollider.isTrigger)
        {
            return false;
        }

        Transform otherTransform = otherCollider.transform;
        if (otherTransform == transform || otherTransform.IsChildOf(transform))
        {
            return false;
        }

        return ((1 << otherCollider.gameObject.layer) & blockingMask.value) != 0;
    }

    private float CalculateKnockbackTravelDistance(float force, float duration, AnimationCurve curve)
    {
        if (force <= 0f || duration <= 0f)
        {
            return 0f;
        }

        if (curve == null || curve.length == 0)
        {
            return force * duration;
        }

        const int Samples = 12;
        float area = 0f;
        float previousValue = curve.Evaluate(0f);

        for (int i = 1; i <= Samples; i++)
        {
            float time = i / (float)Samples;
            float currentValue = curve.Evaluate(time);
            area += (previousValue + currentValue) * 0.5f * (1f / Samples);
            previousValue = currentValue;
        }

        return Mathf.Max(0f, force * duration * area);
    }

    private bool CanStartKnockback(Vector3 horizontalDirection, float force, float duration, AnimationCurve curve)
    {
        if (_characterController == null || !_characterController.enabled)
        {
            return false;
        }

        Vector3 direction = horizontalDirection;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        float totalDistance = CalculateKnockbackTravelDistance(force, duration, curve);
        if (totalDistance <= 0.0001f)
        {
            return false;
        }

        direction.Normalize();
        LayerMask blockingMask = GetKnockbackBlockingMask();

        Vector3 currentPosition = _owner.transform.position;
        Vector3 expectedTarget = currentPosition + direction * totalDistance;
        Vector3 safeTarget = _owner.Movement.GetSafeKnockbackPosition(currentPosition, direction, totalDistance, out bool wasClamped);
        if (wasClamped || !_owner.Movement.IsMeaningfulSafeMove(currentPosition, safeTarget))
        {
            return false;
        }

        GetCharacterControllerCapsule(out Vector3 top, out Vector3 bottom, out float radius);
        float castPadding = Mathf.Max(_characterController.skinWidth, 0.01f);
        int hitCount = Physics.CapsuleCastNonAlloc(
            top,
            bottom,
            radius,
            direction,
            KnockbackSweepHits,
            totalDistance + castPadding,
            blockingMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = KnockbackSweepHits[i];
            if (!IsBlockingKnockbackCollider(hit.collider, blockingMask))
            {
                continue;
            }

            if (hit.distance > castPadding)
            {
                return false;
            }
        }

        Vector3 targetOffset = expectedTarget - currentPosition;
        Vector3 targetTop = top + targetOffset;
        Vector3 targetBottom = bottom + targetOffset;
        int overlapCount = Physics.OverlapCapsuleNonAlloc(
            targetTop,
            targetBottom,
            radius,
            KnockbackOverlapHits,
            blockingMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < overlapCount; i++)
        {
            if (IsBlockingKnockbackCollider(KnockbackOverlapHits[i], blockingMask))
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator KnockbackCoroutine(Vector3 direction, DamageData damageData)
    {
        bool isDead = curHealth <= 0;
        float elapsedTime = 0f;
        float duration = isDead ? damageData.DeathKnockbackDuration : damageData.KnockbackDuration;
        float force = isDead ? damageData.DeathKnockbackForce : damageData.KnockbackForce;
        Vector3 horizontalDirection = direction;
        horizontalDirection.y = 0f;
        horizontalDirection.Normalize();

        if (_owner == null || _owner.Movement == null || _characterController == null || !_characterController.enabled)
        {
            _KnockbackCoroutine = null;
            yield break;
        }

        if (horizontalDirection.sqrMagnitude < 0.01f || duration <= 0f || force <= 0f)
        {
            _KnockbackCoroutine = null;
            yield break;
        }

        AIPath aiPath = _owner.aIPath;
        _owner.Movement.StopMovement();

        if (aiPath != null)
        {
            if (isDead)
            {
                aiPath.enabled = false;
            }
            else
            {
                aiPath.canMove = false;
                aiPath.isStopped = true;
                aiPath.destination = _owner.transform.position;
                aiPath.Teleport(_owner.transform.position, false);
            }
        }

        while (elapsedTime < duration)
        {
            if (_characterController == null || !_characterController.enabled)
            {
                break;
            }

            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            float curveValue = damageData.KnockbackCurve != null
                ? damageData.KnockbackCurve.Evaluate(normalizedTime)
                : 1f;
            float horizontalMoveDistance = force * curveValue * Time.deltaTime;
            if (horizontalMoveDistance <= 0.0001f)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
                continue;
            }

            Vector3 currentPosition = _owner.transform.position;
            Vector3 safeHorizontalTarget = _owner.Movement.GetSafeKnockbackPosition(currentPosition, horizontalDirection, horizontalMoveDistance, out bool wasClamped);

            if (!_owner.Movement.IsMeaningfulSafeMove(currentPosition, safeHorizontalTarget))
            {
                if (wasClamped)
                {
                    break;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
                continue;
            }

            Vector3 move = safeHorizontalTarget - currentPosition;
            if (!_characterController.isGrounded)
            {
                move.y += Physics.gravity.y * Time.deltaTime;
            }

            _characterController.Move(move);

            if (aiPath != null && aiPath.enabled)
            {
                aiPath.Teleport(_owner.transform.position, false);
            }

            if (wasClamped)
            {
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (aiPath != null && aiPath.enabled)
        {
            aiPath.Teleport(_owner.transform.position, false);
            aiPath.canMove = false;
            aiPath.isStopped = true;
            aiPath.destination = _owner.transform.position;
        }

        _KnockbackCoroutine = null;
    }
}
