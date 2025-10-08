
using UnityEngine;

/// <summary>
/// 플레이어의 기본 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "BasePlayerDatasSO", menuName = "Player/BasePlayerDatasSO")]
public class BasePlayerDatasSO : ScriptableObject
{
    [Header("Stats")]
    public int MaxHealth = 100; // 최대 체력

    [Header("Movement")]
    public LayerMask GroundLayerMask = 1 << 3; // 지면으로 인식할 레이어 마스크
    public LayerMask ObstacleLayerMask = 1 << 4; // 장애물 레이어 마스크

    public float MoveSpeed = 5f; // 이동 속도
    public float RotateSpeed = 5f; // 회전 속도
    public float Gravity = -9.81f; // 중력 값
    public float GroundCheckDistance = 0.1f; // 지면 체크 거리

    [Header("Combat")]
    public PlayerCombatData CombatData; // 전투 관련 데이터


}