using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 공격 시스템을 담당하는 컴포넌트
/// 근거리 공격, 원거리 공격, 패링 기능을 제공합니다.
/// </summary>
public class PlayerCombat : MonoBehaviour, IPlayerMeleeAttack, IPlayerRangedAttack
{
    #region Serialized Fields

    [Header("Combat Settings")]
    [Tooltip("원거리 공격 투사체 발사 위치")]
    [SerializeField] private Transform _rangedAttackPoint;
    
    [Tooltip("적 레이어 마스크 (공격 대상 감지용)")]
    [SerializeField] private LayerMask _enemyLayerMask = 1 << 8;
    
    [Tooltip("원거리 공격에 사용할 투사체 프리팹")]
    [SerializeField] private GameObject _projectilePrefab;

    #endregion

    #region Private Fields

    /// <summary>공격 범위의 중심점 위치</summary>
    private Vector3 _attackCenter;
    
    /// <summary>현재 콤보 카운트 (0부터 시작)</summary>
    private int _comboCount = 0;
    
    /// <summary>차지 근접 공격 수행 여부 플래그</summary>
    private bool _isPerformingChargeAttack = false;
    
    /// <summary>플레이어 컨텍스트 참조 (스탯, 이벤트버스 등에 액세스)</summary>
    private PlayerContext _context;

    #endregion

    #region Properties

    /// <summary>
    /// 근거리 공격 데이터 (콤보 또는 차지 상태에 따라 달라짐)
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
    /// </summary>
    public float ProjectileSpeed => _context?.Stats?.RangedAttackData.ProjectileSpeed ?? 100.0f;

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
        
