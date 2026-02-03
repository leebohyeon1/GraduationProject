using GSPAWN;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;


/// <summary>
/// 플레이어의 전투 관련 로직을 담당하는 컴포넌트입니다.
/// </summary>
public class PlayerCombat : MonoBehaviour, IDisposable
{
    [Header("References")]
    private PlayerEvents _events;   // 플레이어 이벤트
    private PlayerData _runtimeData;

    [SerializeField] private OnSwingMissSO _onSwingMiss;  // 공격 미스 이벤트

    [Header("Attack")]
    [SerializeField] private LayerMask _attackLayerMask;
    // 공격 회복 비율
    public float AttackRegainRate => _runtimeData != null ? _runtimeData.AttackRegainRate : 0f;
    // 공격력 배율
    public float AttackDamageMultiplier => _runtimeData != null ? _runtimeData.AttackDamageMultiplier : 0f;

    [Header("NormalAttack")]
    [SerializeField] private int _normalAttackComboIndex = -1;    // 일반 공격 콤보 순서
    public int NormalAttackComboIndex => _normalAttackComboIndex;

    // 최대 공속 속도 배율
    public float MaxNormalAttackSpeedMultiplier => _runtimeData != null ? _runtimeData.MaxNormalAttackSpeedMultiplier : 1.0f;
    // 추가 공속 속도 배율
    public float PlusNormalAttackSpeedMultiplier => _runtimeData != null ? _runtimeData.PlusNormalAttackSpeedMultiplier : 0f;
    // 일반 공격 리스트
    public List<PlayerAttackConfig> NormalAttackConfigList => _runtimeData != null ? _runtimeData.NormalAttackConfigList : new List<PlayerAttackConfig>();

    [Header("Charge")]
    // 차지 스테미나
    public float ChargeStamina => _runtimeData != null ? _runtimeData.ChargeStamina : 5f;
    // 최대 차지 시간
    public float MaxChargeTime => _runtimeData != null ? _runtimeData.MaxChargeTime : 5f;

    [SerializeField] private int _chargeLevel = -1;      // 차지 레벨
    public int ChargeLevel => _chargeLevel;

    [Header("Counter")]
    public PlayerAttackConfig NormalCounterAttackConfig => _runtimeData.NormalCounterAttackConfig;
    public List<PlayerChargeConfig> HeavyCounterAttackConfigList => _runtimeData.HeavyCounterAttackConfigList;
    public List<float> ProjectileCounterAddedVelocity => _runtimeData.ProjectileCounterAddedVelocity;

    [SerializeField] public float CounterAngle => _runtimeData != null ? _runtimeData.CounterAngle : 120;   // 상쇄 가능 각도

    [SerializeField] private bool _isCounterable = false;          // 상쇄 가능 여부
    [SerializeField] private HashSet<IParryable> _counterEnemySet = new HashSet<IParryable>();

    [SerializeField] private PlayerAbilityTagSO _counterSuperArmorTagSO; // 카운터 성공 시 슈퍼아머
    public PlayerAbilityTagSO CounterSuperArmorTagSO => _counterSuperArmorTagSO;

    public event Action CheckedProjectileCounter;

    [Header("SpecialAttack")]
    [SerializeField] private CanSpecialAttackSO _specialAttackSO;      // 특수 공격 SO
    public CanSpecialAttackSO SpecialAttackSO => _specialAttackSO;


    [Header("BattleState")]
    [SerializeField] private float _lastBattleTime;  // 마지막 전투 시간
    public float LastBattleTime => _lastBattleTime; // 마지막 전투 시간

    [SerializeField] private bool _isBattleState;    // 전투 중인지 여부
    public bool IsBattleState => _isBattleState; // 전투 상태 여부

