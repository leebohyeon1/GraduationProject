using System.Collections.Generic;
using UnityEngine;

public static class ProjectileParryOverlap
{
    // Include projectiles already touching the player's body, as well as the attack volume.
    public static Collider[] Find(PlayerController player, Vector3 center, Vector3 halfExtents)
    {
        var hits=new HashSet<Collider>(Physics.OverlapBox(center,halfExtents,player.transform.rotation,player.Data.AttackLayerMask,QueryTriggerInteraction.Collide));
        var body=player.GetComponent<Collider>();
        if(body) foreach(var hit in Physics.OverlapBox(body.bounds.center,body.bounds.extents+Vector3.one*.2f,Quaternion.identity,~0,QueryTriggerInteraction.Collide))
            if(hit.GetComponentInParent<EnemyProjectile>() || hit.GetComponentInParent<HomingProjectile>() || hit.GetComponentInParent<ParabolicMineProjectile>()) hits.Add(hit);
        var result=new Collider[hits.Count];hits.CopyTo(result);return result;
    }
}
