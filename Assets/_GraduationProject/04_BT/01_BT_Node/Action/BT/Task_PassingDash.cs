using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_PassingDash", menuName = "BehaviorTree/Action/Task_PassingDash")]
public class Task_PassingDash : BaseAttackNode
{
    [Header("Dash Settings")]
    public float dashSpeed = 15.0f;
    public float extraDist = 10.0f;
    public LayerMask obstacleMask;
    public float arrivalThreshold = 0.5f;
    public float maxTriggerRange = 10f;

    private Vector3 _targetPos;
    private bool _isDashing;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        Vector3 startPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;
        Vector3 direction = (playerPos - startPos);
        direction.y = 0;

        if (direction.sqrMagnitude < 0.1f) direction = runner.transform.forward;
        else direction.Normalize();

        _targetPos = playerPos + (direction * extraDist);
        _targetPos.y = startPos.y;

        runner.transform.rotation = Quaternion.LookRotation(direction);
        runner.aIPath.enableRotation = false;
        
        _isDashing = false;
        Log("관통 대시 준비 완료. 목표: " + _targetPos);
    }

    protected override void OnActionSOTriggered()
    {
        Log("관통 대시 시작 (OnActionSOTriggered)");
        _isDashing = true;

        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.isStopped = true;
            ai.canMove = false;
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isDashing) return;

        Vector3 currentPos = runner.transform.position;
        Vector3 moveDir = (_targetPos - currentPos);
        moveDir.y = 0;
        float distToTarget = moveDir.magnitude;

        if (distToTarget <= arrivalThreshold)
        {
            Log("관통 대시 목표 도달");
            runner.transform.position = _targetPos;
            _isDashing = false;
            brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
            return;
        }

        moveDir.Normalize();
        float moveDistance = dashSpeed * Time.deltaTime;

        if (Physics.Raycast(currentPos + Vector3.up * 1.0f, moveDir, moveDistance + 1f, obstacleMask))
        {
            Log("관통 대시 중 벽 충돌");
            _isDashing = false;
            return;
        }

        runner.transform.position += moveDir * moveDistance;
    }

    protected override bool IsMovementFinished => !_isDashing;

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.animationStateName = this.animationStateName;
        node.transitionBuffer = this.transitionBuffer;
        node.continuousRotation = this.continuousRotation;
        node.maintainAtk = this.maintainAtk;
        node.SO = this.SO;
        node.LoopAttack = this.LoopAttack;
        node.NextBT = this.NextBT;
        node.debugMode = this.debugMode;
        node.checkRangeOnEnter = this.checkRangeOnEnter;
        node.rangeThreshold = this.rangeThreshold;
        node.dashSpeed = this.dashSpeed;
        node.extraDist = this.extraDist;
        node.obstacleMask = this.obstacleMask;
        node.arrivalThreshold = this.arrivalThreshold;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        return node;
    }
}