        // 전투 관련 이벤트 버스 구독
        _context.EventBus.OnParry += TryParry;                           // 패링 시도 이벤트
        _context.EventBus.OnRangedAttackStart += FireProjectile;         // 원거리 공격 시작 이벤트
        _context.EventBus.OnRotateToAttackDirection += RotateToAttackDirection; // 공격 방향 회전 이벤트
        _context.EventBus.OnPerformAttack += PerformAttack;              // 일반 공격 수행 이벤트
        _context.EventBus.OnAttack += ProcessHitEnemies;                 // 공격 히트 처리 이벤트
        _context.EventBus.OnPerformChargeMeleeAttack += PerformChargeMeleeAttack; // 차지 공격 수행 이벤트
        _context.EventBus.OnMeleeAttackChargeStart += () => SetIsPerformingChargeAttack(true);  // 차지 시작
        _context.EventBus.OnAttackFinished += () => SetIsPerformingChargeAttack(false);         // 공격 종료
    }

    /// <summary>
    /// 공격 시도 (입력 기기에 따른 방향 설정 포함)
    /// 입력 기기에 따라 마우스 또는 게임패드 방향으로 플레이어를 회전시킵니다.
    /// </summary>
    /// <param name="deviceType">입력 기기 타입 (키보드/마우스 또는 게임패드)</param>
    /// <param name="lookInput">게임패드의 우측 스틱 입력 벡터</param>
    /// <param name="mousePosition">마우스의 스크린 좌표 위치</param>
    public void TryAttack(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition)
    {
        // 입력 기기에 따라 공격 방향으로 회전 이벤트 발생
        _context.EventBus.PublishRotateToAttackDirection(deviceType, lookInput, mousePosition);
    }

    /// <summary>
    /// 실제 공격 실행 (Physics.OverlapBox로 범위 내 적 감지 및 피해 적용)
    /// 애니메이션 이벤트에서 호출됩니다.
    /// </summary>
    public void PerformAttack()
    {
        ExecuteAttack(false);
        UpdateComboCount();
    }

    /// <summary>
    /// 실제 차지 공격 실행 (Physics.OverlapBox로 범위 내 적 감지 및 피해 적용)
    /// 애니메이션 이벤트에서 호출됩니다.
    /// </summary>
    public void PerformChargeMeleeAttack()
    {
        ExecuteAttack(true);
    }

    /// <summary>
    /// 공통 공격 실행 로직 (일반/차지 공격 둘 다 사용)
    /// Physics.OverlapBox를 사용하여 박스 형태의 공격 범위에서 적을 감지합니다.
    /// </summary>
    /// <param name="isChargeAttack">차지 공격 여부 (차지 공격시 다른 데이터 사용)</param>
    private void ExecuteAttack(bool isChargeAttack)
    {
        // 컨텍스트와 스탯 데이터 유효성 검사
        if (_context?.Stats == null) return;

        // 공격 타입에 따른 로그 출력
        string attackType = isChargeAttack ? "차지 공격" : "공격";
        Log.Print($"플레이어가 {attackType}을 시도합니다! 공격력: {AttackDamage}");

        // 공격 중심점과 범위 설정
        Vector3 attackCenter = GetAttackCenter();
        Vector3 halfExtents = MeleeAttackData.AttackRadius / 2f;  // OverlapBox는 halfExtents를 사용
        
        // 박스 형태로 공격 범위 내 적 감지
        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _enemyLayerMask);

        // 감지된 적들에게 피해 적용 이벤트 발생
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
    /// 입력 기기에 따라 공격 방햦으로 회전
    /// 키보드/마우스는 마우스 위치, 게임패드는 우측 스틱 방향 사용
    /// </summary>
    /// <param name="deviceType">현재 사용 중인 입력 기기 타입</param>
    /// <param name="lookInput">게임패드 우측 스틱 입력 벡터</param>
    /// <param name="mousePosition">마우스 스크린 좌표 위치</param>
    private void RotateToAttackDirection(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition)
    {
        if (deviceType == InputDeviceType.KeyboardMouse)
        {
            RotatePlayerWithMouse(mousePosition);    // 마우스 위치 기반 회전
        }
        else // Gamepad
        {
            RotatePlayerWithGamepad(lookInput);      // 게임패드 스틱 방향 기반 회전
        }
    }

    /// <summary>
    /// 게임패드 우측 스틱 입력으로 플레이어 회전
    /// 카메라 방향을 기준으로 스틱 입력을 월드 방향으로 변환합니다.
    /// </summary>
    /// <param name="lookInput">게임패드 우측 스틱의 2D 입력 벡터</param>
    private void RotatePlayerWithGamepad(Vector2 lookInput)
    {
        // 입력 강도가 최소 임계값 이하이거나 카메라가 없으면 무시
        if (lookInput.sqrMagnitude < 0.1f || Camera.main == null) return;

        // 스틱 입력을 카메라 기준 3D 방향으로 변환
        Vector3 lookDirection = CalculateLookDirection(lookInput);
        if (lookDirection.sqrMagnitude > 0.1f)
        {
            // 방향 벡터를 사용하여 즐시 회전 (수직 축은 고정)
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }

    /// <summary>
    /// 마우스 스크린 좌표로 플레이어 회전
    /// 마우스 스크린 위치를 월드 좌표로 변환하여 회전 방햦 계산
    /// Ray를 사용하여 마우스 포인터의 3D 월드 좌표를 구합니다.
    /// </summary>
    /// <param name="mousePosition">마우스의 스크린 좌표 (pixels)</param>
    private void RotatePlayerWithMouse(Vector2 mousePosition)
    {
        if (Camera.main == null) return;

        // 스크린 좌표를 3D 공간의 레이로 변환
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        
        // 플레이어와 같은 Y 높이의 수평 평면 생성 (지면 평면)
        Plane groundPlane = new Plane(Vector3.up, transform.position.y);

        // 레이가 지면 평면과 교차하는지 확인
        if (groundPlane.Raycast(ray, out float distance))
        {
            // 교차점에서 플레이어로의 방햦 계산
            Vector3 direction = GetMouseDirection(ray.GetPoint(distance));
            if (direction.sqrMagnitude > 0.1f)  // 최소 방햦 강도 체크
            {
                // 계산된 방햦으로 즐시 회전
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
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
    /// 콤보 카운트 리셋 (공격 체인 종료 시 호출)
    /// </summary>
    public void ResetComboCount()
    {
        _comboCount = 0;
    }

    public void SetAttackCenter()
    {
        _attackCenter = transform.position + transform.forward * (MeleeAttackData.AttackRadius.z / 2);
    }
    #endregion
    
    #region Private Methods

    /// <summary>
    /// 공격 범위의 중심점 계산
    /// </summary>
    /// <returns>공격 범위 박스의 중심 위치</returns>
    private Vector3 GetAttackCenter()
    {
        return _attackCenter;
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
    /// <param name="hitObjects">감지된 적들의 Collider 배열</param>
    private void ProcessHitEnemies(Collider[] hitObjects)
    {
        foreach (Collider obj in hitObjects)
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

    /// <summary>
    /// 게임패드 2D 스틱 입력을 카메라 기준 3D 월드 방향으로 변환
    /// 카메라의 전후좌우 방햦을 기준으로 스틱 입력을 3D 공간에 매핑합니다.
    /// </summary>
    /// <param name="lookInput">게임패드 우측 스틱의 2D 입력 (x: 좌우, y: 전후)</param>
    /// <returns>카메라 기준으로 정규화된 3D 방향 벡터</returns>
    private Vector3 CalculateLookDirection(Vector2 lookInput)
    {
        // 카메라의 전진 및 우측 방햦 벡터 추출
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        
        // Y축(vertical) 성분 제거하여 수평 면만 고려
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        // 벡터 정규화
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 스틱 입력을 카메라 기준 방햦으로 변환 및 정규화
        return (cameraRight * lookInput.x + cameraForward * lookInput.y).normalized;
    }

    /// <summary>
    /// 3D 월드 마우스 위치에서 플레이어로의 수평 방햦 계산
    /// Y축을 제거하여 수평면에서만의 방햦을 계산합니다.
    /// </summary>
    /// <param name="worldMousePosition">Ray가 지면과 교차한 3D 월드 좌표</param>
    /// <returns>플레이어에서 마우스 방햦으로의 정규화된 2D 방햦 벡터</returns>
    private Vector3 GetMouseDirection(Vector3 worldMousePosition)
    {
        // 마우스 위치에서 플레이어 위치로의 벡터 계산
        Vector3 direction = (worldMousePosition - transform.position).normalized;
        
        // Y축(수직) 성분 제거하여 수평 방햦만 사용
        direction.y = 0;
        
        return direction;
    }

    #endregion

    #region Unity Lifecycle

    private void OnDestroy()
    {
        if (_context?.EventBus != null)
        {
            _context.EventBus.OnParry -= TryParry;
            _context.EventBus.OnRangedAttackStart -= FireProjectile;
            _context.EventBus.OnRotateToAttackDirection -= RotateToAttackDirection;
            _context.EventBus.OnPerformAttack -= PerformAttack;
            _context.EventBus.OnAttack -= ProcessHitEnemies;
            _context.EventBus.OnPerformChargeMeleeAttack -= PerformChargeMeleeAttack;
            _context.EventBus.OnMeleeAttackChargeStart -= () => SetIsPerformingChargeAttack(true);
            _context.EventBus.OnAttackFinished -= () => SetIsPerformingChargeAttack(false);

        }
    }

    #endregion

#if UNITY_EDITOR

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (_context?.Stats == null) return;

        DrawAttackGizmo();
        DrawChargeAttackGizmo();
        DrawParryGizmo();
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
    private void DrawParryGizmo()
    {
        Vector3 parryCenter = GetParryCenter();
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(parryCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, _context.Stats.ParryRadius);
        Gizmos.matrix = Matrix4x4.identity;
    }

    #endregion

#endif

}