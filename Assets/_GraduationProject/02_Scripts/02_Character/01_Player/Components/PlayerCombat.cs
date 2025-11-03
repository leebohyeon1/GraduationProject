using BH_Lib.Log;
using DG.Tweening;
using Pathfinding.Drawing;
using System;
using UnityEngine;


/// <summary>
/// 플레이어의 전투 관련 로직을 담당하는 컴포넌트입니다.
/// </summary>
public class PlayerCombat : MonoBehaviour, IDisposable, IEventListener<bool>    
{
    #region Private Fields
    private PlayerStats _stats; // 플레이어 스탯
    private PlayerEvents _events; // 플레이어 이벤트
    
    /// <summary>
    /// 전투 중심점의 위치
    /// </summary>
    private Vector3 _combatCenter;

    /// <summary>
    /// 카운터 공격 가능 여부
    /// </summary>
    private bool _canCounterAttack;
    /// <summary>
    /// 카운터 공격 가능 오브젝트
    /// </summary>
    private GameObject _counterableTarget = null;
    
    /// <summary>
    /// 마지막 전투 시간
    /// </summary>
    private float _lastBattleTime;
    /// <summary>
    /// 전투 중인지 여부
    /// </summary>
    private bool _isBattleState;

    /// <summary>
    /// 디버깅용 기즈모 표시 여부
    /// </summary>
    private bool _isDrawGizmos = false;

    [SerializeField] private OnParry _onParry;
    #endregion

    #region Properties
    public bool CanCounterAttack => _canCounterAttack; // 카운터 공격 가능 여부
    public GameObject CounterableTarget => _counterableTarget; // 카운터 공격 대상

