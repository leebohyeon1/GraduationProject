using UnityEngine;

/// <summary>
/// 사용자가 직접 입력하여 실행하는 액티브형 능력의 기본 클래스
/// (공격, 회피, 스킬 등)
/// </summary>
public abstract class ActiveAbilitySO : PlayerAbilitySO
{
    [Header("Active Ability Settings")]
    [Tooltip("이 스킬이 장착될 기본 슬롯 위치 (자동 장착 시 사용)")]
    public AbilitySlotType DefaultSlot;

    [Tooltip("재사용 대기시간 (초)")]
    public float Cooldown;

    [Tooltip("스킬 사용 시 소모되는 스테미나")]
    public float StaminaCost;

    // 런타임 상태 (쿨타임 계산용) -> 주의: SO는 데이터이므로 런타임 변수는 별도 관리가 필요함.
    // 하지만 구조를 단순화하기 위해 일단 여기 두거나, 
    // 나중에 'AbilityState' 클래스로 분리하는 것을 추천합니다.
    // 지금은 PlayerAbility 컴포넌트 쪽에서 쿨타임을 관리하도록 설계하겠습니다.

    /// <summary>
    /// 스킬 실행 가능 여부 체크 (쿨타임, 스테미나, 상태이상 등)
    /// </summary>
    public virtual bool CanExecute(PlayerController player)
    {
        // 1. 스테미나 체크
        if (!player.Stamina.CheckStamina())
        { 
            return false;
        }

        // 2. 태그 체크 (예: 기절, 침묵 상태면 사용 불가)
        // 이 부분은 PlayerAbility 컴포넌트를 통해 확인해야 하므로
        // player.GetComponent<PlayerAbility>().HasTag(...) 등을 활용

        return true;
    }

    /// <summary>
    /// 실제 스킬 실행 로직
    /// </summary>
    public abstract void Execute(PlayerController player);

    /// <summary>
    /// 스킬 실행 후 처리 (자원 소모, 쿨타임 시작 등)
    /// </summary>
    public virtual void OnPostExecute(PlayerController player)
    {
        // 스테미나 소모
        if (StaminaCost > 0)
        {
            player.RuntimeData.CurrentStamina -= StaminaCost;
            // UI 업데이트 알림 등을 여기서 보낼 수도 있음
        }
    }
}
