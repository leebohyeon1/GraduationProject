using UnityEngine;

[CreateAssetMenu(fileName = "OverHeatData", menuName = "GameData/OverHeatData")]
public class OverHeatDataSO : ScriptableObject
{
    public int TriggerHeat;
    public float DurationSecond;    
    public float DelaySecond;
    public float TickSecond;
    public int DamagePerTick;
    public float MaxHpRatioDamage;
    public float GroggySecond;
    public bool IsHeatLock;
}
