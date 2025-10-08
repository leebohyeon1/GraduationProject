using UnityEngine;

public abstract class SkillSO : ScriptableObject
{
    public string Name;
    public string Description;

    public int SkillCost;
    public float CoolDown;
    public int Count;

    public Sprite SkillIcon;

    public SkillType Type;
}
