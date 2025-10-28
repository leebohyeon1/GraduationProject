using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAnchor", menuName = "EnemyAnchor")]
public class EnemyAnchor : EnemyUseAnything
{
    public override T OnEnter<T>(T enemy)
    {
        enemy.EnemyHealth.SetKnockbackable(false);
        return enemy;
    }

    public override T OnUpdate<T>(T enemy)
    {
        throw new System.NotImplementedException();
    }
}