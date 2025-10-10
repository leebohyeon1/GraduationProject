using UnityEngine;

[CreateAssetMenu(fileName = "FlashSkillSO", menuName = "Player/Skill/FlashSkillSO")]
public class FlashSkillSO : SkillSO
{
    [Header("Level 0")]
    public float MoveDistance;

    [Header("Level 1")]
    public float DecreaseCoolDownAmount = 1;

    [Header("Level 2")]
    public int IncreaseCountAmount = 1;

    [Header("Level 3")]
    public Vector3 FlashAttackRange;
    public int FlashDamage;
}
