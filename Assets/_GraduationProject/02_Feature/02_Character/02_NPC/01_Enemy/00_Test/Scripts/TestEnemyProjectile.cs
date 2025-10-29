using BH_Lib.Log;
using NUnit.Framework;
using UnityEngine;

public class TestEnemyProjectile : Projectile, IParryable
{
    public bool IsParryable => true;

    public OnParry OnParry => throw new System.NotImplementedException();

    public bool Parry(GameObject parryInstigator)
    {
        if (IsParryable)
        {
            Log.PrintColor(Color.red, $"테스트 적 총알 패링 성공");

            Destroy(gameObject);
            return true;
        }
        else
        {
            return false;
        }

    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (p_owner == null)
        {
            DestroyProjectile();
            return;
        }
        if (other.gameObject == p_owner) return;
        
        int layer = 1 << other.gameObject.layer;
        if ((p_targetLayerMask.value & layer) != 0)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead && !damageable.IsInvincible)
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

}
