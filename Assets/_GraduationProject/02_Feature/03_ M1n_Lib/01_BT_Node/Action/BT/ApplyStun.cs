using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "ApplyStun", menuName = "BehaviorTree/Action/ApplyStun")]
public class ApplyStun : Node
{
    [Tooltip("기절시킬 시간(초)입니다.")]
    public float stunDuration = 2.0f;

    protected override NodeState OnUpdate()
    {
        // runner(Enemy)의 ApplyStun 함수를 직접 호출합니다.
        runner.ParrySystem.ApplyStun();
        
        // 이 노드는 스위치를 누르는 역할만 하고 즉시 성공을 반환합니다.
        return NodeState.SUCCESS;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.stunDuration = this.stunDuration;
        return node;
    }
}