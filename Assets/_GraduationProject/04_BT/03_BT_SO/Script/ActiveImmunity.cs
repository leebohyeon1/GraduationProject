using UnityEngine;
[CreateAssetMenu(fileName = "ActiveImmunity", menuName = "Enemy/Strategy/Active Immunity")]
public class ActiveImmunity : EnemyUseAnything
{
    public override T OnEnter<T>(T runner)
    {
        runner.ParrySystem.ActivateMinorImmunity();
        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        runner.ParrySystem.DeactivateImmunity();
        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        return runner;
    }

    public override void Reset<T>(T runner)
    {
        
    }
}