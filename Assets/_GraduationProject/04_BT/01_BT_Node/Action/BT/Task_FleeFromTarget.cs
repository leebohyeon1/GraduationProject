using UnityEngine;
using BehaviorTree;
using Pathfinding;
public class Task_FleeFromTarget : Node
{
    private Vector3 fleeDestination; // 도망갈 최종 목적지
    private IAstarAI ai;             // A* 이동 컴포넌트
    public float fleeDistance = 5f;
    public float fleeSpeed = 5f; 
    public override void OnEnter()
    {
        base.OnEnter();
        if (ai == null) ai = runner.GetComponent<IAstarAI>();

        if (runner.player == null) return;

        Transform target = runner.player.transform;
        Vector3 myPos = runner.transform.position;
        Vector3 playerPos = target.position;
        Vector3 fleeDir = (myPos - playerPos).normalized;
        Vector3 potentialTarget = myPos + (fleeDir * fleeDistance);
        
        NNInfo info = AstarPath.active.GetNearest(potentialTarget, NNConstraint.Default);
        
        if (info.node != null)
        {
            fleeDestination = info.position;
        }
        else
        {
            fleeDestination = potentialTarget;
        }
        if (ai != null) ai.maxSpeed = fleeSpeed;
    }
    protected override NodeState OnUpdate()
    {
        if (runner.player == null) return NodeState.FAILURE;
        if (ai == null) return NodeState.FAILURE;
        runner.Movement.StartOrUpdateChase(fleeDestination, EnemyStateController.EnemyState.Rush, fleeSpeed);
        if (ai.reachedDestination) 
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    
}
