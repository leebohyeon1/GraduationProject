using BehaviorTree;
using Pathfinding;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomPatrol", menuName = "BehaviorTree/RandomPatrol")]
public class RandomPatrol : Node
{
    AIPath _aiPath;
    bool _hasTarget = false;

    public override void OnEnter()
    {
        Debug.Log("Entering RandomPatrol");
        runner.SetState(Enemy.EnemyState.Patrol);
        _aiPath = runner.GetComponent<AIPath>();
        _hasTarget = false;
    }

    protected override NodeState OnUpdate()
    {
        if (!_hasTarget || (_aiPath != null && _aiPath.reachedDestination))
        {
            Vector3 randomDirection = runner.PatrolOriginPoint + (Random.insideUnitSphere * 15);
            Debug.Log(randomDirection);

            GraphNode graphNode = AstarPath.active.GetNearest(randomDirection).node;

            if (graphNode != null && !graphNode.Destroyed)
            {
                Vector3 nodePos = (Vector3)graphNode.position;
                Vector3 targetPos = nodePos;
                if (Physics.Raycast(nodePos + Vector3.up * 10f, Vector3.down, out RaycastHit hitInfo, 20f, LayerMask.GetMask("Ground")))
                {
                    targetPos = hitInfo.point;
                }
                runner.Movement.StartOrUpdateChase(targetPos);
                _hasTarget = true;
            }
            if (_hasTarget)
            {
                RaycastHit hit;
                if (Physics.Raycast(runner.transform.position + Vector3.up * 0.25f, runner.transform.forward, out hit, 1f, LayerMask.GetMask("Ground")))
                {
                    Debug.Log("Obstacle detected, recalculating path");
                    _hasTarget = false; // 장애물이 감지되면 새로운 목표 지점을 설정하도록 플래그를 재설정
                }
            }
        }
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        runner.Movement.StopMovement();
    }

    
    public override Node Clone()
    {
        return Instantiate(this);
    }
}