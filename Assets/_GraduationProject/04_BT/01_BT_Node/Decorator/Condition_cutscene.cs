using BehaviorTree;

public class Condition_cutscene : Node
{
    public InputReaderSO.InputMode inputMode = InputReaderSO.InputMode.Gameplay;
    protected override NodeState OnUpdate()
    {
        if (runner.player.InputReader.CurrentInputMode == inputMode)
        {
            return NodeState.SUCCESS;
        }
        return NodeState.FAILURE;
    }

    public override Node Clone()
    {
        Condition_cutscene node = Instantiate(this);
        return node;
    }
}