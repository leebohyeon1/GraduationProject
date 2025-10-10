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

    [Header("Heat")]
    [SerializeField] private TierStatDatabaseSO _tierStatDatabaseSO; // 등급별 스탯 데이터
    [SerializeField] private SourceMapDatabaseSO _sourceMapDatabaseSO; // 소스맵 데이터
    [SerializeField] private OverHeatDataSO _overHeatDataSO; // 과열 시스템 데이터
    
    [Header("Skill")]
    [SerializeField] private FlashSkillSO _flashSkillSO; // 점멸 데이터
    [SerializeField] private BoostSkillSO _boostSkillSO; // 증폭 데이터
    [SerializeField] private TimeStopSkillSO _timeStopSkillSO; // 시간 정지 데이터
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

    public FlashSkillSO FlashSkill => _flashSkillSO;
    public BoostSkillSO BoostSkill => _boostSkillSO;
    public TimeStopSkillSO TimeStopSkill => _timeStopSkillSO;
    #endregion
}
