using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 원거리 공격 투사체 클래스
/// </summary>
public class Projectile : MonoBehaviour, IAttacker
{
    [Header("Projectile Settings")]
    [SerializeField] private float _lifeTime = 5f;
    [SerializeField] private bool _destroyOnHit = true;
    
    private int _damage;
    private float _speed;
    private GameObject _owner;
    private LayerMask _targetLayerMask;
    private Rigidbody _rigidbody;
    
    public int AttackDamage => _damage;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        _rigidbody.useGravity = false;
        
        Destroy(gameObject, _lifeTime);
    }

    public void Initialize(int damage, float speed, GameObject owner, LayerMask targetLayerMask)
    {
        _damage = damage;
        _speed = speed;
        _owner = owner;
        _targetLayerMask = targetLayerMask;
        
        _rigidbody.linearVelocity = transform.forward * _speed;
        
        Log.Print($"투사체 초기화: 데미지={damage}, 속도={speed}");
    }

    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead) return;
        
        target.TakeDamage(_damage, this);
        Log.Print($"투사체가 {target}에게 {_damage} 피해를 입혔습니다!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _owner) return;
        
        int layer = 1 << other.gameObject.layer;
        if ((_targetLayerMask.value & layer) != 0)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                Attack(damageable);
                
                if (_destroyOnHit)
                {
                    DestroyProjectile();
                }
            }
        }
        else if (other.gameObject.layer != _owner.layer)
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        Log.Print("투사체 파괴");
        Destroy(gameObject);
    }
}