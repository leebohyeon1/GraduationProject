using System.Collections;
using BH_Lib.Log;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCombat : MonoBehaviour, IPlayerCombat
{
    #region Serialized Fields
    [SerializeField] private LayerMask _attackLayerMask = 1 << 8;

    /// <summary>
    /// 패링 이펙트 위치
    /// </summary>
    [Space(10)]
    [SerializeField] private Transform _parryStartEffectPoint;

    /// <summary>
    /// 카운터 공격 이펙트 위치
    /// </summary>
    [Space(10)]
    [SerializeField] private Transform _counterAttackStartEffectPoint;
    [SerializeField] private Transform _firstCounterAttackEffectPoint;
    [SerializeField] private Transform _SecondCounterAttackEffectPoint;
    [SerializeField] private Transform _counterAttackFinishEffectPoint;

    /// <summary>
    /// 차지 공격 이펙트 위치
    /// </summary>
    [Space(10)]
    [SerializeField] private Transform _chargeStartEffectPoint;
    [SerializeField] private Transform _chargeFinishEffectPoint;
    [SerializeField] private Transform _chargeAttackStartEffectPoint;
    [SerializeField] private Transform _chargeAttackEffectPoint;
    [SerializeField] private Transform _chargeAttackFinishEffectPoint;
    #endregion

    #region Private Fields
    private PlayerContext _context;
    private PlayerEventChannel _event;
    private bool _canCounterAttack;
    private float _lastActionTime;
    private Coroutine _counterAttackCoroutine;
    private Collider _counterObject;

    #endregion

    #region Properties
    public PlayerMeleeAttackData MeleeAttackData => _context.Stats.CounterAttackData;
    public bool CanCounterAttack => _canCounterAttack && ScanCounterable();
    public bool IsRest => (Time.time - _lastActionTime) >= 8.0f;
    public int MaxMana => throw new System.NotImplementedException();
    public int CurrentMana => throw new System.NotImplementedException();

    public Transform ParryStartEffectPoint => _parryStartEffectPoint;

    public Transform CounterAttackStartEffectPoint => _counterAttackStartEffectPoint;
    public Transform FirstCounterAttackEffectPoint => _firstCounterAttackEffectPoint;
    public Transform SecondCounterAttackEffectPoint => _SecondCounterAttackEffectPoint;
    public Transform CounterAttackFinishEffectPoint => _counterAttackFinishEffectPoint;


    public Transform ChargeStartEffectPoint => _chargeStartEffectPoint;
    public Transform ChargeFinishEffectPoint => _chargeFinishEffectPoint;

    public Transform ChargeAttackStartEffectPoint => _chargeAttackStartEffectPoint;
    public Transform ChargeAttackEffectPoint => _chargeAttackEffectPoint;
    public Transform ChargeAttackFinishEffectPoint => _chargeAttackFinishEffectPoint;


    #endregion

    public void Initialize(PlayerContext context)
    {
        _context = context;
        _event = _context.Event;

        // 전투 관련 이벤트 버스 구독
        _event.Parry.OnPerform += HandleParryPerform;
        _event.Parry.OnAffect += HandleParryAffect;

        _event.CounterAttack.OnPerform += HandleCounterAttackPerform;

        // 행동 관련 이벤트 버스 구독
        _event.MeleeAttack.OnStart += HandleActionStart;
        _event.MeleeAttackCharge.OnStart += HandleActionStart;
        _event.MeleeAttackCharge.OnPerform += HandleActionStart;
        _event.ChargeMeleeAttack.OnStart += HandleActionStart;
        _event.RangedAttackCharge.OnStart += HandleActionStart;
        _event.RangedAttack.OnStart += HandleActionStart;
        _event.CounterAttack.OnStart += HandleActionStart;
        _event.Parry.OnStart += HandleActionStart;
        _event.OnDefendHit += ResetRestTimer;
        _event.OnNomalHit += ResetRestTimer;
        _event.OnStrongHit += ResetRestTimer;
    }
    private void OnDisable()
    {
        _event.Parry.OnPerform -= HandleParryPerform;
        _event.Parry.OnAffect -= HandleParryAffect;

        _event.CounterAttack.OnPerform -= HandleCounterAttackPerform;

        _event.MeleeAttack.OnStart -= HandleActionStart;
        _event.MeleeAttackCharge.OnStart -= HandleActionStart;
        _event.MeleeAttackCharge.OnPerform -= HandleActionStart;
        _event.ChargeMeleeAttack.OnStart -= HandleActionStart;
        _event.RangedAttackCharge.OnStart -= HandleActionStart;
        _event.RangedAttack.OnStart -= HandleActionStart;
        _event.CounterAttack.OnStart -= HandleActionStart;
        _event.Parry.OnStart -= HandleActionStart;
        _event.OnDefendHit -= ResetRestTimer;
        _event.OnNomalHit -= ResetRestTimer;
        _event.OnStrongHit -= ResetRestTimer;
    }

    #region Feedback Handlers
    private void HandleParryPerform(Vector3 position)
    {
        TryParry();
    }

    private void HandleParryAffect(Vector3 position, Collider collider)
    {
        EnterCounterAttackStance();
    }

    private void HandleCounterAttackPerform(Vector3 position)
    {
        TryCounterAttack();
    }

    private void HandleActionStart(Vector3 position)
    {
        ResetRestTimer();
    }
    #endregion

    #region Parry
    /// <summary>
    /// 패링 시도 (패링 가능한 적의 공격을 반격)
    /// 애니메이션 이벤트에서 호출됩니다.
    /// </summary>
    public void TryParry()
    {
        if (_context?.Stats == null) return;

        Vector3 parryCenter = GetParryCenter();
        Collider[] hitEnemies = Physics.OverlapBox(parryCenter, _context.Stats.ParryRadius / 2, transform.rotation, _attackLayerMask);

        ProcessParryableEnemies(hitEnemies);
    }

    /// <summary>
    /// 패링 범위의 중심점 계산
    /// </summary>
    /// <returns>패링 범위 박스의 중심 위치</returns>
    private Vector3 GetParryCenter()
    {
        return transform.position + transform.forward * (_context.Stats.ParryRadius.z / 2);
    }

    /// <summary>
    /// 패링 범위 내 감지된 적들에게 패링 적용
    /// </summary>
    /// <param name="hitEnemies">감지된 적들의 Collider 배열</param>
    private void ProcessParryableEnemies(Collider[] hitEnemies)
    {
        foreach (Collider enemy in hitEnemies)
        {
            IParryable parryable = enemy.GetComponent<IParryable>();
            if (parryable != null && parryable.IsParryable)
            {
                parryable.Parry(gameObject);
                _event.Parry.PublishAffect(enemy.transform.position, enemy);
            }
        }
    }
    #endregion

    #region CounterAttack

    /// <summary>
    /// 코루틴 작동
    /// </summary>
    public void EnterCounterAttackStance()
    {

        if (_counterAttackCoroutine != null)
        {
            StopCoroutine(_counterAttackCoroutine);
        }

        _counterAttackCoroutine = StartCoroutine(CoCounterAttackWindow());
    }

    /// <summary>
    /// 패링 가능 상태로 변경 후 해제
    /// </summary>
    /// <returns></returns>
    public IEnumerator CoCounterAttackWindow()
    {
        SetCanCounterAttack(true);

        yield return new WaitForSeconds(_context.Stats.ParryCounterWindow);

        SetCanCounterAttack(false);
    }

    /// <summary>
    /// 카운터 공격 가능 여부 설정
    /// </summary>
    /// <param name="value">가능 여부</param>
    public void SetCanCounterAttack(bool value)
    {
        _canCounterAttack = value;
    }

    public bool ScanCounterable()
    {
        Vector3 counterCenter = GetCounterCenter();
        Collider[] hits = Physics.OverlapBox(counterCenter, MeleeAttackData.AttackRadius / 2, transform.rotation, _attackLayerMask);
        foreach (Collider hit in hits)
        {
            ICounterable counterable = hit.GetComponent<ICounterable>();
            if (counterable != null && counterable.IsCounterable)
            {
                _counterObject = hit;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 카운터 공격 시도
    /// </summary>
    public void TryCounterAttack()
    {
        ProcessHitEnemies(_counterObject);
    }

    private void ProcessHitEnemies(Collider hitObjects)
    {
        SetCanCounterAttack(false);

        IDamageable damageable = hitObjects.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(MeleeAttackData.AttackDamage, _context.MeleeAttack);
            _event.CounterAttack.PublishAffect(_firstCounterAttackEffectPoint.position, hitObjects);
        }

        ICounterable counterable = hitObjects.GetComponent<ICounterable>();
        if (counterable != null)
        {
            counterable.ExecuteCounterEffect();
        }

        _counterObject = null;
    }

    /// <summary>
    /// 패링 범위의 중심점 계산
    /// </summary>
    /// <returns>패링 범위 박스의 중심 위치</returns>
    private Vector3 GetCounterCenter()
    {
        return transform.position + transform.forward * (MeleeAttackData.AttackRadius.z / 2);
    }

    #endregion

    #region Rest
    private void ResetRestTimer()
    {
        _lastActionTime = Time.time;
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_context?.Stats == null) return;

        DrawParryGizmo();

        Vector3 counterCenter = GetCounterCenter();
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(counterCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, MeleeAttackData.AttackRadius);
        Gizmos.matrix = Matrix4x4.identity;
    }
    private void DrawParryGizmo()
    {
        Vector3 parryCenter = GetParryCenter();
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(parryCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, _context.Stats.ParryRadius);
        Gizmos.matrix = Matrix4x4.identity;
    }

#endif

}
