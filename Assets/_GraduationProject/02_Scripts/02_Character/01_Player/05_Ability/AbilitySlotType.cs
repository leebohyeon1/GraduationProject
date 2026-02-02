/// <summary>
/// 플레이어 어빌리티가 장착될 수 있는 슬롯 타입
/// </summary>
public enum AbilitySlotType
{
    None = 0,

    // --- 기본 행동 ---
    PrimaryAttack,   // 기본 공격 (좌클릭)
    SecondaryAttack, // 보조 공격 (우클릭)
    Evade,           // 회피/구르기 (Space/Shift)
    Jump,            // 점프 (선택 사항)

    // --- 액티브 스킬 ---
    Skill_1,         // Q
    Skill_2,         // E
    Skill_3,         // R
    Skill_4,         // F
    
    // --- 특수 ---
    Ultimate,        // 궁극기
    Passive          // 패시브 (슬롯에 장착은 안 되지만 분류용)
}
