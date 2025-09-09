using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 공격 시스템을 담당하는 클래스
/// </summary>
public class PlayerCombat : MonoBehaviour, IPlayerMeleeAttack, IPlayerRangedAttack
{
    [Header("Combat Settings")]
    [Tooltip("원거리 공격 투사체 발사 위치")]
    [SerializeField] private Transform _rangedAttackPoint;
    [Tooltip("적 레이어 마스크 (공격 대상 감지용)")]
    [SerializeField] private LayerMask _enemyLayerMask = 1 << 8;
    [Tooltip("원거리 공격에 사용할 투사체 프리팹")]
    [SerializeField] private GameObject _projectilePrefab;

    private Vector3 _attackCenter;
    /// <summary>현재 콤보 카운트 (0부터 시작)</summary>
    private int _comboCount = 0;
    /// <summary>차지 근접 공격 수행 여부</summary>
    private bool _isPerformingChargeAttack = false;
    /// <summary>플레이어 컨텍스트 참조</summary>
    private PlayerContext _context;

    /// <summary>
    /// 근거리 공격 데이터
    /// </summary>
    public PlayerMeleeAttackData MeleeAttackData =>
        _isPerformingChargeAttack
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
    /// 원거리 공격 데미지
    /// </summary>
    public int RangedAttackDamage => _context?.Stats?.RangedAttackData.AttackDamage ?? 10;
    /// <summary>
    /// 원거리 공격 차징 시간
    /// </summary>
    public float RangedAttackChargeTime => _context?.Stats?.RangedAttackData.RangedAttackChargeTime ?? 3.0f;
    /// <summary>
    /// 투사체 속도
    /// 
    /// </summary>
    public float ProjectileSpeed => _context?.Stats?.RangedAttackData.ProjectileSpeed ?? 100.0f;

    public Vector3 AttackCenter => _attackCenter;

    /// <summary>
    /// 플레이어 전투 시스템 초기화
    /// </summary>
    /// <param name="context">플레이어 컨텍스트</param>
    public void Initialize(PlayerContext context)
    {
        _context = context;
        // 이벤트 버스 구독
        _context.EventBus.OnParry += TryParry;
        _context.EventBus.OnRangedAttackStart += FireProjectile;
        _context.EventBus.OnRotateToAttackDirection += RotateToAttackDirection;
        _context.EventBus.OnAttackStart += PerformAttack;
        _context.EventBus.OnAttack += ProcessHitEnemies;
        _context.EventBus.OnChargeMeleeAttack += PerformChargeMeleeAttack;
        _context.EventBus.OnMeleeAttackChargeStart += () => { SetIsPerformingChargeAttack(true); };
        _context.EventBus.OnAttackFinished += () => { SetIsPerformingChargeAttack(false); };
    }

    /// <summary>
    /// 공격 시도 (입력 기기에 따른 방향 설정 포함)
    /// </summary>
    /// <param name="deviceType">입력 기기 타입</param>
    /// <param name="lookInput">게임패드 조준 입력</param>
    /// <param name="mousePosition">마우스 위치</param>
    public void TryAttack(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition)
    {
       _context.EventBus.PublishRotateToAttackDirection(deviceType, lookInput, mousePosition);
    }

    /// <summary>
    /// 실제 공격 실행 (Physics.OverlapBox로 범위 내 적 감지 및 피해 적용)
    /// 애니메이션 이벤트에서 호출됩니다.
    /// </summary>
    public void PerformAttack()
    {
        if (_context?.Stats == null) return;

        Log.Print($"플레이어가 공격을 시도합니다! 공격력: {AttackDamage}");

        // 공격 중심점 계산
        Vector3 attackCenter = GetAttackCenter();
        // 공격 범위 내 적 감지
        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, MeleeAttackData.AttackRadius, transform.rotation, _enemyLayerMask);

        // 감지된 적들에게 피해 적용
        _context.EventBus.PublishAttack(hitEnemies);

