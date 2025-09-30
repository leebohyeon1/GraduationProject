using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Wait", menuName = "BehaviorTree/Wait")]
public class Wait : Node
{
    [Tooltip("대기할 시간 (초)")]
    public float waitTime = 1.0f;
    private float startTime;

    public override void OnEnter()
    {
        startTime = Time.time;
    }

    protected override NodeState OnUpdate()
    {
        if (Time.time - startTime > waitTime)
        {
            return NodeState.SUCCESS;
        }
        return NodeState.RUNNING;
    }

    public override void OnExit() { }
    
    public override Node Clone()
    {
        Wait newNode = Instantiate(this);
        newNode.waitTime = this.waitTime;
        return newNode;
    }
}