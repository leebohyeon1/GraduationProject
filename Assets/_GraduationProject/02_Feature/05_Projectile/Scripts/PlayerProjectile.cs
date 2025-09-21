using UnityEngine;
using player.Refactor;

public class PlayerProjectile : Projectile
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == p_owner) return;
        
        int layer = 1 << other.gameObject.layer;
        if ((p_targetLayerMask.value & layer) != 0)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                Attack(damageable);
                p_owner.GetComponent<player.Refactor.Player>()
                    .Events.TriggerRangedAttackAffect(other);


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
}
