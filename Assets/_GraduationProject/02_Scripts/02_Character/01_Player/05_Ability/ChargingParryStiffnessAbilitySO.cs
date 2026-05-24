using UnityEngine;

/// <summary>
/// 차징 패링 성공 시 적의 경직도를 추가로 깎는(경직 게이지를 쌓는) 능력
/// </summary>
[CreateAssetMenu(fileName = "ChargingParryStiffnessAbilitySO", menuName = "Project/Player/Ability/Ability/ChargingParryStiffnessAbilitySO")]
public class ChargingParryStiffnessAbilitySO : PlayerAbilitySO
{
    [Header("Ability Settings")]
    [SerializeField] private int _stiffnessAmount = 50; // 추가로 입힐 경직도 양

    public override void RegisterAbility(PlayerAbility ability)
    {
        base.RegisterAbility(ability);
        
        // 플레이어 이벤트 시스템을 통해 카운터 성공 이벤트 구독
        if (p_owner != null && p_owner.Events != null)
        {
            p_owner.Events.CounterSucceeded += OnCounterSucceeded;
        }
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        if (p_owner != null && p_owner.Events != null)
        {
            p_owner.Events.CounterSucceeded -= OnCounterSucceeded;
        }

        base.UnregisterAbility(ability);
    }

    private void OnCounterSucceeded(Transform enemyTransform, AttackType type)
    {
        // 차징 패링(Strong_Counter)일 때만 발동
        if (type == AttackType.Strong_Counter)
        {
            p_owner.Health.AddStiffness(-_stiffnessAmount, type); // 플레이어 자신에게도 경직도 감소 효과 적용 (선택 사항)
        }
    }
}
