using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float moveSpeed = 5f;

}
