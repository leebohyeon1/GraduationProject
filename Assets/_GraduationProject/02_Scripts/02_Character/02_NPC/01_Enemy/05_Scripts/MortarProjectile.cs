using MoreMountains.Feedbacks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MortarProjectile : MonoBehaviour
{
    public DamageData damage = new DamageData{DamageAmount = 20};
    public float explosiionRadius = 5f;
    public MMF_Player explosionEffect;

    Rigidbody rb;
    Enemy _owner;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Launch(Vector3 launchVelocity, Enemy owner)
    {
        rb.linearVelocity = launchVelocity;
        _owner = owner;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject == _owner.gameObject) return; // 발사체를 쏜 주인은 무시
        Debug.Log(collision.gameObject.name);
        if (explosionEffect != null)
        {
            explosionEffect.transform.parent = null; // 이펙트가 발사체의 자식이 아니도록 설정
            explosionEffect.PlayFeedbacks();

        }
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosiionRadius);
        foreach (Collider hit in colliders)
        {

            if (hit.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
            }

        }
        Destroy(gameObject);
    }
}