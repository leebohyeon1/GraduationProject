using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

// Todo: 차지 레벨 사라짐에 따른 리펙토링 필요


/// <summary>
/// 플레이어의 전투 관련 로직을 담당하는 컴포넌트입니다.
/// </summary>
public class PlayerCombat : MonoBehaviour, IDisposable
{
    [Header("References")]
    private PlayerEvents _events;   // 플레이어 이벤트
    private PlayerData _data = null;
    [SerializeField] private Transform _attackPoint;

    [SerializeField] private OnSwingMissSO _onSwingMiss;  // 공격 미스 이벤트

    [Header("Attack")]
    [SerializeField] private LayerMask _attackLayerMask;
    // 공격 회복 비율
    public float AttackRegainRate => _data != null ? _data.Regain.Value : 0f;

    [Header("NormalAttack")]
    [SerializeField] private int _normalAttackComboIndex = -1;    // 일반 공격 콤보 순서
    public int NormalAttackComboIndex => _normalAttackComboIndex;

    // 일반 공격 리스트
    public List<RuntimeAttackConfig> NormalAttackConfigList => _data != null ? _data.NormalAttacks : new List<RuntimeAttackConfig>();

    public float PlusNormalAttackSpeedMultiplier => 0f; // TODO: 스탯 시스템에 통합 필요시 Stat으로 변경

    [Header("HeavyAttack")]
    [SerializeField] private int _heavyAttackComboIndex = -1;
    public int HeavyAttackComboIndex => _heavyAttackComboIndex;
    public List<RuntimeAttackConfig> HeavyAttackConfigList => _data != null ? _data.HeavyAttacks : new List<RuntimeAttackConfig>();
    
    [SerializeField] private int _heavyAttackConsumedStacks = 0; // 이번 연속 강공격에서 소모한 총 스택 수

    [Header("Charge")]
    // 차지 스테미나
    public float ChargeStamina => _data != null ? _data.ChargeStamina.Value : 5f;
    // 최대 차지 시간
    public float MaxChargeTime => _data != null ? _data.MaxChargeTime.Value : 5f;

    [SerializeField] private bool _isCharge = false;      // 차지 여부
    public bool IsCharge => _isCharge;

    [Header("Counter")]
    public RuntimeAttackConfig NormalCounterAttackConfig => _data.NormalCounterAttack;
    public RuntimeChargeAttackConfig HeavyCounterAttackConfig => _data.HeavyCounterAttack;
    public List<float> ProjectileCounterAddedVelocity => _data.BaseData.ProjectileCounterAddedVelocity;

    public float CounterAngle => _data != null ? _data.BaseData.CounterAngle : 120;   // 상쇄 가능 각도

    [SerializeField] private bool _isCounterable = false;          // 상쇄 가능 여부
    [SerializeField] private HashSet<IParryable> _counterEnemySet = new HashSet<IParryable>();

    [SerializeField] private PlayerAbilityTagSO _counterSuccessTagSO; // 카운터 성공 시 슈퍼아머
    public PlayerAbilityTagSO CounterSuccessTagSO => _counterSuccessTagSO;

    public event Action CheckedProjectileCounter;

    [Header("BattleState")]
    [SerializeField] private float _lastBattleTime;  // 마지막 전투 시간
    public float LastBattleTime => _lastBattleTime; // 마지막 전투 시간

    [SerializeField] private bool _isBattleState;    // 전투 중인지 여부
    public bool IsBattleState => _isBattleState; // 전투 상태 여부

    private Coroutine _battleStateStopCoroutine; // 전투 상태 종료 코루틴
    public event Action<bool> BattleStateChaged; // 전투 상태 변경 이벤트

    [Header("Parry Stack")]
    [SerializeField] private int _parryStacks = 0;
    public int ParryStacks => _parryStacks;
    public event Action<int> ParryStackChanged;

    [SerializeField] private float _parryStackTimer = 0f;
    public float ParryStackTimer => _parryStackTimer;
    private const int MAX_PARRY_STACKS = 3;
    private const float PARRY_STACK_DURATION = 30f;

    /// <summary>
    /// 현재 패링 스택에 따른 데미지 배율을 반환합니다.
    /// </summary>
    public float ParryStackMultiplier;
    //{
    //    get
    //    {
    //        if (_data == null || _data.ParryStackDamageMultipliers == null || _data.ParryStackDamageMultipliers.Count == 0)
    //            return 1f;

