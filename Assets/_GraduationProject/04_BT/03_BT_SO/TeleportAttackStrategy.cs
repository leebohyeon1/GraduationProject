using UnityEngine;

[CreateAssetMenu(fileName = "TeleportAttackStrategy", menuName = "Enemy/Strategy/Teleport Attack")]
public class TeleportAttackStrategy : EnemyUseAnything
{
    [Header("Data Inspector")]
    [Tooltip("은신 유지 시간(초)")]
    public float InvisibleDuration = 1.0f;

    [Tooltip("플레이어와 떨어져서 순간이동할 거리")]
    public float TeleportOffset = 5.0f;

    [Tooltip("텔레포트 지점의 벽 충돌 검사 반경")]
    public float detectionRadius = 1.0f;

    [Header("Settings")]
    public LayerMask wallLayerMask;

    public string Animation_Ready = "Teleport_Ready";
    public string ANimation_End = "Teleport_End";
    private const string KEY_TELEPORT_START_TIME = "Teleport_StartTime";

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null) return runner;

        enemy._aiController._aiBrain.blackboard.SetValue(KEY_TELEPORT_START_TIME, Time.time);
        enemy.Movement.StopMovement();
        SetEnemyInvisible(enemy, true);

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null) return runner;

        var blackboard = enemy._aiController._aiBrain.blackboard;
        if (!blackboard.HasKey(KEY_TELEPORT_START_TIME))
            return runner;

        float startTime = blackboard.GetValue<float>(KEY_TELEPORT_START_TIME);
        float elapsedTime = Time.time - startTime;

        if (elapsedTime >= InvisibleDuration)
        {
            MoveToBackOfPlayer(enemy);
            SetEnemyInvisible(enemy, false);
            enemy.AnimationEvent(ANimation_End);
            blackboard.RemoveKey(KEY_TELEPORT_START_TIME);
        }

        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            SetEnemyInvisible(enemy, false);
            enemy._aiController._aiBrain.blackboard.RemoveKey(KEY_TELEPORT_START_TIME);
        }

        return runner;
    }

    private void MoveToBackOfPlayer(Enemy enemy)
    {
        if (enemy.player == null) return;

        Transform playerTr = enemy.player.transform;
        Vector3 finalPos = CalculateSafePosition(playerTr);

        enemy.transform.position = finalPos;

        Vector3 dirToPlayer = (playerTr.position - finalPos).normalized;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero)
        {
            enemy.transform.rotation = Quaternion.LookRotation(dirToPlayer);
        }
    }

    private Vector3 CalculateSafePosition(Transform playerTr)
    {
        Vector3 playerPos = playerTr.position;
        Vector3 forward = playerTr.forward;
        Vector3 right = playerTr.right;

        Vector3[] checkDirs = { -forward, -right, right, forward };

        foreach (Vector3 dir in checkDirs)
        {
            Vector3 targetPos = playerPos + (dir * TeleportOffset);
            Vector3 checkPos = targetPos + Vector3.up * 1.0f;

            if (!Physics.CheckSphere(checkPos, detectionRadius, wallLayerMask))
            {
                return targetPos;
            }
        }

        return playerPos;
    }

    private void SetEnemyInvisible(Enemy enemy, bool invisible)
    {
        var renderers = enemy.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = !invisible;

        var col = enemy.GetComponent<Collider>();
        if (col != null)
            col.enabled = !invisible;

        if (enemy.Shield != null)
            enemy.Shield.IsActive = invisible;
    }

    public override void Reset<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            enemy._aiController._aiBrain.blackboard.RemoveKey(KEY_TELEPORT_START_TIME);
        }
    }
}
