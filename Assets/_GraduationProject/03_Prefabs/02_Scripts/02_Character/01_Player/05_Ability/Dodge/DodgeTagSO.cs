using UnityEngine;

/// <summary>
/// 회피 태그 스크립터블 오브젝트
/// </summary>
[CreateAssetMenu(fileName = "DodgeTagSO", menuName = "Project/Player/Ability/Tag/Dodge/DodgeTagSO")]
public class DodgeTagSO : PlayerAbilityTagSO
{
    public DodgeData DodgeConfig;       // 회피 설정 

    public override void Apply(PlayerController player)
    {
        base.Apply(player);

        // 회피 데이터 설정
        player.Movement.SetDodgeConfig(DodgeConfig);
    }

}
