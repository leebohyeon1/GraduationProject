// RunSubTreeNode.cs
using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "RunSubTreeNode", menuName = "BehaviorTree/Action/RunSubTreeNode")]
public class RunSubTreeNode : Node
{
    [Tooltip("실행할 SubTree 에셋을 여기에 할당하세요.")]
    public SubTree subTreeToRun;

    private Node _runningSubTreeInstance;

    public override void OnEnter()
    {
        if (subTreeToRun == null) return;
        
        // ★ Deep Duplication (Deep Copy)
        _runningSubTreeInstance = subTreeToRun.rootNode.Clone();
        _runningSubTreeInstance.SetRunner(runner, brain);
        _runningSubTreeInstance.initNode();
    }

    protected override NodeState OnUpdate()
    {
        if (_runningSubTreeInstance == null) return NodeState.FAILURE;
        
        return _runningSubTreeInstance.Evaluate();
    }

    public override void OnExit()
    {
        if (_runningSubTreeInstance != null)
        {
            _runningSubTreeInstance.Abort();
        }
    }
    
    public override Node Clone()
    {
        var node = Instantiate(this);
        node.subTreeToRun = this.subTreeToRun; // 원본 SubTree 참조는 그대로 복사
        return node;
    }
}