using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Basic Info")]
    public int Money;
    
    [Header("Stats")]
    public int CurrentHealth;
    public int MaxHealth;
    public float CurrentStamina;
    public float MaxStamina;
    
    [Header("Items")]
    public int CurrentPotion;
    public int MaxPotion;
    
    [Header("Position")]
    public float x, y, z;

    [Header("Abilities")]
    public List<string> AcquiredAbilityIds = new List<string>();

    public PlayerData()
    {
        Money = 0;
        
        CurrentHealth = 100;
        MaxHealth = 100;
        CurrentStamina = 100f;
        MaxStamina = 100f;
        
        CurrentPotion = 3;
        MaxPotion = 3;
        
        x = 0;
        y = 0;
        z = 0;
        
        AcquiredAbilityIds = new List<string>();
    }
}
