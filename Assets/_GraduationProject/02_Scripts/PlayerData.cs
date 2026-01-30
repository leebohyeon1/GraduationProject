using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Basic Info")]
    public string playerName;
    public int level;
    public int gold;
    
    [Header("Stats")]
    public int currentHealth;
    public int maxHealth;
    public float currentStamina;
    public float maxStamina;
    
    [Header("Items")]
    public int currentPotion;
    public int maxPotion;
    
    [Header("Position")]
    public float x, y, z;

    [Header("Abilities")]
    public List<string> acquiredAbilityIds = new List<string>();

    public PlayerData()
    {
        playerName = "Hero";
        level = 1;
        gold = 0;
        
        currentHealth = 100;
        maxHealth = 100;
        currentStamina = 100f;
        maxStamina = 100f;
        
        currentPotion = 3;
        maxPotion = 3;
        
        x = 0;
        y = 0;
        z = 0;
        
        acquiredAbilityIds = new List<string>();
    }
}
