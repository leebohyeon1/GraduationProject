using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Task_TeleportAttack", menuName = "BehaviorTree/Action/Task_TeleportAttack")]
public class Task_TeleportAttack : BaseAttackNode
{
    [Header("Teleport Settings")]
    public float invisibleDuration = 1.0f;
    public float teleportOffset = 5.0f;
    public float detectionRadius = 1.0f;
    public LayerMask wallLayerMask;

    private bool _teleportDone;
    private float _teleportStartTime;

    protected override void InitialMovementSetup()
    {
        _teleportDone = false;
        _teleportStartTime = Time.time;

        runner.Movement.StopMovement();
        SetEnemyInvisible(runner, true);
    }

    protected override void UpdateMovement()
    {
        if (_teleportDone)
        {
            return;
        }

        if (Time.time - _teleportStartTime < invisibleDuration)
        {
            return;
        }

        MoveToSafePositionAroundPlayer();
        SetEnemyInvisible(runner, false);



        _teleportDone = true;
    }

    protected override bool IsMovementFinished => _teleportDone;

    protected override void SpecificCleanup()
    {
        SetEnemyInvisible(runner, false);
        base.SpecificCleanup();
    }

    private void MoveToSafePositionAroundPlayer()
    {
        if (runner.player == null)
        {
            _teleportDone = true;
            return;
        }

        Transform playerTr = runner.player.transform;
        Vector3 finalPos = CalculateSafePosition(playerTr);

        runner.transform.position = finalPos;

        Vector3 dirToPlayer = (playerTr.position - finalPos).normalized;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.LookRotation(dirToPlayer);
        }
    }

    private Vector3 CalculateSafePosition(Transform playerTr)
    {
        Vector3 playerPos = playerTr.position;
        Vector3 forward = playerTr.forward;
        Vector3 right = playerTr.right;

        // 우선순위: 뒤 -> 왼쪽 -> 오른쪽 -> 정면
        Vector3[] checkDirs = { -forward, -right, right, forward };

        foreach (Vector3 dir in checkDirs)
        {
            Vector3 targetPos = playerPos + (dir * teleportOffset);
            if (IsValidTeleportPosition(playerPos, targetPos))
            {
                return targetPos;
            }
        }

        // 뒤/양옆이 모두 막힌 경우에는 정면을 최종 폴백으로 강제
        return CalculateFrontFallbackPosition(playerPos, forward);
    }

    private bool IsValidTeleportPosition(Vector3 playerPos, Vector3 targetPos)
    {
        Vector3 checkPos = targetPos + Vector3.up;
        if (Physics.CheckSphere(checkPos, detectionRadius, wallLayerMask))
        {
            return false;
        }

        Vector3 lineStart = playerPos + Vector3.up;
        Vector3 lineEnd = targetPos + Vector3.up;
        if (Physics.Linecast(lineStart, lineEnd, wallLayerMask))
        {
            return false;
        }

        return true;
    }

    private Vector3 CalculateFrontFallbackPosition(Vector3 playerPos, Vector3 playerForward)
    {
        Vector3 frontTarget = playerPos + (playerForward * teleportOffset);
        Vector3 rayOrigin = playerPos + Vector3.up;

        if (!Physics.Raycast(rayOrigin, playerForward, out RaycastHit hit, teleportOffset, wallLayerMask))
        {
            return frontTarget;
        }

        float safeDistance = Mathf.Max(0.5f, hit.distance - detectionRadius);
        return playerPos + (playerForward * safeDistance);
    }

    private void SetEnemyInvisible(Enemy enemy, bool invisible)
    {
        if (enemy == null)
        {
            return;
        }

        var renderers = enemy.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = !invisible;
        }

        var col = enemy.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = !invisible;
        }

        if (enemy.Shield != null)
        {
            enemy.Shield.IsActive = invisible;
        }
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = attackKey;
        node.animationStateName = animationStateName;
        node.transitionBuffer = transitionBuffer;
        node.maxNodeDuration = maxNodeDuration;
        node.maintainAtk = maintainAtk;
        node.SO = SO;
        node.LoopAttack = LoopAttack;
        node.NextBT = NextBT;
        node.debugMode = debugMode;
        node.checkRangeOnEnter = checkRangeOnEnter;
        node.rangeThreshold = rangeThreshold;
        node.ignoreYDistance = ignoreYDistance;
        node.allowOutOfCombat = allowOutOfCombat;

        node.ExceptKey = ExceptKey;
        node.escapeOnHitConfirm = escapeOnHitConfirm;
        node.hitEscapeDelay = hitEscapeDelay;

        node.invisibleDuration = invisibleDuration;
        node.teleportOffset = teleportOffset;
        node.detectionRadius = detectionRadius;
        node.wallLayerMask = wallLayerMask;
        return node;
    }
}
