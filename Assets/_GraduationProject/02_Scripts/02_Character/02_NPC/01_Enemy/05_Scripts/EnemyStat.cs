using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStat", menuName = "ScriptableObjects/Character/EnemyStat")]
public class EnemyStat : ScriptableObject
{
    public int Maxhealth;
    public int CurrentHealth;
    public float MoveSpeed;
    public ActorType ActorType = ActorType.Monster;
    // public int MaxHeat;
    // public int CurrentHeat;
    // public SourceMapDatabaseSO SourceMapDatabase;
    // public TierStatDatabaseSO TierStatDatabase;

}