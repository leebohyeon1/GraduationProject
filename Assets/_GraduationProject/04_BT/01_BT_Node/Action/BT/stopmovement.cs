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
            }

            runner.SetState(EnemyStateController.EnemyState.Idle);
            //?댁깋??遺遺꾩쓣 ?놁븷湲??꾪빐 ?뺣㈃???꾩갑吏濡??뺥븿
        }
    }

    protected override NodeState OnUpdate()
    {
        // ???몃뱶??利됱떆 ?깃났 ?곹깭瑜?諛섑솚?⑸땲??
        return NodeState.SUCCESS;
    }

    public override void OnExit() { }
    public override void Abort() { }
}