    //        int index = Mathf.Clamp(_parryStacks, 0, _data.ParryStackDamageMultipliers.Count - 1);
    //        return _data.ParryStackDamageMultipliers[index];
    //    }
    //}


    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerController player)
    {
        _data = player.RuntimeData;
        _events = player.Events;

        _events.CounterWindowStarted += OnCounterWindowStarted;
        _events.CounterWindowFinished += OnCounterWindowFinished;
        _events.CounterSucceeded += OnCounterSucceeded;

        _events.BeforeDamaged += OnBeforeDamaged;

        // 이벤트 해제 구독
        player.RegisterDisposable(this);

        InitializeData(player.Data);

        // 패링 스택 초기화
        _parryStacks = 0;
        _parryStackTimer = 0f;
    }



    private void Update()
    {
        // 패링 스택 타이머 관리
        if (_parryStacks > 0)
        {
            _parryStackTimer -= Time.deltaTime;
            if (_parryStackTimer <= 0)
            {
                // 스택 1개 감소 및 타이머 재설정
                _parryStacks--;
                ParryStackChanged?.Invoke(_parryStacks);
                if (_parryStacks > 0)
                {
                    _parryStackTimer = PARRY_STACK_DURATION;
                }
            }
        }
    }

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
        _events.CounterWindowStarted -= OnCounterWindowStarted;
        _events.CounterWindowFinished -= OnCounterWindowFinished;

        _events.BeforeDamaged -= OnBeforeDamaged;

        if(_battleStateStopCoroutine != null)
        {
            StopCoroutine(_battleStateStopCoroutine);
            _battleStateStopCoroutine = null;
        }

        BattleStateChaged = null;
    }

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    /// <param name="data">플레이어 데이터</param>
    private void InitializeData(PlayerDataSO data)
    {
        _attackLayerMask = data.AttackLayerMask;
    }

    //==========================================================================================================================
    // BattleState =============================================================================================================
    //==========================================================================================================================

    #region BattleState

    /// <summary>
    /// 전투 상태를 변경합니다.
    /// </summary>
    /// <param name="isBattleState">새로운 전투 상태</param>
    public void SetBattleState(bool isBattleState)
    {
        _isBattleState = isBattleState;
    }

    /// <summary>
    /// 전투 상태 변경 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerBattleStateChanged(bool isBattleState)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (isBattleState)
        {
            if(_battleStateStopCoroutine != null)
            {
                StopCoroutine(_battleStateStopCoroutine);
            }
            
            _battleStateStopCoroutine = StartCoroutine(BattleStateStopCoroutine());
        }

        SetBattleState(isBattleState);
        BattleStateChaged?.Invoke(isBattleState);
    }

    /// <summary>
    /// 전투 상태 종료 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator BattleStateStopCoroutine()
    {
        yield return new WaitForSeconds(8f);
        
        TriggerBattleStateChanged(false);
        _battleStateStopCoroutine = null;
    }

    #endregion


    //==========================================================================================================================
    // Attack ==================================================================================================================
    //==========================================================================================================================

    #region Attack
    /// <summary>
    /// 공격의 중심 위치를 계산합니다.
    /// </summary>
    /// <returns>공격 박스의 중심 위치</returns>
    public Vector3 GetAttackCenter(PlayerAttackConfig attackData)
    {
        return _attackPoint.position + _attackPoint.forward * (attackData.AttackRadius.z / 2);
    }

    /// <summary>
    /// 공격의 중심 위치를 계산합니다. (IRuntimeAttackConfig 인터페이스 사용)
    /// </summary>
    /// <returns>공격 박스의 중심 위치</returns>
    public Vector3 GetAttackCenter(IRuntimeAttackConfig attackData)
    {
        return _attackPoint.position + _attackPoint.forward * (attackData.AttackRadius.z / 2);
    }

    /// <summary>
    /// 공격을 실행합니다. (IRuntimeAttackConfig 인터페이스 사용)
    /// </summary>
    public Collider[] ExecuteAttack(IRuntimeAttackConfig attackData)
    {
        return ExecuteAttackInternal(attackData, (int)attackData.Damage.Value);
    }

    /// <summary>
    /// 공격을 실행합니다. (커스텀 데미지 계산 지원)
    /// </summary>
    public Collider[] ExecuteAttackWithCustomDamage(IRuntimeAttackConfig attackData, Func<int, int> damageCalculator)
    {
        return ExecuteAttackInternal(attackData, (int)attackData.Damage.Value, damageCalculator);
    }

    /// <summary>
    /// 내부 공격 실행 로직
    /// </summary>
    private Collider[] ExecuteAttackInternal(IRuntimeAttackConfig data, int runtimeDamage, Func<int, int> damageCalculator = null)
    {
        Vector3 attackCenter = GetAttackCenter(data);
        Vector3 halfExtents = data.AttackRadius / 2f;

        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _attackLayerMask);

        if (hitEnemies.Length > 0)
        {
            ProcessHitEnemiesInternal(data, runtimeDamage, hitEnemies, damageCalculator);
        }
        else
        {
            _onSwingMiss.Publish("OnSwingMiss");
        }

        return hitEnemies;
    }

    /// <summary>
    /// 공격에 맞은 적들에게 데미지를 입힙니다. (내부용)
    /// </summary>
    private void ProcessHitEnemiesInternal(IRuntimeAttackConfig data, int runtimeDamage, Collider[] hitObjects, Func<int, int> damageCalculator = null)
    {
        foreach (Collider obj in hitObjects)
        {
            // 패링된 적일 경우 넘긴다.
            if (obj.TryGetComponent<IParryable>(out var parryable) && IsEnemyCountered(parryable))
            {
                continue;
            }

            if (obj.TryGetComponent<IDamageable>(out var damageable))
            {
                int finalDamage = (damageCalculator != null) ? damageCalculator(runtimeDamage) : CalculateFinalDamage(runtimeDamage);

                DamageData damage = new DamageData
                {
                    AttackerTransform = transform,
                    AttackType = data.AttackType,
                    DamageAmount = finalDamage,
                    StiffnessAmount = 0,
                    KnockbackCurve = data.KnockbackConfig.StepCurve,
                    KnockbackDuration = data.KnockbackConfig.StepDuration,
                    KnockbackForce = data.KnockbackConfig.StepDistance,
                };

                Attack(damageable, damage);
            }
        }
    }

    /// <summary>
    /// 데미지를 입을 수 있는 객체에 타격
    /// </summary>
    /// <param name="damageable">타격 받을 객체</param>
    /// <param name="damageData">데미지 데이터</param>
    public void Attack(IDamageable damageable, DamageData damageData)
    {
        if (!damageable.IsDead)
        {
            damageable.TakeDamage(damageData);

            int regainAmount = Mathf.RoundToInt(damageData.DamageAmount * AttackRegainRate);
            _events.TriggerAttackRegained(regainAmount);
        }
    }

    /// <summary>
    /// 최종 데미지를 계산합니다. (공격력 배율 및 패링 스택 배율 적용)
    /// </summary>
    /// <param name="baseDamage">기본 데미지</param>
    /// <returns>최종 데미지</returns>
    public int CalculateFinalDamage(int baseDamage)
    {
        return Mathf.RoundToInt(baseDamage);

    }

    #endregion

    //==========================================================================================================================
    // NormalAttack ============================================================================================================
    //==========================================================================================================================

    #region NormalAttack
    /// <summary>
    /// 일반 공격 콤보 번호 증가
    /// </summary>
    public void IncreaseNormalAttackComboIndex()
    {
        _normalAttackComboIndex++;
    }

    /// <summary>
    /// 일반 공격 콤보 리셋
    /// </summary>
    public void ResetNormalAttackComboIndex()
    {
        _normalAttackComboIndex = -1;
    }

    /// <summary>
    /// 일반 공격 콤보 번호와 일반 공격 데이터 크기 비교 후
    /// 일반 공격이 가능한지 여부 반환
    /// </summary>
    /// <returns>일반 공격 가능 여부</returns>
    public bool CanNormalAttack()
    {
        return _normalAttackComboIndex < (NormalAttackConfigList.Count - 1);
    }

    /// <summary>
    /// 강공격 콤보 인덱스 증가
    /// </summary>
    public void IncreaseHeavyAttackComboIndex()
    {
        _heavyAttackComboIndex++;
    }

    /// <summary>
    /// 강공격 콤보 인덱스 리셋 (연속 공격 종료 시 호출)
    /// </summary>
    public void ResetHeavyAttackComboIndex()
    {
        _heavyAttackComboIndex = -1;
        _heavyAttackConsumedStacks = 0;
    }

    /// <summary>
    /// 강공격 가능 여부 확인 (패리 스택이 있고 최대 3콤보까지만 가능하도록 제한)
    /// </summary>
    public bool CanHeavyAttack()
    {
        // 패리 스택이 1개 이상 있어야 함
        return _parryStacks > 0;
    }

    /// <summary>
    /// 패리 스택 소모 (강공격 시 호출)
    /// </summary>
    public void ConsumeParryStack()
    {
        if (_parryStacks > 0)
        {
            _parryStacks--;
            _heavyAttackConsumedStacks++;
            _parryStackTimer = _parryStacks > 0 ? PARRY_STACK_DURATION : 0f;
            ParryStackChanged?.Invoke(_parryStacks);
        }
    }

    /// <summary>
    /// 강공격 전용 최종 데미지 계산 (소모 스택 배율 적용)
    /// </summary>
    public int CalculateHeavyAttackDamage(int baseDamage)
    {
        // 1타: base * stackMultiplier
        // 2타 이상: base * stackMultiplier * consumedStacks
        int modifiedBase = baseDamage + Mathf.RoundToInt(baseDamage);
        float damage = modifiedBase;
        
        if (_heavyAttackComboIndex > 0)
        {
            damage *= _heavyAttackConsumedStacks;
        }

        return Mathf.RoundToInt(damage);
    }
    #endregion

    //==========================================================================================================================
    // Charge ==================================================================================================================
    //==========================================================================================================================
    
    #region Charge
    /// <summary>
    /// 차지 레벨 증가
    /// </summary>
    public void SetCharge(bool isCharge)
    {
        if(isCharge != _isCharge)
        {
            _isCharge = isCharge;
        }
    }
    #endregion

    //==========================================================================================================================
    // Counter =================================================================================================================
    //==========================================================================================================================

    #region Counter
    /// <summary>
    /// 상쇄 가능 여부 설정
    /// </summary>
    /// <param name="value">설정값</param>
    public void SetCounterable(bool value)
    {
        _isCounterable = value;
    }

    /// <summary>
    /// 카운터된 적 추가
    /// </summary>
    /// <param name="enemy">카운터된 적</param>
    public void AddCounterEnemy(IParryable enemy)
    {
        _counterEnemySet.Add(enemy);
    }

    /// <summary>
    /// 카운터된 적들 초기화
    /// </summary>
    public void ClearCounterEnemySet()
    {
        _counterEnemySet.Clear();   
    }

    /// <summary>
    /// 이미 적이 상쇄되었는지 체크
    /// </summary>
    /// <returns>있는지 여부</returns>
    public bool IsEnemyCountered(IParryable enemy)
    {
        if(_counterEnemySet.Contains(enemy))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 투사체 체크 이벤트 발행
    /// </summary>
    public void TriggerCheckedProjectileCounter()
    {
        CheckedProjectileCounter?.Invoke();
    }
    #endregion

    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 카운터 검사 시작
    /// </summary>
    private void OnCounterWindowStarted()
    {
        SetCounterable(true);
    }

    /// <summary>
    /// 카운터 검사 종료
    /// </summary>
    private void OnCounterWindowFinished()
    {
        SetCounterable(false);
    }

    private void OnCounterSucceeded(Transform transform)
    {
        // 패링 스택 획득 및 타이머 초기화
        _parryStacks = Mathf.Min(_parryStacks + 1, MAX_PARRY_STACKS);
        _parryStackTimer = PARRY_STACK_DURATION;
        ParryStackChanged?.Invoke(_parryStacks);
    }

    /// <summary>
    /// 데미지 받기 전 이벤트 발행
    /// </summary>
    /// <param name="damageContext">받은 데미지 데이터</param>
    private void OnBeforeDamaged(ref PlayerDamageContext damageContext)
    {
        DamageData damageData = damageContext.Data;

        Vector3 toEnemy = damageData.AttackerTransform.transform.position - transform.position; // 적으로 가는 벡터 구하기
        // 적을 마주보고 있는가
        bool isFacingEnemy = Vector3.Angle(transform.forward, toEnemy) <= (CounterAngle / 2f);


        // 공격 타입이 Heavy일 때 차징했거나, 공격 타입이 Normal인가
        bool validateAttackType = (damageData.AttackType >= AttackType.Heavy1 && _isCharge) || damageData.AttackType == AttackType.Normal;


        // 카운터에 성공하면 데미지 데이터 전부 0으로 처리
        if (_isCounterable && isFacingEnemy && validateAttackType && !damageData.IsMagic
            && damageData.AttackerTransform.TryGetComponent<IParryable>(out IParryable parryable))
        {
            damageData.DamageAmount = 0;
            damageData.StiffnessAmount = 0;
            damageData.KnockbackDuration = 0;
            damageData.KnockbackForce = 0;

            damageContext.HasSuperArmor = true;


            // 카운터 성공 이벤트 발행
            _events.TriggerCounterSucceeded(damageData.AttackerTransform);
        }

        damageContext.Data = damageData;
    }


    private void OnDrawGizmosSelected()
    {
        if (_data != null)
        {
            foreach (var attackData in _data.NormalAttacks)
            {
                Gizmos.DrawWireCube(GetAttackCenter(attackData), attackData.AttackRadius);
            }
        }
    }
}