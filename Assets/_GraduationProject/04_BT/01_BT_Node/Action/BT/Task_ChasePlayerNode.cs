using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "ChasePlayerNode", menuName = "BehaviorTree/ChasePlayerNode")]
public class Task_ChasePlayerNode : Node
{
    AIPath aIPath;
    public float Distance = 3;
    public float speed = 4;
    public override void OnEnter()
    {
        aIPath = runner.GetComponent<AIPath>();
        runner.SetState(EnemyStateController.EnemyState.Chase);
        runner.aIPath.enableRotation = true;
    }
    protected override NodeState OnUpdate()
    {
        if(runner._animationBridge.IsAttacking) {
            Debug.Log("<color=red>[Task] 공격 애니메이션이 재생 중입니다. 추격 실패.</color>");
            return NodeState.FAILURE;
        }
        if (runner.player == null)
        {
            return NodeState.FAILURE;
        }
        runner.Movement.StartOrUpdateChase(runner.player.transform.position, EnemyStateController.EnemyState.Chase, speed);
        float distance = Vector3.Distance(runner.transform.position, runner.player.transform.position);
        if(distance <= Distance)
        {
            return NodeState.SUCCESS;
            
        }
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        Debug.Log("<color=cyan>[Task] 추격 완료 또는 실패, 이동 멈춤.</color>");
        runner.Movement.StopMovement();
    }
    public override void Abort()
    {
        base.Abort();
        runner.Movement.StopMovement();

    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}