using UnityEngine;
[CreateAssetMenu(fileName = "LoopAction", menuName = "Enemy/Strategy/LoopAction")]
public class LoopAction : EnemyUseAnything
{
    public string  AnimationBool;
    public override T OnEnter<T>(T runner)
    {
        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        return runner;
    }
    public override bool UseSomeThing<T>(T runner,bool check)
    {
        
        runner.AnimationBool(AnimationBool, check);
        return true;
    }
}