using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    public int MaxHealth = 100;

    [Header("Movement")]
    public float MoveSpeed = 5f;

}
