using System;
using System.Collections;
using UnityEngine;

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
    private float _knockbackResistance = 1f;
    public bool ImmunityStart = false;

    public void InitializeHealth(Enemy owner, EnemyStatMultiplier statMultiplier = default)
    {
        _owner = owner;
        _knockbackResistance = statMultiplier?.KnockbackMultiply ?? 1f;
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
            if (incomingAttackType == AttackType.Normal) return true;
        }
        if (_currentImmunityLevel == ImmunityLevel.Major)
        {
            if (incomingAttackType == AttackType.NormalCounter || incomingAttackType == AttackType.Normal) return true;
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

    public void Die()
    {
        OnDied?.Invoke();
        _owner.animHandler.PlayFeedback("Die");
        if (_owner.player != null && _owner.player.Money != null)
            _owner.player.Money.GiveMoney(_owner.enemyStat.MoneyReward);
        
        _owner.animator.SetBool("Die", true);
        _owner.animator.speed = 1;
        _owner.Movement.StopMovement();
        _owner.SetState(EnemyStateController.EnemyState.Die);
        _owner.groupAi.GroupRemove(_owner);
        _owner.tag = "DeadEnemy";
        
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
    }

    public float MinorTime = 5;
    private Coroutine _delayCoroutine;

    public void TakeDamage(DamageData damageData)
    {
        if (CurrentHealth <= 0) return;
        bool isBlocked = false;
        int finalDamage = damageData.DamageAmount;
        if(_owner.Shield != null && damageData.AttackerTransform != null)
        {
            if (_owner.Shield.CheckBlock(damageData.AttackerTransform.position))
            {
                isBlocked = true;
                float damageMultiplier = 1.0f - _owner.Shield.DamageReduction;
                finalDamage = Mathf.RoundToInt(damageData.DamageAmount * damageMultiplier);
            }
        }
        _owner.groupAi.CombatAll();
        if (_delayCoroutine == null && !IsImmune(damageData.AttackType) && !_owner.ParrySystem._isStunned )
            _delayCoroutine = StartCoroutine(ActivateImmunityAfterDelay(MinorTime));
        
        bool isImmune = IsImmune(damageData.AttackType);
        if(!isImmune) OnDamageReceived?.Invoke(damageData.AttackType);
        
        if (!isImmune && !isBlocked )
        {
            if (!_owner._aiController.IsActionable())
            {
                _owner.SetState(EnemyStateController.EnemyState.Hit);
                _owner.AnimationEvent("Hit");
                _owner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, true);
            }
        }

        int previousHealth = curHealth;
        _owner.animHandler.PlayFeedback(isBlocked ? "Block_FB" : "Damage_FB", damageData.AttackType);

        curHealth -= finalDamage;
        OnHealthChanged?.Invoke(previousHealth, curHealth);
        _owner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.SelfHealth, curHealth);
        OnRecoveryHealth?.Invoke(false);

        if (Knockbackable && !isBlocked)
        {
            Vector3 knockbackDir = (transform.position - damageData.AttackerTransform.position).normalized;
            knockbackDir.y = 0;
            if (_KnockbackCoroutine != null) StopCoroutine(_KnockbackCoroutine);
            _KnockbackCoroutine = StartCoroutine(KnockbackCoroutine(knockbackDir, damageData));
        }
        if (CurrentHealth <= 0) Die();
    }

    private void OnEnable() { OnRecoveryHealth += SetRecovery; }
    private bool IsImmune(AttackType attackType) => CheckStunImmunity?.Invoke(attackType) ?? false;

    public float KnockbackForce = 30f;
    private IEnumerator KnockbackCoroutine(Vector3 direction, DamageData damageData)
    {
        float elapsedTime = 0;
        Vector3 horizontalDirection = direction;
        horizontalDirection.y = 0;
        horizontalDirection.Normalize();
        if (horizontalDirection.sqrMagnitude < 0.01f) { _KnockbackCoroutine = null; yield break; }

        while (elapsedTime < damageData.KnockbackDuration)
        {
            float curveValue = damageData.KnockbackCurve.Evaluate(elapsedTime / damageData.KnockbackDuration);
            Vector3 move = horizontalDirection * damageData.KnockbackForce * curveValue * Time.deltaTime;
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
