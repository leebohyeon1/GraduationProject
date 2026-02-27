using System;
using System.Collections;
using Packages.Rider.Editor.UnitTesting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyStat enemyStat;
    public ImmunityLevel _currentImmunityLevel = ImmunityLevel.None;

    public int CurrentHealth => curHealth;
    public int MaxHealth => enemyStat.Maxhealth;

    int _maxHealth = 100;
    int curHealth = 100;
    public bool IsDead => CurrentHealth <= 0;
    public event Action<int, int> OnHealthChanged;
    public Action<bool> OnRecoveryHealth;
    public int healthPerSecond = 20; // 초당 회복량
    private Coroutine _recoveryCoroutine;
    public event Action OnDied;
    private CharacterController _characterController;
    private Coroutine _KnockbackCoroutine;
    public Action<AttackType> OnDamageReceived;
    public Func<AttackType, bool> CheckStunImmunity;
  
    Enemy _owner;
    private float _knockbackResistance = 1f;
    public bool ImmunityStart = false;

    public void InitializeHealth(Enemy owner, EnemyStatMultiplier statMultiplier = default)
    {
        _owner = owner;
        _knockbackResistance = statMultiplier?.KnockbackMultiply ?? 1f;
        _maxHealth = enemyStat.Maxhealth;
        _maxHealth = (int)(_maxHealth * statMultiplier?.HealthMultiply ?? _maxHealth);
        curHealth = _maxHealth;
        if (_owner.animator.GetBool("Die"))
            _owner.animator.SetBool("Die", false);
        _characterController = _owner.GetComponent<CharacterController>();
        SetKnockbackable(true);
        _owner.tag = "Enemy";
        _currentImmunityLevel = ImmunityLevel.None;
        if(ImmunityStart)
        {
        _currentImmunityLevel = ImmunityLevel.Minor;
            
        }
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
    void OnDisable()
    {
        OnRecoveryHealth -= SetRecovery;
    }
    public void SetRecovery(bool isRecovering)
    {
        if (isRecovering)
        {
            // 이미 회복 중이 아닐 때만 코루틴 시작
            if (_recoveryCoroutine == null)
            {
                _recoveryCoroutine = StartCoroutine(RecoveryRoutine());
            }
        }
        else
        {
            // 회복 중이라면 중단
            if (_recoveryCoroutine != null)
            {
                StopCoroutine(_recoveryCoroutine);
                _recoveryCoroutine = null;
            }
        }
    }

    // 4. 점진적 회복 코루틴
    private IEnumerator RecoveryRoutine()
    {
        while (curHealth < _maxHealth)
        {
            curHealth += (int)(healthPerSecond * Time.deltaTime);

            // 최대 체력 초과 방지
            if (curHealth > _maxHealth)
            {
                curHealth = _maxHealth;
            }

            // UI 갱신 등이 필요하다면 여기서 호출
            // Debug.Log($"Recovering... {curHealth}");

            yield return null; // 다음 프레임 대기
        }

        // 체력이 다 차면 코루틴 종료 및 변수 초기화
        _recoveryCoroutine = null;
    }

    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead)
        {
            return;
        }

    }

    public bool Knockbackable { get; private set; } = true;

    public void Die()
    {
        OnDied?.Invoke();
        _owner.animHandler.PlayFeedback("Die");
        _owner.player.Money.GiveMoney(_owner.enemyStat.MoneyReward);
        _owner.animator.SetBool("Die", true);
        _owner.animator.speed = 1;
        _owner.Movement.StopMovement();
        _owner.SetState(EnemyStateController.EnemyState.Die);
        _owner.groupAi.GroupRemove(_owner);
        GetComponent<Animator>().enabled = false;
        _owner.tag = "DeadEnemy";
    }

    private IEnumerator ActivateImmunityAfterDelay(float delay)
    {
        // 1. 지정된 시간(5초)만큼 대기
        yield return new WaitForSeconds(delay);

        // 2. 시간이 지난 후 면역 활성화
        if (!_owner.EnemyHealth.IsDead) // 죽지 않았다면 실행
        {
            // ParrySystem에 있는 면역 활성화 함수 호출
            _owner.ParrySystem.ActivateMinorImmunity();
            Debug.Log("5초 경과: 이제부터 소경직 면역입니다!");
        }

        // (선택사항) 코루틴 변수를 null로 초기화하여 나중에 다시 카운트할지, 
        // 아니면 한 번 켜지면 끝까지 갈지 결정합니다.
        // 한 번만 켜지고 끝이라면 초기화하지 않습니다.
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
        {
            _delayCoroutine = StartCoroutine(ActivateImmunityAfterDelay(MinorTime));
        }
        bool isImmune = IsImmune(damageData.AttackType);
        if(!isImmune)
        {
            OnDamageReceived?.Invoke(damageData.AttackType);
        }
        if (!isImmune && !isBlocked )
        {
            // 액션(공격 등) 중이 아닐 때만 히트 모션 취함
            if (!_owner._aiController.IsActionable())
            {
                Debug.Log("히트 리액션 적용: 액션 중이 아님");
                _owner.SetState(EnemyStateController.EnemyState.Hit);
                _owner.AnimationEvent("Hit");
                _owner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, true);
            }
        }


        int previousHealth = curHealth;
        if (isBlocked)
        {
            switch (damageData.AttackType)
            {
                case AttackType.Heavy1:
                    _owner.animHandler.PlayFeedback("Block_FB", AttackType.Heavy1);
                    break;
                case AttackType.Heavy2:

                    _owner.animHandler.PlayFeedback("Block_FB", AttackType.Heavy2);
                    break;
                case AttackType.Heavy3:

                    _owner.animHandler.PlayFeedback("Block_FB", AttackType.Heavy3);
                    break;
                default:
                    _owner.animHandler.PlayFeedback("Block_FB", AttackType.Normal);
                    break;
            }
        
        }
        else
        {
            
        // Debug.Log(damageData.AttackType);
        switch (damageData.AttackType)
            {
                case AttackType.Heavy1:
                    _owner.animHandler.PlayFeedback("Damage_FB", AttackType.Heavy1);
                    break;
                case AttackType.Heavy2:

                    _owner.animHandler.PlayFeedback("Damage_FB", AttackType.Heavy2);
                    break;
                case AttackType.Heavy3:

                    _owner.animHandler.PlayFeedback("Damage_FB", AttackType.Heavy3);
                    break;
                default:
                    _owner.animHandler.PlayFeedback("Damage_FB", AttackType.Normal);
                    break;
            }
        }

        curHealth -= finalDamage;

        OnHealthChanged?.Invoke(previousHealth, curHealth);
        _owner.BillboardUI?.SetHealthBar(_maxHealth, curHealth);
        _owner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.SelfHealth, curHealth);

        
        OnRecoveryHealth?.Invoke(false);

        if (Knockbackable && !isBlocked)
        {
            Vector3 knockbackDir = (transform.position - damageData.AttackerTransform.position).normalized;
            knockbackDir.y = 0;
            if (_KnockbackCoroutine != null)
            {
                StopCoroutine(_KnockbackCoroutine);
            }
            _KnockbackCoroutine = StartCoroutine(KnockbackCoroutine(knockbackDir, damageData));
        }
        if (CurrentHealth <= 0)
        {
            Die();

        }
    }
    void OnEnable()
    {
        OnRecoveryHealth += SetRecovery;
    }
    void Start()
    {
    }
    private bool IsImmune(AttackType attackType)
    {
        if (CheckStunImmunity != null)
            return CheckStunImmunity(attackType);
        return false;
    }

    public float KnockbackForce = 30f;
    public float upwardForce = 5f;
    public void SetKnockbackable(bool value)
    {
        Knockbackable = value;
    }

    private IEnumerator KnockbackCoroutine(Vector3 direction, DamageData damageData)
    {
        float elapsedTime = 0;
        Vector3 horizontalDirection = direction;
        horizontalDirection.y = 0;
        horizontalDirection.Normalize();

        if (horizontalDirection.sqrMagnitude < 0.01f)
        {
            _KnockbackCoroutine = null;
            yield break;
        }


        while (elapsedTime < damageData.KnockbackDuration)
        {
            float curveValue = damageData.KnockbackCurve.Evaluate(elapsedTime / damageData.KnockbackDuration);

            Vector3 move = horizontalDirection * damageData.KnockbackForce * _knockbackResistance * curveValue * Time.deltaTime;

            if (!_characterController.isGrounded)
            {
                move.y += Physics.gravity.y * Time.deltaTime;
            }

            _characterController.Move(move);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _KnockbackCoroutine = null;
    }

}