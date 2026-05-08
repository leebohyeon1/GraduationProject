using System;
using System.Collections;
using Pathfinding;
using UnityEngine;
using FIMSpace.FProceduralAnimation;
using Packages.Rider.Editor.UnitTesting;

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
        _owner.enemyStat.RewardSO.RemoveMoneyFromEnemies(_owner.MonsterId);
        _owner.animator.SetBool("Die", true);
        _owner.animator.speed = 1;
        _owner.Movement.StopMovement();
        _owner.enemyStat.EStateEventSO.Publish(new EnemyStateData{
            enemy = _owner, stateType = EnemyStateType.Dead});
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
            Vector3 knockbackDir = (transform.position - damageData.AttackerTransform.position).normalized;
            knockbackDir.y = 0;
            if (_KnockbackCoroutine != null) StopCoroutine(_KnockbackCoroutine);
            
            if (curHealth <= 0 && _ragdollAnimator != null)
            {
                // Ragdoll will handle the physics impact
            }
            else
            {
                _KnockbackCoroutine = StartCoroutine(KnockbackCoroutine(knockbackDir, damageData));
            }
        }
        if (CurrentHealth <= 0) Die(damageData);
    }

    private void OnEnable() { OnRecoveryHealth += SetRecovery; }
    private bool IsImmune(AttackType attackType) => CheckStunImmunity?.Invoke(attackType) ?? false;

    private IEnumerator KnockbackCoroutine(Vector3 direction, DamageData damageData)
    {
        bool isDead = curHealth <= 0;
        float elapsedTime = 0;
        Vector3 horizontalDirection = direction;
        horizontalDirection.y = 0;
        horizontalDirection.Normalize();
        if (horizontalDirection.sqrMagnitude < 0.01f) { _KnockbackCoroutine = null; yield break; }

        while (elapsedTime < (isDead ? damageData.DeathKnockbackDuration : damageData.KnockbackDuration))
        {
            if (isDead)
            {
                _owner.aIPath.enabled = false; // 죽음 넉백 동안 경로 탐색 비활성화
            }
            float curveValue = damageData.KnockbackCurve.Evaluate(elapsedTime / (isDead ? damageData.DeathKnockbackDuration : damageData.KnockbackDuration));
            Vector3 move = horizontalDirection * (isDead ? damageData.DeathKnockbackForce : damageData.KnockbackForce) * curveValue * Time.deltaTime;
            if (_characterController != null)
            {
                if (!_characterController.isGrounded) move.y += Physics.gravity.y * Time.deltaTime;
                _characterController.Move(move);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _KnockbackCoroutine = null;
    }
}
