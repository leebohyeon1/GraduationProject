using UnityEngine;

public class SkillEnchantNPC :InteractableObject
{
    private PlayerSkill _playerSkill;

    protected override void OnEnable()
    {
        base.OnEnable();

        _playerSkill = p_player.Skill;
    }

    public PlayerSkillData GetPlayerSkillData()
    {
        return _playerSkill.SkillData;
    }

}
