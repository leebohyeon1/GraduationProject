using System.Collections;
using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 공격 시스템을 담당하는 클래스
/// </summary>
public class PlayerAttack : MonoBehaviour, IPlayerAttack
{
    [Header("Combat")]
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private LayerMask _enemyLayerMask = 1 << 8; // Enemy 레이어

    private int _comboCount = 0;
    private PlayerContext _context;

    // IAttacker 인터페이스 구현
    public int AttackDamage => _context.Stats != null ? _context.Stats.AttackData[_comboCount].AttackDamage : 10;
    public float AttackSpeed => 1f;

    public void Initialize(PlayerContext context)
    {
        _context = context;

        // Attack Point가 없으면 플레이어 위치를 사용
        if (_attackPoint == null)
        {
            _attackPoint = transform;
        }
    }

    public void TryAttack(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition)
    {
        SetAttackDirection(deviceType, lookInput, mousePosition);
    }

    public void PerformAttack()
    {
        Log.Print($"플레이어가 공격을 시도합니다! 공격력: {AttackDamage}");

        // 공격 범위 내의 적들을 찾기
        Collider[] hitEnemies = Physics.OverlapSphere(_attackPoint.position, _context.Stats.AttackData[_comboCount].AttackRadius, _enemyLayerMask);

        foreach (Collider enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                Attack(damageable);
            }
        }

        _comboCount++;
        if (_comboCount >= _context.Stats.AttackData.Length)
        {
            _comboCount = 0;
        }
    }

    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead)
        {
            return;
        }

        target.TakeDamage(AttackDamage, this);
        Log.Print($"플레이어가 {target}에게 {AttackDamage} 피해를 입혔습니다!");
    }

    private void SetAttackDirection(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition)
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
        if (lookInput.sqrMagnitude < 0.1f) return;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 lookDirection = (cameraRight * lookInput.x + cameraForward * lookInput.y).normalized;

        if (lookDirection.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = targetRotation;
        }
    }

    private void RotatePlayerWithMouse(Vector2 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position.y);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldMousePosition = ray.GetPoint(distance);
            Vector3 direction = (worldMousePosition - transform.position).normalized;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = targetRotation;
            }
        }
    }

    public void ResetComboCount()
    {
        _comboCount = 0;
    }

    public float AttackRadius => _context.Stats.AttackData[_comboCount].AttackRadius;
    public Transform AttackPoint => _attackPoint;
    public int ComboCount => _comboCount;
    
    // 디버깅을 위한 Gizmos
    private void OnDrawGizmosSelected()
    {
        if (AttackPoint != null && _context != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_attackPoint.position, _context.Stats.AttackData[_comboCount].AttackRadius);
        }
    }
}
