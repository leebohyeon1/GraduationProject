using System.Collections.Generic;
using UnityEngine;

public static class ProjectileParryOverlap
{
    // Include projectiles already touching the player's body, as well as the attack volume.
    public static Collider[] Find(PlayerController player, Vector3 center, Vector3 halfExtents, bool includeArcingProjectiles = false)
    {
        var hits=new HashSet<Collider>(Physics.OverlapBox(center,halfExtents,player.transform.rotation,player.Data.AttackLayerMask,QueryTriggerInteraction.Collide));
        var body=player.GetComponent<Collider>();
        if(body) foreach(var hit in Physics.OverlapBox(body.bounds.center,body.bounds.extents+Vector3.one*.2f,Quaternion.identity,~0,QueryTriggerInteraction.Collide))
            if(hit.GetComponentInParent<EnemyProjectile>() || hit.GetComponentInParent<HomingProjectile>() || hit.GetComponentInParent<ParabolicMineProjectile>()) hits.Add(hit);
        if (includeArcingProjectiles)
        {
            // F uses the Default layer, outside the authored enemy/hit-object mask.
            // Keep the attack's horizontal reach; allow its descending arc above it.
            AddAirborneMines(hits, center + Vector3.up * .5f,
                halfExtents + new Vector3(0f, .7f, 0f), player.transform.rotation);
            if (body)
                AddAirborneMines(hits, body.bounds.center + Vector3.up * .5f,
                    body.bounds.extents + new Vector3(.6f, .7f, .6f), Quaternion.identity);
        }
        var result=new Collider[hits.Count];hits.CopyTo(result);return result;
    }

    private static void AddAirborneMines(HashSet<Collider> hits, Vector3 center, Vector3 halfExtents, Quaternion rotation)
    {
        foreach (var hit in Physics.OverlapBox(center, halfExtents, rotation, ~0, QueryTriggerInteraction.Collide))
        {
            var mine = hit.GetComponentInParent<ParabolicMineProjectile>();
            if (mine && mine.IsAirborneParryTarget) hits.Add(hit);
        }
    }
}
