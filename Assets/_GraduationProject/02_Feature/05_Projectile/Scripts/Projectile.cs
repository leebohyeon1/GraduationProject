using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 원거리 공격 투사체 클래스
/// </summary>
public class Projectile : MonoBehaviour
{ 
    [Header("Projectile Settings")]
    [SerializeField] private float _lifeTime = 5f;
    [SerializeField] protected bool p_destroyOnHit = true;
    
    private int _damage;
    private float _speed;
    protected GameObject p_owner;
    protected LayerMask p_targetLayerMask;
    protected Rigidbody p_rigidbody;
    
    public int AttackDamage => _damage;

    protected virtual void Awake()
    {
        p_rigidbody = GetComponent<Rigidbody>();
        if (p_rigidbody == null)
        {
            p_rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        p_rigidbody.useGravity = false;
        
        Destroy(gameObject, _lifeTime);
    }

    public void Initialize(int damage, float speed, GameObject owner, LayerMask targetLayerMask)
    {
        _damage = damage;
        _speed = speed;
        p_owner = owner;
        p_targetLayerMask = targetLayerMask;
        
        p_rigidbody.linearVelocity = transform.forward * _speed;
        
        Log.Print($"투사체 초기화: 데미지={damage}, 속도={speed}");
    }

    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead) return;
        
        target.TakeDamage(_damage, 50);
        Log.Print($"투사체가 {target}에게 {_damage} 피해를 입혔습니다!");
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == p_owner) return;
        
        int layer = 1 << other.gameObject.layer;
        if ((p_targetLayerMask.value & layer) != 0)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                Attack(damageable);

                if (p_destroyOnHit)
                {
                    DestroyProjectile();
                }
            }
        }
        else if (other.gameObject.layer != p_owner.layer)
        {
            DestroyProjectile();
        }
    }

    protected virtual void DestroyProjectile()
    {
        Log.Print("투사체 파괴");
        Destroy(gameObject);
    }
}