using UnityEngine;
using BehaviorTree;
using Pathfinding;
using Pathfinding.RVO;

public class Task_Pressure : Node
{
    public string Pos_Key = "PressurePos";
    public float MoveSpeed = 4.0f;
    public float StoppingDist = 0.5f;
    public float RotationSpeed = 10.0f;
    private AIPath ai;

    public override void OnEnter()
    {
        base.OnEnter();
        ai = runner.aIPath;
        if (ai != null)
        {
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = MoveSpeed;
            ai.enableRotation = false;
            ai.SetPath(null); 
        }
        brain.blackboard.SetValue("IsSurrounding", true);
        runner.SetState(EnemyStateController.EnemyState.Rush);
        runner.AnimationBool("Walk", true);
    }

    protected override NodeState OnUpdate()
    {
        if (brain == null || brain.blackboard == null || runner.player == null) return NodeState.FAILURE;
        if (ai == null) return NodeState.FAILURE;

        // [Crucial Fix] 매 프레임 이동 권한 보장 (다른 스크립트의 간섭 방어)
        if (!ai.canMove) ai.canMove = true;
        if (ai.isStopped) ai.isStopped = false;

        if (runner._animationBridge != null && runner._animationBridge.IsAttacking) return NodeState.RUNNING;
        if (!brain.blackboard.GetValue<Vector3>(Pos_Key, out Vector3 moveDest)) return NodeState.FAILURE;

        ai.destination = moveDest;
        if (Vector3.Distance(runner.transform.position, moveDest) < StoppingDist)
        {
            RotateTowards(runner.player.transform.position);
            return NodeState.SUCCESS;
        }

        RotateTowards(runner.player.transform.position);
        return NodeState.RUNNING;
    }

    private void RotateTowards(Vector3 target)
    {
        Vector3 dir = (target - runner.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            runner.transform.rotation = Quaternion.Slerp(runner.transform.rotation, lookRot, Time.deltaTime * RotationSpeed);
        }
    }

    public override void Abort() { if (isEntered) { Cleanup(); isEntered = false; } }
    public override void OnExit() { Cleanup(); }

    private void Cleanup()
    {
        if (ai != null)
        {
            ai.isStopped = true;
            ai.enableRotation = true;
        }
        runner.AnimationBool("Walk", false);
        brain.blackboard.SetValue("IsSurrounding", false);
    }

    public override Node Clone() => Instantiate(this);
}
