using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    public float MaxHealth = 100f;

    [Header("Movement")]
    public float MoveSpeed = 5f;

}
