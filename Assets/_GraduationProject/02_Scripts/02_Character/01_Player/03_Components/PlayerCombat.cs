using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 플레이어의 전투 관련 로직을 담당하는 컴포넌트입니다.
/// </summary>
public class PlayerCombat : MonoBehaviour, IDisposable
{
    [Header("References")]
    private PlayerDataSO _data;     // 플레이어 데이터
    private PlayerEvents _events;   // 플레이어 이벤트
    [SerializeField] private OnSwingMiss _onSwingMiss;  // 공격 미스 이벤트

    [Header("NormalAttack")]
    private int _normalAttackComboIndex = -1;    // 일반 공격 콤보 순서

    [Header("Charge")]
    private int _chargeLevel;   // 차지 레벨
    
    [Header("Counter")]
    private bool _isCounterable = false;          // 상쇄 가능 여부
    private HashSet<IParryable> _counterEnemySet = new HashSet<IParryable>();

    private float _lastBattleTime;  // 마지막 전투 시간
    private bool _isBattleState;    // 전투 중인지 여부

    [Header("Properties")]
    public float LastBattleTime => _lastBattleTime; // 마지막 전투 시간
    public bool IsBattleState => _isBattleState; // 전투 상태 여부
    public int NormalAttackComboIndex => _normalAttackComboIndex;
    public int ChargeLevel => _chargeLevel;

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerController player)
    {
        _data = player.Data;
        _events = player.Events;

        _events.BattleStateChaged += OnBattleStateChaged;
        _events.CounterWindowStarted += OnCounterWindowStarted;
        _events.CounterWindowFinished += OnCounterWindowFinished;
    }

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
        _events.BattleStateChaged -= OnBattleStateChaged;
        _events.CounterWindowStarted -= OnCounterWindowStarted;
        _events.CounterWindowFinished -= OnCounterWindowFinished;
    }

    #region BattleState
    /// <summary>
    /// 마지막 전투 시간을 현재 시간으로 설정합니다.
    /// </summary>
    public void SetupBattleTime()
    {
        _lastBattleTime = Time.time;
    }
    /// <summary>
    /// 전투 상태를 변경합니다.
    /// </summary>
    /// <param name="isBattleState">새로운 전투 상태</param>
    public void SetBattleState(bool isBattleState)
    {
        _isBattleState = isBattleState;
    }
    #endregion

    #region Attack
    /// <summary>
    /// 공격의 중심 위치를 계산합니다.
    /// </summary>
    /// <returns>공격 박스의 중심 위치</returns>
    private Vector3 GetAttackCenter(PlayerAttackConfig attackData)
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

        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _data.AttackLayerMask);

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

            if (obj.TryGetComponent<IDamageable>(out var damageable) && !damageable.IsDead)
            {
                damageable.TakeDamage(new DamageData(transform, attackData.AttackType, attackData.AttackDamage
                    ,0 , attackData.KnockbackCofig.StepCurve, attackData.KnockbackCofig.StepDuration, attackData.KnockbackCofig.StepDistance));
            }
        }
    }
    #endregion

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
        return _normalAttackComboIndex < (_data.NormalAttackConfigList.Count - 1);
    }
    #endregion

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
        _chargeLevel = 0;
    }
    #endregion

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
    #endregion

    #region EventHandle
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
    /// 전투 상태 변경
    /// </summary>
    /// <param name="isBattleState">전투 상태 여부</param>
    private void OnBattleStateChaged(bool isBattleState)
    {
        if (isBattleState)
        {
            SetupBattleTime();
        }

        SetBattleState(isBattleState);
    }
    #endregion
}