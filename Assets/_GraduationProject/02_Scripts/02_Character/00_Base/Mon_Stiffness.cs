using UnityEngine;

public class Mon_Stiffness : StiffnessSystem
{
    private Enemy _owner;
    public override void AddStiffness(int amount, AttackType attackType)
    {
        if(_owner._stateController.CurrentState == EnemyStateController.EnemyState.Die|| _owner._stateController.CurrentState == EnemyStateController.EnemyState.Stunned)
            return;
        base.AddStiffness(amount, attackType);
    }
    public void Initialize(Enemy owner)
    {
        _owner = owner;
        _currentStiffness = 0;
    }
    protected override void OnLightStagger()
    {
        _owner.ParrySystem.SetCounterAttack(true);
        _owner.ParrySystem.ApplyWeakStun(_weakStiffnessDuration);
    }
    protected override void OnHeavyStagger(AttackType attackType)
    {
        _owner.ParrySystem.SetCounterAttack(true);
        bool isCounterAttack = attackType == AttackType.Normal_Counter || attackType == AttackType.Strong_Counter;
        _owner.ParrySystem.ApplyStun(_stiffnessDuration, isCounterAttack);
    }
}