        // 콤보 카운트 증가
        UpdateComboCount();
    }

    /// <summary>
    /// 실제 차지 공격 실행 (Physics.OverlapBox로 범위 내 적 감지 및 피해 적용)
    /// 애니메이션 이벤트에서 호출됩니다.
    /// </summary>
    public void PerformChargeMeleeAttack()
    {
        if (_context?.Stats == null) return;

        Log.Print($"플레이어가 차지 공격을 시도합니다! 공격력: {AttackDamage}");


        // 공격 중심점 계산
        Vector3 attackCenter = GetAttackCenter();
        // 공격 범위 내 적 감지
        Collider[] hitEnemies = Physics.OverlapBox(attackCenter,  MeleeAttackData.AttackRadius, transform.rotation, _enemyLayerMask);

        // 감지된 적들에게 피해 적용
        _context.EventBus.PublishAttack(hitEnemies);
    }

    private void SetIsPerformingChargeAttack(bool value)
    {
        _isPerformingChargeAttack = value;    
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
    /// 입력 기기에 따라 공격 방향으로 회전
    /// </summary>
    /// <param name="deviceType">입력 기기 타입</param>
    /// <param name="lookInput">게임패드 조준 입력</param>
    /// <param name="mousePosition">마우스 스크린 위치</param>
    private void RotateToAttackDirection(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition)
    {
        if (deviceType == InputDeviceType.KeyboardMouse)
        {
            RotatePlayerWithMouse(mousePosition);
        }
        else // Gamepad
        {
            RotatePlayerWithGamepad(lookInput);
        }
    }

    /// <summary>
    /// 게임패드 입력으로 플레이어 회전
    /// </summary>
    /// <param name="lookInput">게임패드 우측 스틱 입력</param>
    private void RotatePlayerWithGamepad(Vector2 lookInput)
    {
        if (lookInput.sqrMagnitude < 0.1f || Camera.main == null) return;

        Vector3 lookDirection = CalculateLookDirection(lookInput);
        if (lookDirection.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }

    /// <summary>
    /// 마우스 위치로 플레이어 회전
    /// 마우스 스크린 위치를 월드 좌표로 변환하여 회전 방향 계산
    /// </summary>
    /// <param name="mousePosition">마우스 스크린 위치</param>
    private void RotatePlayerWithMouse(Vector2 mousePosition)
    {
        if (Camera.main == null) return;

        // 스크린 좌표를 레이로 변환
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        // 플레이어와 같은 높이의 평면 생성
        Plane groundPlane = new Plane(Vector3.up, transform.position.y);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 direction = GetMouseDirection(ray.GetPoint(distance));
            if (direction.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }

    /// <summary>
    /// 콤보 카운트 리셋 (공격 체인 종료 시 호출)
    /// </summary>
    public void ResetComboCount()
    {
        _comboCount = 0;
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
            projectile.Initialize(RangedAttackDamage, ProjectileSpeed, gameObject, _enemyLayerMask);
        }
    }

    /// <summary>
    /// 패링 시도 (패링 가능한 적의 공격을 반격)
    /// 애니메이션 이벤트에서 호출됩니다.
    /// </summary>
    public void TryParry()
    {
        if (_context?.Stats == null) return;

        Vector3 parryCenter = GetParryCenter();
        Collider[] hitEnemies = Physics.OverlapBox(parryCenter, _context.Stats.ParryRadius, transform.rotation, _enemyLayerMask);

        ProcessParryableEnemies(hitEnemies);
    }


    /// <summary>
    /// 공격 범위의 중심점 계산
    /// </summary>
    /// <returns>공격 범위 박스의 중심 위치</returns>
    private Vector3 GetAttackCenter()
    {
        return AttackCenter;
    }

    public void SetAttackCenter()
    {
        _attackCenter = transform.position + transform.forward * ( MeleeAttackData.AttackRadius.z / 2);
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
    /// 공격 범위 내 감지된 적들에게 피해 적용
    /// </summary>
    /// <param name="hiyObjects">감지된 적들의 Collider 배열</param>
    private void ProcessHitEnemies(Collider[] hiyObjects)
    {
        foreach (Collider obj in hiyObjects)
        {
            IDamageable damageable = obj.GetComponent<IDamageable>();

            if (damageable != null && !damageable.IsDead)
            {
                Attack(damageable);
            }
        }
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
                _context.EventBus.PublishParrySuccess();
            }
        }
    }

    /// <summary>
    /// 콤보 카운트 증가 (최대치 도달 시 0으로 초기화)
    /// </summary>
    private void UpdateComboCount()
    {
        _comboCount++;
        if (_comboCount >= _context.Stats.AttackData.Length)
        {
            _comboCount = 0;
        }
    }

    /// <summary>
    /// 게임패드 입력을 카메라 기준 월드 방향으로 변환
    /// </summary>
    /// <param name="lookInput">게임패드 우측 스틱 입력</param>
    /// <returns>카메라 기준 정규화된 방향 벡터</returns>
    private Vector3 CalculateLookDirection(Vector2 lookInput)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        return (cameraRight * lookInput.x + cameraForward * lookInput.y).normalized;
    }

    /// <summary>
    /// 월드 마우스 위치에서 플레이어로의 방향 계산
    /// </summary>
    /// <param name="worldMousePosition">월드 좌표계의 마우스 위치</param>
    /// <returns>정규화된 방향 벡터 (Y축 제거)</returns>
    private Vector3 GetMouseDirection(Vector3 worldMousePosition)
    {
        Vector3 direction = (worldMousePosition - transform.position).normalized;
        direction.y = 0;
        return direction;
    }

    private void OnDestroy()
    {
        if (_context?.EventBus != null)
        {
            _context.EventBus.OnParry -= TryParry;
            _context.EventBus.OnRangedAttackStart -= FireProjectile;
            _context.EventBus.OnRotateToAttackDirection -= RotateToAttackDirection;
            _context.EventBus.OnAttackStart -= PerformAttack;
            _context.EventBus.OnAttack -= ProcessHitEnemies;
            _context.EventBus.OnChargeMeleeAttack -= PerformChargeMeleeAttack;
            _context.EventBus.OnMeleeAttackChargeStart -= () => { SetIsPerformingChargeAttack(true); };
            _context.EventBus.OnAttackFinished -= () => { SetIsPerformingChargeAttack(false); };

        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_context?.Stats == null) return;

       //  DrawAttackGizmo();
        DrawChargeAttackGizmo();
        DrawParryGizmo();
    }

    private void DrawAttackGizmo()
    {
        Vector3 attackCenter =  transform.position + transform.forward * ( MeleeAttackData.AttackRadius.z / 2);
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero,  MeleeAttackData.AttackRadius);
        Gizmos.matrix = Matrix4x4.identity;
    }
    private void DrawChargeAttackGizmo()
    {
        Vector3 attackCenter = GetAttackCenter();
        Gizmos.color = Color.darkRed;

        Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero,  _context.Stats.ChargeMeleeAttackData.AttackRadius);
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