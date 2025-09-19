using System.Collections;
using BH_Lib.Log;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombat : MonoBehaviour, IPlayerCombat
{   
    private PlayerContext _context;
    private PlayerEventChannel _event;
    [SerializeField] private LayerMask _attackLayerMask = 1 << 8;
    private bool _canCounterAttack;
    private Vector3 _attackCenter;
    private Coroutine _counterAttackCoroutine;


    public PlayerMeleeAttackData MeleeAttackData => _context.Stats.CounterAttackData;
    public bool CanCounterAttack => _canCounterAttack;

    public void Initialize(PlayerContext context)
    {
        _context = context;
        _event = _context.Event;

        // 전투 관련 이벤트 버스 구독
        _event.Parry.OnPerform += TryParry;                           // 패링 시도 이벤트
        _event.Parry.OnAffect += (collider) => EnterCounterAttackStance();

        _event.CounterAttack.OnStart += () => SetAttackCenter(transform.position);
        _event.CounterAttack.OnPerform += TryCounterAttack;
    }

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
                _event.Parry.PublishAffect(enemy);
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

    /// <summary>
    /// 카운터 공격 시도
    /// </summary>
    public void TryCounterAttack()
    {
        Vector3 attackCenter = GetAttackCenter();
        
        Collider[] hitEnemies = Physics.OverlapSphere(attackCenter, MeleeAttackData.AttackRadius.x, _attackLayerMask);

        ProcessHitEnemies(hitEnemies);
    }

    private void ProcessHitEnemies(Collider[] hitObjects)
    {
        SetCanCounterAttack(false);
        
        foreach (Collider obj in hitObjects)
        {
            IDamageable damageable = obj.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                damageable.TakeDamage(MeleeAttackData.AttackDamage, _context.MeleeAttack);
                _event.CounterAttack.PublishAffect(obj);
            }
        }
    }

    private void SetAttackCenter(Vector3 position)
    {
        _attackCenter = position;
    }

    private Vector3 GetAttackCenter()
    {
        return _attackCenter;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_context?.Stats == null) return;

        DrawParryGizmo();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, MeleeAttackData.AttackRadius.x);
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
