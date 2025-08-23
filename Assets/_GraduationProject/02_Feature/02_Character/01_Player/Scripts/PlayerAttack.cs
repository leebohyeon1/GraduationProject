using System.Collections;
using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 공격 시스템을 담당하는 클래스
/// IAttacker 인터페이스를 구현하여 공격 기능을 제공
/// </summary>
public class PlayerAttack : DIMonoBehaviour, IAttacker
{
    [Header("Attack Settings")]
    [SerializeField] private PlayerStats _playerStats;
    
    [Header("Combat")]
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private LayerMask _enemyLayerMask = 1 << 8; // Enemy 레이어
    
    private float _lastAttackTime;
    private bool _canAttack = true;
    
    // IAttacker 인터페이스 구현
    public float AttackDamage => _playerStats != null ? _playerStats.attackDamage : 10f;
    public float AttackSpeed => _playerStats != null ? _playerStats.attackSpeed : 1f;
    
    protected override void Awake()
    {
        // Attack Point가 없으면 플레이어 위치를 사용
        if (_attackPoint == null)
            _attackPoint = transform;
    }
    
    public void TryAttack()
    {
        if (!_canAttack) return;
        
        if (Time.time - _lastAttackTime >= 1f / AttackSpeed)
        {
            PerformAttack();
            _lastAttackTime = Time.time;
        }
    }
    
    private void PerformAttack()
    {
        Log.Print($"플레이어가 공격을 시도합니다! 공격력: {AttackDamage}");
        
        // 공격 범위 내의 적들을 찾기
        Collider[] hitEnemies = Physics.OverlapSphere(_attackPoint.position, _playerStats.attackRadius, _enemyLayerMask);
        
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
    
    // 디버깅을 위한 Gizmos
    private void OnDrawGizmosSelected()
    {
        if (_attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_attackPoint.position, _playerStats.attackRadius);
        }
    }
    
    // 공개 프로퍼티들
    public bool CanAttack => _canAttack;
    public float AttackRadius => _playerStats.attackRadius;
    public Transform AttackPoint => _attackPoint;
}