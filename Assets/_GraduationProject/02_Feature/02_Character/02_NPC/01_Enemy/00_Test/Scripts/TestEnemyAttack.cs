using System.Threading;
using UnityEngine;

public class TestEnemyAttack : MonoBehaviour, IPlayerRangedAttack
{
    [SerializeField] private Transform _rangedAttackPoint;
    [SerializeField] private LayerMask _enemyLayerMask = 1 << 8;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float ProjectileSpeed = 100.0f;
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _rangedAttackChargeTime = 3.0f;

    public GameObject ProjectilePrefab => _projectilePrefab;

    public float RangedAttackChargeTime => _rangedAttackChargeTime;

    public int RangedAttackDamage => _attackDamage;

    private float _timer = 0f;

    private void Update()
    {
        _timer += Time.deltaTime;
        if(_timer >= RangedAttackChargeTime)
        {
            FireProjectile();
            _timer = 0f;
        }
    }

    public void FireProjectile()
    {
        if (ProjectilePrefab == null || ProjectilePrefab == null)
        {
            return;
        }
        // 투사체 생성
        GameObject projectileObj = Instantiate(ProjectilePrefab, _rangedAttackPoint.position, _rangedAttackPoint.rotation);

        // 투사체 초기화
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(RangedAttackDamage, ProjectileSpeed, gameObject, _enemyLayerMask);
        }
    }
}
