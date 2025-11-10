using BH_Lib.Log;
using DG.Tweening;
using Pathfinding.Drawing;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;


/// <summary>
/// 플레이어의 전투 관련 로직을 담당하는 컴포넌트입니다.
/// </summary>
public class PlayerCombat : MonoBehaviour, IDisposable
{
    #region Private Fields
    private PlayerStats _stats; // 플레이어 스탯
    private PlayerEvents _events; // 플레이어 이벤트

    [SerializeField] private OnSwingMiss _onSwingMiss;

    /// <summary>
    /// 마지막 전투 시간
    /// </summary>
    private float _lastBattleTime;
    /// <summary>
    /// 전투 중인지 여부
    /// </summary>
    private bool _isBattleState;

    #endregion

    #region Properties
    public float LastBattleTime => _lastBattleTime; // 마지막 전투 시간
    public bool IsBattleState => _isBattleState; // 전투 상태 여부
    #endregion

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerStats combatData, PlayerEvents events)
    {
        _stats = combatData;
        _events = events;

        _events.OnBattleStateChaged += HandleBattleStateChanged;
        _events.OnParryWindowStart += HandleParryWindowStart;
        _events.OnParryWindowFinish += HandleParryWindowFinish;
    }

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
        _events.OnBattleStateChaged -= HandleBattleStateChanged;
        _events.OnParryWindowStart -= HandleParryWindowStart;
        _events.OnParryWindowFinish -= HandleParryWindowFinish;
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
    private Vector3 GetAttackCenter(PlayerAttackDataSO attackData)
    {
        return transform.position + transform.forward * (attackData.AttackRadius.z / 2);
    }

    /// <summary>
    /// 공격을 실행합니다. (일반/차지 공격 등)
    /// Physics.OverlapBox를 사용하여 박스 범위 내의 적을 감지합니다.
    /// </summary>
    /// <param name="attackData">공격 데이터</param>
    /// <returns>타격한 대상의 콜라이더 배열</returns>
    public Collider[] ExecuteAttack(PlayerAttackDataSO attackData)
    {
        Vector3 attackCenter = GetAttackCenter(attackData);
        Vector3 halfExtents = attackData.AttackRadius / 2f;

        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _stats.Data.CombatData.AttackLayerMask);

        if (hitEnemies.Length > 0)
        {
            ProcessHitEnemies(attackData, hitEnemies);
        }
        else
        {
            _onSwingMiss.Publish(true);
        }
        
        return hitEnemies;
    }

    /// <summary>
    /// 공격에 맞은 적들에게 데미지를 입힙니다.
    /// </summary>
    /// <param name="attackData">공격 데이터</param>
    /// <param name="hitObjects">타격한 대상의 콜라이더 배열</param>
    private void ProcessHitEnemies(PlayerAttackDataSO attackData, Collider[] hitObjects)
    {
        foreach (Collider obj in hitObjects)
        {
            if (obj.TryGetComponent<IParryable>(out var parryable) && _stats.ParrySet.Contains(parryable))
            {
                continue;
            }

            if (obj.TryGetComponent<IDamageable>(out var damageable) && !damageable.IsDead)
            {
                damageable.TakeDamage(new DamageData(transform, attackData.AttackType, attackData.AttackDamage
                    ,attackData.StiffnessAmount, attackData.KnockBackCurve, attackData.KnockBackDuration, attackData.KnockBackForce));
            }
        }
    }
    #endregion

    #region Defend
    /// <summary>
    /// 방어 상태를 설정합니다.
    /// </summary>
    /// <param name="isDefending">방어 여부</param>
    public void SetDefending(bool isDefending)
    {
        _stats.IsDefending = isDefending;
    }
    #endregion

    #region Parry
   

    #endregion

    private void HandleParryWindowStart()
    {
        _stats.IsParring = true;    
    }

    private void HandleParryWindowFinish()
    {
        _stats.IsParring = false;
    }

    private void HandleBattleStateChanged(bool isBattleState)
    {
        if (isBattleState)
        {
            SetupBattleTime();
        }
        SetBattleState(isBattleState);
    }

}