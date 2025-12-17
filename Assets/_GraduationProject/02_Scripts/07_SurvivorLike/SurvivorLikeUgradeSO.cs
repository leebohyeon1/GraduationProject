using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SurvivorLikeUgradeSO", menuName = "Scriptable Objects/SurvivorLike/SurvivorLikeUgradeSO")]
public class SurvivorLikeUgradeSO : ScriptableObject
{
    public string ID;
    public string AbilityName;
    public string Description;

    [Header("PlusPlayerData")]
    public PlusPlayerData PlayerData;
}

[Serializable]
public class PlusPlayerData
{
    public int MaxHealth;
    public float MaxStamina;
    public float StaminaRegenPerSecond;
    
    public PlayerCombatData CombatData;
}