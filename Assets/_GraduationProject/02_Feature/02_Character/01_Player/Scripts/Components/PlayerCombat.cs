using BH_Lib.Log;
using Pathfinding.Drawing;
using System;
using UnityEngine;


public class PlayerCombat : MonoBehaviour, IAttacker
{
    #region Private Fields
    private PlayerCombatData _combatData;
    /// <summary>
    /// 전투 중심점의 위치
    /// </summary>
    private Vector3 _combatCenter;
    /// <summary>
    /// 카운터 공격 가능 여부
    /// </summary>
    private bool _canCounterAttack;
    /// <summary>
    /// 마지막 전투 시간
    /// </summary>
    private float _lastBattleTime;
    /// <summary>
    /// 전투 중이 아닌지 여부
    /// </summary>
    private bool _isBattleState;

    /// <summary>
    /// 디버깅용 기즈모 표시 여부
    /// </summary>
    private bool _isDrawGizmos = false;
    #endregion

    #region Properties
    public bool CanCounterAttack => _canCounterAttack;
    public float LastBattleTime => _lastBattleTime;
    public bool IsBattleState => _isBattleState;
    #endregion

    public void Initialize(PlayerCombatData combatData)
    {
        _isDrawGizmos = true;
        _combatData = combatData;
    }

    /// <summary>
    /// 전투 중심점을 설정
    /// </summary>
    public void SetupCombatCenter()
    {
        _combatCenter = transform.position;
    }

    #region BattleState
    /// <summary>
    /// 마지막 전투 시간 설정
    /// </summary>
    public void SetupBattleTime()
    {
        _lastBattleTime = Time.time;
    }
    /// <summary>
    /// 전투 중 상태 변경 함수
    /// </summary>
    /// <param name="isBattleState"></param>
    public void SetBattleState(bool isBattleState)
    {
        _isBattleState = isBattleState;
    }
    #endregion

    #region Attack
    /// <summary>
    /// 공격 중심점을 계산
    /// </summary>
    /// <returns>공격 박스의 중심 위치</returns>
    private Vector3 GetAttackCenter(PlayerAttackData attackData)
    {
        return _combatCenter + transform.forward * (attackData.AttackRadius.z / 2);
    }

    /// <summary>
    /// 공격 실행 (일반/차지 공격 등 공격 처리)
    /// Physics.OverlapBox를 사용하여 박스 범위 내의 적을 감지합니다.
    /// </summary>
    /// <param name="attackData">공격 데이터</param>
    /// <returns>타격한 대상의 콜라이더 배열</returns>
    public Collider[] ExecuteAttack(PlayerAttackData attackData)
    {
        // 공격 중심점과 범위 계산
        Vector3 attackCenter = GetAttackCenter(attackData);
        Vector3 halfExtents = attackData.AttackRadius / 2f;  // OverlapBox에 필요한 halfExtents 계산

        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _combatData.AttackLayerMask);

        Log.Print(hitEnemies.Length);
        ProcessHitEnemies(attackData, hitEnemies);

        return hitEnemies;
    }

    /// <summary>
    /// 공격에 맞은 적들에 대한 처리
    /// </summary>
    /// <param name="attackData">공격 데이터</param>
    /// <param name="hitObjects">타격한 대상의 콜라이더 배열</param>
    private void ProcessHitEnemies(PlayerAttackData attackData, Collider[] hitObjects)
    {
        foreach (Collider obj in hitObjects)
        {
            IDamageable damageable = obj.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                damageable.TakeDamage(attackData.AttackDamage, this);
            }
        }
    }
    #endregion

    #region RangedAttack
    /// <summary>
    /// 원거리 공격 실행
    /// </summary>
    /// <param name="firePoint">발사 지점</param>
    public void FireProjectile(Transform firePoint)
    {
        if (_combatData.RangedAttackData.ProjectilePrefab == null)
        {
            return;
        }

        GameObject projectileObj = Instantiate(_combatData.RangedAttackData.ProjectilePrefab,
            firePoint.position, firePoint.rotation);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(_combatData.RangedAttackData.AttackDamage,
                _combatData.RangedAttackData.ProjectileSpeed, gameObject, _combatData.AttackLayerMask);
        }
    }
    #endregion

    #region Parry
    /// <summary>
    /// 패리(방어) 실행
    /// </summary>
    /// <param name="parryRadius">패리 범위</param>
    /// <returns>패리에 영향을 받은 대상의 콜라이더 배열</returns>
    public Collider[] ExecuteParry(Vector3 parryRadius)
    {
        // 공격 중심점과 범위 계산
        Vector3 attackCenter = _combatCenter + transform.forward * (parryRadius.z / 2);
        Vector3 halfExtents = parryRadius / 2f;  // OverlapBox에 필요한 halfExtents 계산

        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _combatData.AttackLayerMask);

        ProcessParryEnemies(hitEnemies);

        return hitEnemies;
    }

    /// <summary>
    /// 패리 성공 시 적들에 대한 처리
    /// </summary>
    /// <param name="hitObjects">타격한 대상의 콜라이더 배열</param>
    private void ProcessParryEnemies(Collider[] hitObjects)
    {
        foreach (Collider obj in hitObjects)
        {
            IParryable parryable = obj.GetComponent<IParryable>();
            if (parryable != null && parryable.IsParryable)
            {
                parryable.Parry(gameObject);
            }
        }
    }
    #endregion

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (!_isDrawGizmos)
        {
            return;
        }

        DrawActionGizmo(_combatData.AttackDatas[0].AttackRadius, Color.mediumVioletRed);
        DrawActionGizmo(_combatData.AttackDatas[1].AttackRadius, Color.orangeRed);
        DrawActionGizmo(_combatData.AttackDatas[2].AttackRadius, Color.darkRed);
        DrawActionGizmo(_combatData.ChargeAttackData.AttackRadius, Color.indianRed);
        DrawActionGizmo(_combatData.ParryRadius, Color.green);
    }

    private void DrawActionGizmo(Vector3 radius, Color color)
    {
        Vector3 attackCenter = transform.position + transform.forward * (radius.z / 2);
        Gizmos.color = color;
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, radius);
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}

public class PlayerCombatManager : IDisposable
{
    [SerializeField] private PlayerCombat _combat;
    [SerializeField] private PlayerEvents _events;

    public PlayerCombatManager(PlayerCombat combat, PlayerEvents events)
    {
        _combat = combat;
        _events = events;

        _events.OnRangedAttackStart += HandleRangedAttack;
        _events.OnBattleStateChaged += HandleBattleStateChanged;
    }

    public void Dispose()
    {
        _events.OnRangedAttackStart -= HandleRangedAttack;
        _events.OnBattleStateChaged -= HandleBattleStateChanged;
    }

    private void HandleRangedAttack(Transform firePoint)
    {
        _combat.FireProjectile(firePoint);
    }

    private void HandleBattleStateChanged(bool isBattleState)
    {
        if (isBattleState)
        {
            _combat.SetupBattleTime();
            _combat.SetBattleState(isBattleState);
        }
        else
        {
            _combat.SetBattleState(isBattleState);
        }
    }
}
