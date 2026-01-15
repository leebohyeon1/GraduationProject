using UnityEngine;
[CreateAssetMenu(fileName = "LoopAction", menuName = "Enemy/Strategy/LoopAction")]
public class LoopAction : EnemyUseAnything
{
    public string  AnimationBool;
    float timer = 0f;
    public float ExitTime = 5;
    public override T OnEnter<T>(T runner)
    {
        return runner;
        
    }

    public override T OnExit<T>(T runner)
    {
        runner.AnimationBool(AnimationBool, false);
        timer = 0;
        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        timer += Time.deltaTime;
        if(timer >= ExitTime)
        {
            Debug.Log   ("[LoopAction] Exit Loop Action");
            runner.AnimationBool(AnimationBool, true);
        }
        return runner;
    }
    public override bool UseSomeThing<T>(T runner)
    {
        runner.AnimationBool(AnimationBool, true);
        return true;
    }
}