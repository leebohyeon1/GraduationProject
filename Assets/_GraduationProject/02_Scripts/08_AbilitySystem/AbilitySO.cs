using System;
using UnityEngine;

/// <summary>
/// 능력의 종류를 정의합니다.
/// </summary>
public enum AbilityType
{
    None,
    StatBoost, // 스탯 강화
    NewSkill,  // 새로운 스킬
    WeaponUpgrade, // 무기 업그레이드
}

[CreateAssetMenu(fileName = "AbilitySO", menuName = "Scriptable Objects/Player/AbilitySO")]
public class AbilitySO : ScriptableObject
{
    [Header("기본 정보")]
    public string AbilityName;
    [TextArea]
    public string Description;
    public Sprite Icon;

    [Header("능력 상세")]
    public AbilityType Type;
    public PlusPlayerStat PlusStat;

    // 만약 새로운 스킬이나 무기 업그레이드일 경우, 해당 프리팹을 연결할 수 있습니다.
    public GameObject SkillPrefab; 
}

[Serializable]
public class PlusPlayerStat
{
    public int Health = 10; // 최대 체력
    public float Stamina = 10;
    public float StaminaRegenPerSecond = 5;

    public PlayerCombatData CombatData; // 전투 관련 데이터
}
