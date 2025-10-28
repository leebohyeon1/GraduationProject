using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAnchor", menuName = "EnemyAnchor")]
public class EnemyAnchor : EnemyUseAnything
{
    public override T OnEnter<T>(T enemy)
    {
        enemy.EnemyHealth.SetKnockbackable(false);
        enemy.Movement.StopMovement();
        return enemy;
    }

    public override T OnUpdate<T>(T enemy)
    {
        Vector3 directionToPlayer = enemy.player.transform.position - enemy.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            // 1. 목표 회전값(Quaternion)을 계산합니다.
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // 2. 현재 회전값에서 목표 회전값으로 부드럽게 회전시킵니다.
            // Quaternion.Slerp(현재 회전, 목표 회전, 회전 속도)
            enemy.transform.rotation = Quaternion.Slerp(
                enemy.transform.rotation,
                targetRotation,
                5 * Time.deltaTime
            );
        }
        return enemy;
    }
}