using UnityEngine;

[CreateAssetMenu(fileName = "BoostSkillSO", menuName = "Player/Skill/BoostSkillSO")]
public class BoostSkillSO : SkillSO
{
    public float Duration;

    [Header("Level 1")]
    public float IncreaseAttackRangeAmount = 1.5f;

    [Header("Level 2")]
    public float IncreaseAttackDamageAmount = 2;
}