    private Coroutine _battleStateStopCoroutine; // 전투 상태 종료 코루틴
    public event Action<bool> BattleStateChaged; // 전투 상태 변경 이벤트


    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerController player)
    {
        _runtimeData = player.RuntimeData;
        _events = player.Events;

        _events.CounterWindowStarted += OnCounterWindowStarted;
        _events.CounterWindowFinished += OnCounterWindowFinished;

        _events.BeforeDamaged += OnBeforeDamaged;

        // 이벤트 해제 구독
        player.RegisterDisposable(this);

        InitializeData(player.Data);

        // 데이터 초기화 (런타임 데이터가 비어있을 경우 SO에서 가져옴)
        if (_runtimeData != null)
        {
            if (_runtimeData.MaxNormalAttackSpeedMultiplier == 0)
                _runtimeData.MaxNormalAttackSpeedMultiplier = player.Data.MaxNormalAttackSpeedMultiplier;
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
    }

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    /// <param name="data">플레이어 데이터</param>
    private void InitializeData(PlayerDataSO data)
    {
        _attackLayerMask = data.AttackLayerMask;

        if (_runtimeData != null)
        {
            if (_runtimeData.ChargeStamina == 0) { _runtimeData.ChargeStamina = data.ChargeStamina; }
            if (_runtimeData.MaxChargeTime == 0) {_runtimeData.MaxChargeTime = data.MaxChargeTime;}
            if (_runtimeData.CounterAngle == 0) { _runtimeData.CounterAngle = data.CounterAngle; }

            // Sync Configs
            if (_runtimeData.NormalAttackConfigList.Count == 0)
            {
                _runtimeData.NormalAttackConfigList.AddRange(data.NormalAttackConfigList);
            }

            if (_runtimeData.HeavyCounterAttackConfigList.Count == 0)
            {
                _runtimeData.HeavyCounterAttackConfigList.AddRange(data.HeavyCounterAttackConfigList);
            }

            // Check if default (assuming damage 0 is invalid/uninitialized)
            if (_runtimeData.NormalCounterAttackConfig.AttackDamage == 0)
            {
                _runtimeData.NormalCounterAttackConfig = data.NormalCounterAttackConfig;
            }

            if (_runtimeData.ProjectileCounterAddedVelocity.Count == 0)
            {
                _runtimeData.ProjectileCounterAddedVelocity.AddRange(data.ProjectileCounterAddedVelocity);
            }
        }
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
        if(isBattleState)
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
        return transform.position + transform.forward * (attackData.AttackRadius.z / 2);
    }

    /// <summary>
    /// 공격을 실행합니다. (일반/차지 공격 등)
    /// Physics.OverlapBox를 사용하여 박스 범위 내의 적을 감지합니다.
    /// </summary>
    /// <param name="attackData">공격 데이터</param>
    /// <returns>타격한 대상의 콜라이더 배열</returns>
    public Collider[] ExecuteAttack(PlayerAttackConfig attackData)
    {
        Vector3 attackCenter = GetAttackCenter(attackData);
        Vector3 halfExtents = attackData.AttackRadius / 2f;

        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _attackLayerMask);

        if (hitEnemies.Length > 0)
        {
            ProcessHitEnemies(attackData, hitEnemies);
        }
        else
        {
            _onSwingMiss.Publish("OnSwingMiss");
        }

        return hitEnemies;
    }

    /// <summary>
    /// 공격에 맞은 적들에게 데미지를 입힙니다.
    /// </summary>
    /// <param name="attackData">공격 데이터</param>
    /// <param name="hitObjects">타격한 대상의 콜라이더 배열</param>
    private void ProcessHitEnemies(PlayerAttackConfig attackData, Collider[] hitObjects)
    {
        foreach (Collider obj in hitObjects)
        {
            // 패링된 적일 경우 넘긴다.
            if (obj.TryGetComponent<IParryable>(out var parryable) && _counterEnemySet.Contains(parryable))
            {
                continue;
            }

            if (obj.TryGetComponent<IDamageable>(out var damageable))
            {
                DamageData damage = new DamageData
                {
                    AttackerTransform = transform,
                    AttackType = attackData.AttackType,
                    DamageAmount = attackData.AttackDamage + Mathf.RoundToInt(attackData.AttackDamage * AttackDamageMultiplier),
                    StiffnessAmount = 0,
                    KnockbackCurve = attackData.KnockbackCofig.StepCurve,
                    KnockbackDuration = attackData.KnockbackCofig.StepDuration,
                    KnockbackForce = attackData.KnockbackCofig.StepDistance,
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
    /// 공격 흡혈 비율 증가
    /// </summary>
    /// <param name="amount">증가량</param>
    public void IncreaseAttackRegainRate(float amount)
    {
        _runtimeData.AttackRegainRate += amount;
    }

    /// <summary>
    /// 공격 흡혈 비율 감소
    /// </summary>
    /// <param name="amount">감소량</param>
    public void DecreaseAttackRegainRate(float amount)
    {
        _runtimeData.AttackRegainRate = Mathf.Max(_runtimeData.AttackRegainRate - amount, 0);
    }

    /// <summary>
    /// 공격력 배율 증가 함수
    /// </summary>
    /// <param name="amount">증가 배율</param>
    public void IncreaseAttackDamageMultiplier(float amount)
    {
        _runtimeData.AttackDamageMultiplier += amount;
    }

    /// <summary>
    /// 공격력 배율 감소 함수
    /// </summary>
    /// <param name="amount">감소 배울</param>
    public void DecreaseAttackDamageMultiplier(float amount)
    {
        _runtimeData.AttackDamageMultiplier -= amount;
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
    /// 일반 공속 배율 증가 함수
    /// </summary>
    /// <param name="rate">증가 배율</param>
    public void IncreaseNormalAttackSpeedMultiplier(float rate)
    {
        _runtimeData.PlusNormalAttackSpeedMultiplier = Mathf.Min(_runtimeData.PlusNormalAttackSpeedMultiplier + rate, MaxNormalAttackSpeedMultiplier);
    }

    /// <summary>
    /// 일반 공속 배율 감소 함수
    /// </summary>
    /// <param name="rate">감소 배율</param>
    public void DecreaseNormalAttackSpeedMultiplier(float rate)
    {
        _runtimeData.PlusNormalAttackSpeedMultiplier = Mathf.Max(_runtimeData.PlusNormalAttackSpeedMultiplier - rate, 0);
    } 
    #endregion

    //==========================================================================================================================
    // Charge ==================================================================================================================
    //==========================================================================================================================
    
    #region Charge
    /// <summary>
    /// 차지 레벨 증가
    /// </summary>
    public void IncreaseChargeLevel()
    {
        _chargeLevel++;
    }

    /// <summary>
    /// 차지 레벨 초기화
    /// </summary>
    public void ResetChargeLevel()
    {
        _chargeLevel = -1;
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
    // Special Attack ==========================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 특수 공격 스크립터블 오브젝트 설정
    /// </summary>
    /// <param name="specialAttackSO">특수 공격</param>
    public void SetSpecialAttackSO(CanSpecialAttackSO specialAttackSO)
    {
        _specialAttackSO = specialAttackSO;
        if (_runtimeData != null && specialAttackSO != null)
        {
             _runtimeData.CurrentSpecialAttackId = specialAttackSO.Id;
        }
    }

    /// <summary>
    /// 특수 공격 스크립터블 오브젝트 초기화
    /// </summary>
    public void ClearSpecialAttackSO()
    {
        _specialAttackSO = null;
        if (_runtimeData != null)
        {
            _runtimeData.CurrentSpecialAttackId = "";
        }
    }

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

    /// <summary>
    /// 데미지 받기 전 이벤트 발행
    /// </summary>
    /// <param name="damageContext">받은 데미지 데이터</param>
    private void OnBeforeDamaged(ref PlayerDamageContext damageContext)
    {
        DamageData damageData = damageContext.Data;

        Vector3 toEnemy = damageData.AttackerTransform.transform.position - transform.position;        // 적으로 가는 벡터 구하기
        // 적을 마주보고 있는가
        bool isFacingEnemy = Vector3.Angle(transform.forward, toEnemy) <= (CounterAngle / 2f);


        // 공격 타입이 Heavy일 때 차징했거나, 공격 타입이 Normal인가
        bool validateAttackType = damageData.AttackType >= AttackType.Heavy1 && ChargeLevel >= 0 || damageData.AttackType == AttackType.Normal;


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
}