using UnityEngine;

/// <summary>
/// 플레이어 전용 스탯 정의
/// CharacterStats를 상속받아 플레이어만의 고유한 능력치를 추가
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/Player Stats")]
public class PlayerStats : CharacterStats
{
    [Header("Player Combat")]
    public float attackDamage = 10f;
    public float attackSpeed = 1f;
    public float attackRadius = 2f;
    
    [Header("Player Special")]
    public float dodgeSpeed = 8f;
    public float dodgeCooldown = 2f;
}