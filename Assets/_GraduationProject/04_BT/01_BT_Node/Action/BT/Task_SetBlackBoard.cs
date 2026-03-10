using UnityEngine;
using BehaviorTree;

public class Task_SetBlackBoard : Node
{
    [SerializeField]BlackBoardUtils Utils = new BlackBoardUtils();
    
    public override Node Clone()
    {
        var node = new Task_SetBlackBoard();
        node.Utils = Utils;
        return node;
    }
    public override void OnEnter()
    {
        Utils.SetValue(runner._aiController._aiBrain.blackboard, Utils);
        // Debug.Log(runner._aiController._aiBrain.blackboard.GetValue<float>(Utils.enumKey.ToString()));
    }
    
    
    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }


}