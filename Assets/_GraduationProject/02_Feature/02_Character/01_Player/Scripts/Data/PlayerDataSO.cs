using System;
using UnityEngine;

/// <summary>
/// 플레이어의 모든 데이터를 통합 관리하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerDatasBaseSO", menuName = "Player/PlayerDatasBaseSO")]
public class PlayerDataBaseSO : ScriptableObject
{
    #region Private Fields
    [SerializeField] private BasePlayerDatasSO _baseDatasSO; // 플레이어 기본 데이터
    [SerializeField] private TierStatDatabaseSO _tierStatDatabaseSO; // 등급별 스탯 데이터
    [SerializeField] private SourceMapDatabaseSO _sourceMapDatabaseSO; // 소스맵 데이터
    [SerializeField] private OverHeatDataSO _overHeatDataSO; // 과열 시스템 데이터
    #endregion

    #region Properties
    // 기본 데이터 접근자
    public BasePlayerDatasSO BaseData => _baseDatasSO;
    // 등급별 스탯 데이터 접근자
    public TierStatDatabaseSO TierStatData => _tierStatDatabaseSO;
    // 소스맵 데이터 접근자
    public SourceMapDatabaseSO SourceMapData => _sourceMapDatabaseSO;
    // 과열 시스템 데이터 접근자
    public OverHeatDataSO OverHeatData => _overHeatDataSO;
    #endregion
}

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
    public float MoveSpeed = 5f; // 이동 속도
    public float RotateSpeed = 5f; // 회전 속도
    public float Gravity = -9.81f; // 중력 값
    public float GroundCheckDistance = 0.1f; // 지면 체크 거리

    [Header("Combat")]
    public PlayerCombatData CombatData; // 전투 관련 데이터
}