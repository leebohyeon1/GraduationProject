using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
[CreateAssetMenu(fileName = "ActionNode", menuName = "BehaviorTree/ActionNode")]
public class ActionNode : Node
{
    [Header("Inverter Settings")]
    [Tooltip("반전을 시작하기 전의 딜레이 시간(초)입니다.")]
    public float invertDelay = 0.0f;
    // public override void SetRunner(Enemy runner)
    // {
    //     base.SetRunner(runner);
        
    // }

    protected override NodeState OnUpdate()
    {

        return NodeState.SUCCESS;
    }

    public override Node Clone()
    {
        ActionNode clone = CreateInstance<ActionNode>();
        // set clone properties if needed ex) protected string name
        // clone.runner = this.runner; 
        return clone;
    }

    public override void initNode()
    {
        //set init
    }
}
