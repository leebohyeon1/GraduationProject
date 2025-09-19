using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 공격 시스템을 담당하는 컴포넌트
/// 근거리 공격, 원거리 공격, 패링 기능을 제공합니다.
/// </summary>
public class PlayerMeleeAttack : MonoBehaviour, IPlayerMeleeAttack
{
    #region Serialized Fields

    [Header("Combat Settings")]
    [Tooltip("적 레이어 마스크 (공격 대상 감지용)")]
    [SerializeField] private LayerMask _attackLayerMask = 1 << 8;
    
    #endregion

    #region Private Fields

    /// <summary>공격 범위의 중심점 위치</summary>
    private Vector3 _attackCenter;
    
    /// <summary>현재 콤보 카운트 (0부터 시작)</summary>
    private int _comboCount = 0;
    
    /// <summary>
    /// 차지 근접 공격 수행 여부 플래그
    /// </summary>
    private bool _isPerformingChargeAttack = false;
    
    /// <summary>플레이어 컨텍스트 참조 (스탯, 이벤트버스 등에 액세스)</summary>
    private PlayerContext _context;

    private PlayerEventChannel _event;
    #endregion

    #region Properties
    /// <summary>
    /// 현재 차지 공격 수행 여부
    /// </summary>
    public bool IsPerformingChargeAttack => _isPerformingChargeAttack;

    /// <summary>
    /// 근거리 공격 데이터 (콤보 또는 차지 상태에 따라 달라짐)
    /// </summary>
    public PlayerMeleeAttackData MeleeAttackData =>
        IsPerformingChargeAttack 
            ? _context?.Stats?.ChargeMeleeAttackData
            : _context?.Stats?.AttackData?[_comboCount];

    /// <summary>
    /// 현재 콤보 또는 차지 상태에 따른 공격 데미지
    /// </summary>
    public int AttackDamage => MeleeAttackData.AttackDamage;

    /// <summary>
    /// 현재 콤보 카운트
    /// </summary>
    public int ComboCount => _comboCount;

    /// <summary>
    /// 공격 범위 중심점
    /// </summary>
    public Vector3 AttackCenter => _attackCenter;

    #endregion

    #region Public Methods

    /// <summary>
    /// 플레이어 전투 시스템 초기화
    /// 이벤트 버스 구독 및 컨텍스트 설정을 수행합니다.
    /// </summary>
    /// <param name="context">플레이어의 전반적인 컨텍스트 (스탯, 이벤트버스 등)</param>
    public void Initialize(PlayerContext context)
    {
        _context = context;
        _event = _context.Event;

        _event.MeleeAttack.OnStart += SetAttackCenter;
        _event.MeleeAttack.OnPerform += PerformAttack;              // 일반 공격 수행 이벤트
       
        _event.MeleeAttackCharge.OnStart += () => SetIsPerformingChargeAttack(true);  // 차지 시작
        _event.ChargeMeleeAttack.OnStart += SetAttackCenter;
        _event.ChargeMeleeAttack.OnPerform += PerformChargeMeleeAttack; // 차지 공격 수행 이벤트
        _event.ChargeMeleeAttack.OnFinished += () => SetIsPerformingChargeAttack(false);  // 공격 종료
    }

    /// <summary>
    /// 실제 공격 실행 (Physics.OverlapBox로 범위 내 적 감지 및 피해 적용)
    /// 애니메이션 이벤트에서 호출됩니다.
    /// </summary>
    public void PerformAttack()
    {
        ExecuteAttack();
        UpdateComboCount();
    }

    /// <summary>
    /// 실제 차지 공격 실행 (Physics.OverlapBox로 범위 내 적 감지 및 피해 적용)
    /// 애니메이션 이벤트에서 호출됩니다.
    /// </summary>
    public void PerformChargeMeleeAttack()
    {
        ExecuteAttack();
    }

    /// <summary>
    /// 공통 공격 실행 로직 (일반/차지 공격 둘 다 사용)
    /// Physics.OverlapBox를 사용하여 박스 형태의 공격 범위에서 적을 감지합니다.
    /// </summary>
    /// <param name="isChargeAttack">차지 공격 여부 (차지 공격시 다른 데이터 사용)</param>
    private void ExecuteAttack()
    {
        // 컨텍스트와 스탯 데이터 유효성 검사
        if (_context?.Stats == null) return;

        // 공격 중심점과 범위 설정
        Vector3 attackCenter = GetAttackCenter();
        Vector3 halfExtents = MeleeAttackData.AttackRadius / 2f;  // OverlapBox는 halfExtents를 사용

        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _attackLayerMask);

        ProcessHitEnemies(hitEnemies);
    }

    /// <summary>
    /// 특정 대상에게 공격 실행
    /// </summary>
    /// <param name="target">공격 대상</param>
    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead) return;

        target.TakeDamage(AttackDamage, this);
        Log.PrintColor(Color.red, $"플레이어가 {target}에게 {AttackDamage} 피해를 입혔습니다!");
    }

    /// <summary>
    /// 콤보 카운트 리셋 (공격 체인 종료 시 호출)
    /// </summary>
    public void ResetComboCount()
    {
        _comboCount = 0;
    }

    public void SetAttackCenter()
    {
        _attackCenter = transform.position;
    }
    #endregion
    
    #region Private Methods

    /// <summary>
    /// 공격 범위의 중심점 계산
    /// </summary>
    /// <returns>공격 범위 박스의 중심 위치</returns>
    private Vector3 GetAttackCenter()
    { 
        return _attackCenter + transform.forward * (MeleeAttackData.AttackRadius.z / 2);
    }

    /// <summary>
    /// 공격 범위 내 감지된 적들에게 피해 적용
    /// </summary>
    /// <param name="hitObjects">감지된 적들의 Collider 배열</param>
    private void ProcessHitEnemies(Collider[] hitObjects)
    {
        foreach (Collider obj in hitObjects)
        {
            IDamageable damageable = obj.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                Attack(damageable);

                // 감지된 적들에게 피해 적용 이벤트 발생
                if (_isPerformingChargeAttack)
                {
                    _event.ChargeMeleeAttack.PublishAffect(obj);
                }
                else
                {
                    _event.MeleeAttack.PublishAffect(obj);
                }
            }
        }
    }

    /// <summary>
    /// 콤보 카운트 증가 및 순환 처리
    /// 최대 콤보 수에 도달하면 0으로 초기화하여 콤보를 순환시킵니다.
    /// </summary>
    private void UpdateComboCount()
    {
        _comboCount++;  // 다음 콤보로 증가
        
        // 콤보 배열 범위를 초과하면 처음부터 다시 시작
        if (_comboCount >= _context.Stats.AttackData.Length)
        {
            _comboCount = 0;
        }
    }

    private void SetIsPerformingChargeAttack(bool value)
    {
        _isPerformingChargeAttack = value;
    }
    #endregion

#if UNITY_EDITOR

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (_context?.Stats == null) return;

        DrawAttackGizmo();
        DrawChargeAttackGizmo();
    }

    private void DrawAttackGizmo()
    {
        Vector3 attackCenter = transform.position + transform.forward * (MeleeAttackData.AttackRadius.z / 2);
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, MeleeAttackData.AttackRadius);
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void DrawChargeAttackGizmo()
    {
        Vector3 attackCenter = transform.position + transform.forward * (_context.Stats.ChargeMeleeAttackData.AttackRadius.z / 2);
        Gizmos.color = Color.darkRed;
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, _context.Stats.ChargeMeleeAttackData.AttackRadius);
        Gizmos.matrix = Matrix4x4.identity;
    }


    #endregion

#endif

}