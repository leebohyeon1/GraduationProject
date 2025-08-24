using System.Collections;
using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 공격 시스템을 담당하는 클래스
/// IAttacker 인터페이스를 구현하여 공격 기능을 제공
/// </summary>
public class PlayerAttack : PlayerComponent, IAttacker
{
    [Header("Combat")]
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private LayerMask _enemyLayerMask = 1 << 8; // Enemy 레이어
    
    // IAttacker 인터페이스 구현
    public int AttackDamage => p_playerStats != null ? p_playerStats.AttackDamage : 10;
    public float AttackSpeed => 1f;

    public override void Initialize(Player player)
    {
        base.Initialize(player);

        // Attack Point가 없으면 플레이어 위치를 사용
        if (_attackPoint == null)
        {
            _attackPoint = transform;
        }
    }

    public void TryAttack()
    {
        SetAttackDirection();
    }
    
    public void PerformAttack()
    {
        Log.Print($"플레이어가 공격을 시도합니다! 공격력: {AttackDamage}");
        
        // 공격 범위 내의 적들을 찾기
        Collider[] hitEnemies = Physics.OverlapSphere(_attackPoint.position, p_playerStats.AttackRadius, _enemyLayerMask);
        
        foreach (Collider enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                Attack(damageable);
            }
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

    private void SetAttackDirection()
    {
        if (p_player.InputDeviceDetector == null)
        {
            return;
        }

        if (p_player.InputDeviceDetector.CurrentInputDevice == InputDeviceType.KeyboardMouse)
        {
            Vector2 mousePosition = p_player.PlayerController.MousePosition;
            RotatePlayerWithMouse(mousePosition);
        }
        else // Gamepad
        {
            Vector2 lookInput = p_player.PlayerController.LookInput;
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
    

    public float AttackRadius => p_playerStats.AttackRadius;
    public Transform AttackPoint => _attackPoint;
}