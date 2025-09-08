using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 공격 시스템을 담당하는 클래스
/// </summary>
public class PlayerCombat : MonoBehaviour, IPlayerMeleeAttack, IPlayerRangedAttack
{
    [Header("Combat Settings")]
    [SerializeField] private Transform _rangedAttackPoint;
    [SerializeField] private LayerMask _enemyLayerMask = 1 << 8;
    [SerializeField] private GameObject _projectilePrefab;


    private int _comboCount = 0;
    private PlayerContext _context;

    public int AttackDamage => _context?.Stats?.AttackData?[_comboCount].AttackDamage ?? 10;
    public Vector3 AttackRadius => _context?.Stats?.AttackData?[_comboCount].AttackRadius ?? Vector3.one;
    public int ComboCount => _comboCount;

    public int RangedAttackDamage => _context?.Stats?.RangedAttackData.AttackDamage ?? 10;
    public float RangedAttackChargeTime => _context?.Stats?.RangedAttackData.RangedAttackChargeTime ?? 3.0f;
    public float ProjectileSpeed => _context?.Stats?.RangedAttackData.ProjectileSpeed ?? 100.0f;



    public void Initialize(PlayerContext context)
    {
        _context = context;
        _context.EventBus.OnParry += TryParry;
        _context.EventBus.OnRangedAttackStart += FireProjectile;
        _context.EventBus.OnRotateToAttackDirection += RotateToAttackDirection;
    }

    public void TryAttack(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition)
    {
       _context.EventBus.PublishRotateToAttackDirection(deviceType, lookInput, mousePosition);
    }

    public void PerformAttack()
    {
        if (_context?.Stats == null) return;

        Log.Print($"플레이어가 공격을 시도합니다! 공격력: {AttackDamage}");

        Vector3 attackCenter = GetAttackCenter();
        Collider[] hitEnemies = Physics.OverlapBox(attackCenter, AttackRadius, transform.rotation, _enemyLayerMask);

        ProcessHitEnemies(hitEnemies);
        UpdateComboCount();
    }

    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead) return;

        target.TakeDamage(AttackDamage, this);
        Log.Print($"플레이어가 {target}에게 {AttackDamage} 피해를 입혔습니다!");
    }

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

    private void RotatePlayerWithGamepad(Vector2 lookInput)
    {
        if (lookInput.sqrMagnitude < 0.1f || Camera.main == null) return;

        Vector3 lookDirection = CalculateLookDirection(lookInput);
        if (lookDirection.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }

    private void RotatePlayerWithMouse(Vector2 mousePosition)
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
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

    public void ResetComboCount()
    {
        _comboCount = 0;
    }

    public void FireProjectile()
    {
        if (_projectilePrefab == null || _rangedAttackPoint == null)
        {
            Log.Print("투사체 프리팹 또는 발사 지점이 설정되지 않았습니다!");
            return;
        }

        Log.Print($"투사체 발사! 데미지: {RangedAttackDamage}, 속도: {ProjectileSpeed}");

        GameObject projectileObj = Instantiate(_projectilePrefab, _rangedAttackPoint.position, _rangedAttackPoint.rotation);
        
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(RangedAttackDamage, ProjectileSpeed, gameObject, _enemyLayerMask);
        }
    }

    public void TryParry()
    {
        if (_context?.Stats == null) return;

        Vector3 parryCenter = GetParryCenter();
        Collider[] hitEnemies = Physics.OverlapBox(parryCenter, _context.Stats.ParryRadius, transform.rotation, _enemyLayerMask);

        ProcessParryableEnemies(hitEnemies);
    }


    private Vector3 GetAttackCenter()
    {
        return transform.position + transform.forward * (AttackRadius.z / 2);
    }

    private Vector3 GetParryCenter()
    {
        return transform.position + transform.forward * (_context.Stats.ParryRadius.z / 2);
    }

    private void ProcessHitEnemies(Collider[] hitEnemies)
    {
        foreach (Collider enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                Attack(damageable);
            }
        }
    }

    private void ProcessParryableEnemies(Collider[] hitEnemies)
    {
        foreach (Collider enemy in hitEnemies)
        {
            IParryable parryable = enemy.GetComponent<IParryable>();
            if (parryable != null && parryable.IsParryable)
            {
                parryable.Parry(gameObject);
            }
        }
    }

    private void UpdateComboCount()
    {
        _comboCount++;
        if (_comboCount >= _context.Stats.AttackData.Length)
        {
            _comboCount = 0;
        }
    }

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
            _context.EventBus.OnRotateToAttackDirection += RotateToAttackDirection;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_context?.Stats == null) return;

        DrawAttackGizmo();
        DrawParryGizmo();
    }

    private void DrawAttackGizmo()
    {
        Vector3 attackCenter = GetAttackCenter();
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, AttackRadius);
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