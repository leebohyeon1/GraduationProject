using UnityEngine;

public class Mon_Stiffness : StiffnessSystem
{
    private Enemy _owner;
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