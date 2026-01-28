using UnityEngine;

/// <summary>
/// 중량형 4티어
/// 좌클릭 + 우클릭 시 액티브 스킬
/// </summary>
[CreateAssetMenu(fileName = "LeylineOverloadSO", menuName = "Project/Player/Ability/TheDestroyer/Tier4/LeylineOverloadSO")]
public class LeylineOverloadSO : PlayerAbilitySO
{
    public PlayerAttackConfig DamageConfig;  //  기본 데미지
    [Range(0f, 10f)]
    public float ShieldMultipliers; // 보호막 배수

    private bool _normalAttackInput;
    private bool _counterAttackInput;   


    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        p_owner.InputReader.NormalAttackEvent += OnNormalAttack;
        p_owner.InputReader.NormalAttackCancelEvent += OnNormalAttackCancel;

        p_owner.InputReader.NormalCounterEvent += OnCounterAttack;
        p_owner.InputReader.ChargeCancelEvent += OnChargeCancel;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_ability = null;
        p_owner = null;

        p_owner.InputReader.NormalAttackEvent -= OnNormalAttack;
        p_owner.InputReader.NormalAttackCancelEvent -= OnNormalAttackCancel;

        p_owner.InputReader.NormalCounterEvent -= OnCounterAttack;
        p_owner.InputReader.ChargeCancelEvent -= OnChargeCancel;
    }

    private void OnNormalAttack()
    {
        _normalAttackInput = true;

        if(_normalAttackInput && _counterAttackInput)
        {
            ActiveSkill();
        }
    }

    private void OnNormalAttackCancel()
    {
        _normalAttackInput = false;
    }

    private void OnCounterAttack()
    {
        _counterAttackInput = true;

        if (_normalAttackInput && _counterAttackInput)
        {
            ActiveSkill();
        }
    }

    private void OnChargeCancel() 
    {
        _counterAttackInput = false;
    }

    private void ActiveSkill()
    {
        Debug.Log("지맥 폭발 사용");

        float shieldDamage = p_owner.Health.CurrentShieldAmount * ShieldMultipliers;
        
        // 보호막 모두 사용
        p_owner.Health.DecreaseShield(p_owner.Health.CurrentShieldAmount);

        DamageConfig.AttackDamage += Mathf.RoundToInt(shieldDamage);

        p_owner.Combat.ExecuteAttack(DamageConfig);

        // ToDo 스킬 사용 시 방향 전환
    }
}
