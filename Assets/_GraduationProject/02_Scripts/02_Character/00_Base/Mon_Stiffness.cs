using UnityEngine;

public class Mon_Stiffness : StiffnessSystem
{
    private Enemy _owner;
    public override void AddStiffness(int amount, bool isCounterAttack = false)
    {
        if(_owner._stateController.CurrentState == EnemyStateController.EnemyState.Die|| _owner._stateController.CurrentState == EnemyStateController.EnemyState.Stunned)
            return;
        base.AddStiffness(amount, isCounterAttack);
    }
    public void Initialize(Enemy owner)
    {
        _owner = owner;
        _currentStiffness = 0;
    }
    protected override void OnLightStagger()
    {
        _owner.ParrySystem.ApplyWeakStun(_weakStiffnessDuration);
        _owner.ParrySystem.SetCounterAttack(true);
    }
    protected override void OnHeavyStagger()
    {
        _owner.ParrySystem.ApplyStun(_stiffnessDuration);
        _owner.ParrySystem.SetCounterAttack(true);
    }
}