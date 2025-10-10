using UnityEngine;

public class EnemySpecizalAbility : MonoBehaviour
{
    Enemy _owner;
    public Enemy owner => _owner;
    protected bool _abilityReady = false;
    public bool AbilityReady => _abilityReady;

    public void Initialize(Enemy owner)
    {
        _owner = owner;
    }
    public void SetAbility(bool value)
    {
        Debug.Log("Set Ability: " + value);
        _abilityReady = value;
    }
}