    public float LastBattleTime => _lastBattleTime; // 마지막 전투 시간
    public bool IsBattleState => _isBattleState; // 전투 상태 여부
    #endregion

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerStats combatData, PlayerEvents events)
    {
        _isDrawGizmos = true;
        _stats = combatData;
        _events = events;

        _events.OnRangedAttackStart += HandleRangedAttack;
        _events.OnBattleStateChaged += HandleBattleStateChanged;
        _events.OnAttackStart += SetupCombatCenter;
        _events.OnParryPerform += SetupCombatCenter;

        _onParry.Subscribe(this);
    }

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
        _events.OnRangedAttackStart -= HandleRangedAttack;
        _events.OnBattleStateChaged -= HandleBattleStateChanged;
        _events.OnAttackStart -= SetupCombatCenter;
        _events.OnParryPerform -= SetupCombatCenter;

        _onParry.Unsubscribe(this);
    }

    /// <summary>
    /// 전투 중심점을 설정합니다.
    /// </summary>
    public void SetupCombatCenter()
    {
        _combatCenter = transform.position;
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
    private Vector3 GetAttackCenter(PlayerAttackData attackData)
    {
        return _combatCenter + transform.forward * (attackData.AttackRadius.z / 2);
    }

    /// <summary>
    /// 공격을 실행합니다. (일반/차지 공격 등)
    /// Physics.OverlapBox를 사용하여 박스 범위 내의 적을 감지합니다.
    /// </summary>
    /// <param name="attackData">공격 데이터</param>
    /// <returns>타격한 대상의 콜라이더 배열</returns>
    public Collider[] ExecuteAttack(PlayerAttackData attackData)
    {
        Vector3 attackCenter = GetAttackCenter(attackData);
        Vector3 halfExtents = attackData.AttackRadius / 2f;

        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _stats.BasePlayerDatasSO.CombatData.AttackLayerMask);

        ProcessHitEnemies(attackData, hitEnemies);

        return hitEnemies;
    }

    /// <summary>
    /// 공격에 맞은 적들에게 데미지를 입힙니다.
    /// </summary>
    /// <param name="attackData">공격 데이터</param>
    /// <param name="hitObjects">타격한 대상의 콜라이더 배열</param>
    private void ProcessHitEnemies(PlayerAttackData attackData, Collider[] hitObjects)
    {
        foreach (Collider obj in hitObjects)
        {
            if (obj.TryGetComponent<IDamageable>(out var damageable) && !damageable.IsDead)
            {
                damageable.TakeDamage(attackData.AttackDamage,0,new DamageData(0, transform, 
                    attackData.KnockBackCurve, attackData.KnockBackDuration, attackData.KnockBackForce));
            }
        }
    }
    #endregion

    #region RangedAttack
    /// <summary>
    /// 원거리 공격을 실행합니다.
    /// </summary>
    /// <param name="firePoint">발사 지점</param>
    public void FireProjectile(Transform firePoint)
    {
        if (_stats.BasePlayerDatasSO.CombatData.RangedAttackData.ProjectilePrefab == null) return;

        //GameObject projectileObj = Instantiate(_stats.BasePlayerDatasSO.CombatData.RangedAttackData.ProjectilePrefab, firePoint.position, firePoint.rotation);

        //if (projectileObj.TryGetComponent<Projectile>(out var projectile))
        //{
        //    projectile.Initialize(_stats.RangedAttackData.AttackDamage, _stats.RangedAttackData.ProjectileSpeed, gameObject, _stats.BasePlayerDatasSO.CombatData.AttackLayerMask);
        //}
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
    /// <summary>
    /// 패리를 실행합니다.
    /// </summary>
    /// <param name="parryRadius">패리 범위</param>
    /// <returns>패리에 영향을 받은 대상의 콜라이더 배열</returns>
    public Collider[] ExecuteParry(Vector3 parryRadius)
    {
        Vector3 attackCenter = _combatCenter + transform.forward * (parryRadius.z / 2);
        Vector3 halfExtents = parryRadius / 2f;

        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _stats.BasePlayerDatasSO.CombatData.AttackLayerMask);

        return hitEnemies;
    }

    #endregion

    #region CounterAttack
    /// <summary>
    /// 일정 시간 동안 카운터 공격 가능 상태를 활성화/비활성화합니다.
    /// </summary>
    /// <param name="delayTime">활성화 유지 시간</param>
    public void ToggleCanCounter(float delayTime)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => SetCanCounterAttack(true));
        sequence.AppendInterval(delayTime);
        sequence.AppendCallback(() => SetCanCounterAttack(false));
        sequence.Play();
    }

    /// <summary>
    /// 카운터 공격 가능 여부를 설정합니다.
    /// </summary>
    /// <param name="canCounterAttack">가능 여부</param>
    public void SetCanCounterAttack(bool canCounterAttack)
    {
        _canCounterAttack = canCounterAttack;
    }

    /// <summary>
    /// 주변에 카운터 가능한 적이 있는지 확인합니다.
    /// </summary>
    /// <returns>카운터 가능한 적 존재 여부</returns>
    public bool CanIsScanCounterable()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, _stats.CounterAttackDatas[0].AttackRadius / 2, transform.rotation, _stats.BasePlayerDatasSO.CombatData.AttackLayerMask);

        float minDistance = Mathf.Infinity;
        Collider closestCollider = null;
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<ICounterable>(out var counterable) && counterable.IsCounterable)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = col;
                }
            }
        }

        if (closestCollider != null)
        {
            _counterableTarget = closestCollider.gameObject;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 첫 번째 카운터 공격을 실행합니다.
    /// </summary>
    public void ExcuteFirstCounterAttack(PlayerAttackData attackData)
    {
        if (_counterableTarget != null)
        {
            _counterableTarget.GetComponent<ICounterable>().ExecuteCounterEffect();

            if (_counterableTarget.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackData.AttackDamage,0, new DamageData(0, transform, 
                    attackData.KnockBackCurve, attackData.KnockBackDuration, attackData.KnockBackForce));
            }
        }
    }

    /// <summary>
    /// 두 번째 카운터 공격을 실행합니다. (현재 미구현)
    /// </summary>
    public void ExcuteSecondCounterAttack(PlayerAttackData attackData)
    {
        // TODO: 두 번째 카운터 공격 로직 구현
    }

    /// <summary>
    /// 카운터 공격 대상을 초기화합니다.
    /// </summary>
    public void ClearCounterTarget()
    {
        _counterableTarget = null;
    }

    #endregion

    private void HandleRangedAttack(Transform firePoint)
    {
        FireProjectile(firePoint);
    }

    private void HandleBattleStateChanged(bool isBattleState)
    {
        if (isBattleState)
        {
            SetupBattleTime();
        }
        SetBattleState(isBattleState);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!_isDrawGizmos) return;

        // 공격 범위 기즈모
        DrawActionGizmo(_stats.AttackDatas[0].AttackRadius, Color.mediumVioletRed);
        DrawActionGizmo(_stats.AttackDatas[1].AttackRadius, Color.orangeRed);
        DrawActionGizmo(_stats.AttackDatas[2].AttackRadius, Color.darkRed);
        DrawActionGizmo(_stats.ChargeAttackData.AttackRadius, Color.indianRed);
        DrawActionGizmo(_stats.BasePlayerDatasSO.CombatData.ParryRadius, Color.green);
        DrawActionGizmo(_stats.CounterAttackDatas[0].AttackRadius, Color.beige);
    }

    private void DrawActionGizmo(Vector3 radius, Color color)
    {
        Vector3 attackCenter = transform.position + transform.forward * (radius.z / 2);
        Gizmos.color = color;
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, radius);
        Gizmos.matrix = Matrix4x4.identity;
    }

    public void OnEventTrigger(bool eventName)
    {
        _events.PlayFeedback(PlayerFeedbackType.ParrySuccess_FB);
    }
#endif
}