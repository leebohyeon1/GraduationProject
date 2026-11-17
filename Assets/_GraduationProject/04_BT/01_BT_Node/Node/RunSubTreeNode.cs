// RunSubTreeNode.cs
using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RunSubTreeNode", menuName = "BehaviorTree/Action/RunSubTreeNode")]
public class RunSubTreeNode : Node
{
    [Tooltip("실행할 SubTree 에셋을 여기에 할당하세요.")]
    public SubTree subTreeToRun;

    private Node _runningSubTreeInstance;

    public override void OnEnter()
    {
        // Preserve a fresh graph on every entry, including direct re-entry and source changes.
        ReleaseRunningSubTree(true);
        if (subTreeToRun == null || subTreeToRun.rootNode == null) return;
        
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
        ReleaseRunningSubTree(true);
    }

    private void OnDestroy()
    {
        // The runner may already have been destroyed. Only release owned objects here.
        ReleaseRunningSubTree(false);
    }

    private void ReleaseRunningSubTree(bool abort)
    {
        Node instance = _runningSubTreeInstance;
        _runningSubTreeInstance = null;
        if (instance == null) return;
        try
        {
            if (abort) instance.Abort();
        }
        finally
        {
            DestroyOwnedGraph(instance, new HashSet<Node>());
        }
    }

    private static void DestroyOwnedGraph(Node node, HashSet<Node> visited)
    {
        if (node == null || !visited.Add(node)) return;
        if (node is CompositeNode composite)
        {
            if (composite.nodes != null)
                foreach (Node child in composite.nodes) DestroyOwnedGraph(child, visited);
            if (composite.services != null)
                foreach (ServiceNode service in composite.services) DestroyOwnedGraph(service, visited);
        }
        else if (node is Decorator_Inverter inverter)
        {
            DestroyOwnedGraph(inverter.child, visited);
        }

        if (node is RunSubTreeNode nested)
        {
            Node nestedInstance = nested._runningSubTreeInstance;
            nested._runningSubTreeInstance = null;
            DestroyOwnedGraph(nestedInstance, visited);
        }

        // Never follow subTreeToRun: it is a shared source asset, not an owned clone.
        if (Application.isPlaying) Destroy(node);
        else DestroyImmediate(node);
    }
    
    public override Node Clone()
    {
        var node = Instantiate(this);
        node._runningSubTreeInstance = null;
        node.subTreeToRun = this.subTreeToRun; // 원본 SubTree 참조는 그대로 복사
        return node;
    }
}
