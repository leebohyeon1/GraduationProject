using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackSpeed = 1f;

    // TODO: 방어력, 치명타 확률 등 다른 스탯 추가
}
