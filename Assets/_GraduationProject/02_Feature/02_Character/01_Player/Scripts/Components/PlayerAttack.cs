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
    
    private float _lastAttackTime;
    private bool _canAttack = true;
    
    private IAttackDirectionProvider _attackDirectionProvider;
    
    // IAttacker 인터페이스 구현
    public float AttackDamage => p_playerStats != null ? p_playerStats.AttackDamage : 10f;
    public float AttackSpeed => p_playerStats != null ? p_playerStats.AttackSpeed : 1f;

    public override void Initialize(Player player)
    {
        base.Initialize(player);

        // Attack Point가 없으면 플레이어 위치를 사용
        if (_attackPoint == null)
        {
            _attackPoint = transform;
        }
        
        // 공격 방향 제공자 설정
        _attackDirectionProvider = player.AttackDirectionProvider;
    }
    
    public void TryAttack()
    {
        if (!_canAttack) return;

        if (Time.time - _lastAttackTime >= 1f / AttackSpeed)
        {
            SetAttackDirection();   
            PerformAttack();
            _lastAttackTime = Time.time;
        }
    }
    
    /// <summary>
    /// 현재 입력 기기에 따라 공격 방향을 설정하는 함수
    /// </summary>
    private void SetAttackDirection()
    {
        if (_attackDirectionProvider != null)
        {
            Vector3 attackDirection = _attackDirectionProvider.CurrentAttackDirection;
            
            // 공격 방향으로 플레이어 즉시 회전
            if (attackDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(attackDirection, Vector3.up);
                transform.rotation = targetRotation;
                
                Log.Print($"공격 방향 설정: {attackDirection}");
            }
        }
    }
    
    private void PerformAttack()
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
        
        // 공격 애니메이션이나 이펙트를 여기서 실행할 수 있습니다
        StartCoroutine(AttackCooldown());
    }
    
    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead) return;
        
        target.TakeDamage(AttackDamage, gameObject);
        Log.Print($"플레이어가 {target}에게 {AttackDamage} 피해를 입혔습니다!");
    }
    
    private IEnumerator AttackCooldown()
    {
        _canAttack = false;
        yield return new WaitForSeconds(0.1f); // 짧은 쿨다운으로 연타 방지
        _canAttack = true;
    }
    
    public void SetAttackEnabled(bool enabled)
    {
        _canAttack = enabled;
    }
    
    
    // 공개 프로퍼티들
    public bool CanAttack => _canAttack;
    public float AttackRadius => p_playerStats.AttackRadius;
    public Transform AttackPoint => _attackPoint;
}