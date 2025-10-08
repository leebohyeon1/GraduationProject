using UnityEngine;

public abstract class SkillSO : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite SkillIcon;

    public SkillType Type;

    public int Price; // 가격

    public int SkillCost;   // 사용 비용
    public float CoolDown;  // 쿨타임
    public int Count;       // 사용 가능 횟수

}
