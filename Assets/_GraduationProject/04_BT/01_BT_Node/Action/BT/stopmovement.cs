using UnityEngine;
using BehaviorTree;
using Pathfinding;
public class StopMovement : Node
{
    public bool juststop = false;
    AIPath aiPath;
    public override Node Clone()
    {
        var node = Instantiate(this);
        node.juststop = this.juststop;
        return node;
    }

    public override void OnEnter()
    {
        if (runner != null)
        {
            if (juststop)
                runner.Movement.StopMovement();
            else
            {
                runner.Movement.StartOrUpdateChase(runner.transform.position + runner.transform.forward * 0.5f);
                runner.GetComponent<AIPath>().enableRotation = true;
                // // Debug.Log("정면이동");
            }

            runner.SetState(EnemyStateController.EnemyState.Idle);
            //어색한 부분을 없애기 위해 정면을 도착지로 정함
        }
    }

    protected override NodeState OnUpdate()
    {
        // 이 노드는 즉시 성공 상태를 반환합니다.
        return NodeState.SUCCESS;
    }

    public override void OnExit() { }
    public override void Abort() { }
}