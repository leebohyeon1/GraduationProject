using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 공격 시스템을 담당하는 컴포넌트
/// 근거리 공격, 원거리 공격, 패링 기능을 제공합니다.
/// </summary>
public class PlayerRangedAttack : MonoBehaviour, IPlayerRangedAttack
{
    #region Serialized Fields

    [Header("Combat Settings")]
    [Tooltip("원거리 공격 투사체 발사 위치")]
    [SerializeField] private Transform _rangedAttackPoint;
    
    [Tooltip("적 레이어 마스크 (공격 대상 감지용)")]
    [SerializeField] private LayerMask _attackLayerMask = 1 << 8;
    
    [Tooltip("원거리 공격에 사용할 투사체 프리팹")]
    [SerializeField] private GameObject _projectilePrefab;

    #endregion

    #region Private Fields
    /// <summary>플레이어 컨텍스트 참조 (스탯, 이벤트버스 등에 액세스)</summary>
    private PlayerContext _context;

    private PlayerEventChannel _event;
    #endregion

    #region Properties
    /// <summary>
    /// 원거리 공격 데미지
    /// </summary>
    public int RangedAttackDamage => _context?.Stats?.RangedAttackData.AttackDamage ?? 10;

    /// <summary>
    /// 원거리 공격 차징 시간
    /// </summary>
    public float RangedAttackChargeTime => _context?.Stats?.RangedAttackData.RangedAttackChargeTime ?? 3.0f;

    /// <summary>
    /// 투사체 속도
    /// </summary>
    public float ProjectileSpeed => _context?.Stats?.RangedAttackData.ProjectileSpeed ?? 100.0f;
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

        // 전투 관련 이벤트 버스 구독
        _event.RangedAttack.OnPerform += FireProjectile;         // 원거리 공격 시작 이벤트
    }

    /// <summary>
    /// 원거리 공격 투사체 발사
    /// 애니메이션 이벤트 또는 이벤트 버스에서 호출됩니다.
    /// </summary>
    public void FireProjectile()
    {
        if (_projectilePrefab == null || _rangedAttackPoint == null)
        {
            Log.Print("투사체 프리팹 또는 발사 지점이 설정되지 않았습니다!");
            return;
        }

        Log.PrintColor(Color.red, $"투사체 발사! 데미지: {RangedAttackDamage}, 속도: {ProjectileSpeed}");

        // 투사체 생성
        GameObject projectileObj = Instantiate(_projectilePrefab, _rangedAttackPoint.position, _rangedAttackPoint.rotation);

        // 투사체 초기화
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(RangedAttackDamage, ProjectileSpeed, gameObject, _attackLayerMask);
        }
    }

    #endregion


}