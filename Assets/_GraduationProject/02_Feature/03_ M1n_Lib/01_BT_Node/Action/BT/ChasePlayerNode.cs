// ChasePlayerNode.cs 파일
using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "ChasePlayerNode", menuName = "BehaviorTree/ChasePlayerNode")]
public class ChasePlayerNode : Node
{
    public override void OnEnter()
    {
        runner.SetState(Enemy.EnemyState.Chase);
    }

    protected override NodeState OnUpdate()
    {
        if (runner.player == null)
        {
            return NodeState.FAILURE;
        }
        // ★ runner에게 추격을 시작 또는 갱신하라고 명령
        runner.Movement.StartOrUpdateChase(runner.player.transform);
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        // ★ runner에게 이동을 멈추라고 명령
        runner.Movement.StopMovement();
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}