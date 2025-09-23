using UnityEngine;

public class Player_Stiffness : StiffnessSystem
{
    private Player _owner;
    public void Initialize(Player owner)
    {
        _owner = owner;
    }
    protected override void OnLightStagger()
    {
    }
    protected override void OnHeavyStagger()
    {
    }
}