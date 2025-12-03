using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStat", menuName = "Character/EnemyStat")]
public class EnemyStat : ScriptableObject
{
    public int Maxhealth;
    public float MoveSpeed;
    public float SeeRange;
    public float DetectRange;
    public float CircleSeeRange;
    // public int MaxHeat;
    // public int CurrentHeat;
    // public SourceMapDatabaseSO SourceMapDatabase;
    // public TierStatDatabaseSO TierStatDatabase;

}