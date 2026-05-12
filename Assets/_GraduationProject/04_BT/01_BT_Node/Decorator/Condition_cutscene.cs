using BehaviorTree;

public class Condition_cutscene : Node
{

    protected override NodeState OnUpdate()
    {
        if (runner.player.InputReader.CurrentInputMode == InputReaderSO.InputMode.CutScene)
        {
            return NodeState.FAILURE;
        }
        return NodeState.SUCCESS;
    }

    public override Node Clone()
    {
        Condition_cutscene node = Instantiate(this);
        return node;
    }
}