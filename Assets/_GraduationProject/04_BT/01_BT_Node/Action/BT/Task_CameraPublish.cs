using BehaviorTree;
using UnityEngine;

public class Task_CameraPublish : Node
{
    [Tooltip("true면 Detected, false면 Lost 이벤트를 발행합니다.")]
    public bool publish = false;

    public override void OnEnter()
    {
    }
    protected override NodeState OnUpdate()
    {
        
        return NodeState.SUCCESS;
    }
    public override void OnExit()
    {
        base.OnExit();
        runner.StateType = publish ? EnemyStateType.Detected : EnemyStateType.Lost;

    }
    public override Node Clone()
    {
        Task_CameraPublish node = Instantiate(this);
        return node;
    }
}