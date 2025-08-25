using UnityEngine;

/// <summary>
/// 플레이어 전용 스탯 정의
/// CharacterStats를 상속받아 플레이어만의 고유한 능력치를 추가
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/Player Stats")]
public class PlayerStats : CharacterStats
{
    public float RoateSpeed = 5f;

    [Header("Player Attack Data")]
    public PlayerAttackData[] AttackData;
    
    [Header("Player Dodge")]
    public float DodgeSpeed = 8f;
    public float DodgeCooldown = 2f;
}