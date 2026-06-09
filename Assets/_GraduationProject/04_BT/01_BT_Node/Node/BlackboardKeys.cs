using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 에너미 AI의 블랙보드에서 사용하는 키 열거형입니다.
/// </summary>
public enum EnemyBlackboardKeys
{
    #region State
    /// <summary>현재 에너미 상태 (Idle, Chase, Attack 등)</summary>
    CurrentStatus,
    /// <summary>피격 여부</summary>
    OnTakeHit,
    #endregion
    
    #region Health
    /// <summary>자체 체력 (int)</summary>
    SelfHealth,
    #endregion
    
    #region Detection
    /// <summary>시야 내 플레이어 확인 여부 (bool)</summary>
    IsHasLOS,
    /// <summary>플레이어와의 거리 (float)</summary>
    DistanceBetween,
    /// <summary>감지 범위 내 여부 (bool)</summary>
    DetectPlayer,
    /// <summary>플레이어가 에너미를 바라보는지 (bool)</summary>
    OnPlayerLooking,
    /// <summary>마지막으로 확인된 플레이어 위치 (Vector3)</summary>
    LastPlayerPos,
    /// <summary>플레이어가 에어샷을 함 (bool)</summary>
    OnPlayerAirshot,
    /// <summary>플레이어가 회복 중 (bool)</summary>
    OnPlayerRecovery,
    #endregion
    
    #region Group
    /// <summary>공격 토큰 보유 여부 (bool)</summary>
    HasAttackToken,
    /// <summary>타겟이 조준 중인지 (bool)</summary>
    IsTargetAimingMe,
    /// <summary>주변 동료 수 (int)</summary>
    PeripheralColleagues,
    /// <summary>스쿼드 슬롯 인덱스 (int)</summary>
    SquadSlotIndex,
    /// <summary>포위 중인지 (bool)</summary>
    IsSurrounding,
    /// <summary>자신의 GameObject (GameObject)</summary>
    Self,
    /// <summary>플레이어 발견 Discover 전</summary>
    Engage,
    #endregion
    
    #region Position
    /// <summary>시작 위치 (Vector3)</summary>
    HomePosition,
    /// <summary>타겟 위치 (Vector3)</summary>
    TargetLocation,
    /// <summary>이동 정지 (bool)</summary>
    StopMovement,
    #endregion
    
    #region Combat
    DidLastAttackHit,
    LastAttackSuccessTime,
    LastTakeHitTime,
    /// <summary>전투 시작 여부 (bool)</summary>
    IsPlayerDetected,
    /// <summary>현재 공격 페이즈 (int)</summary>
    Phase,
    #endregion

    #region Interaction
    /// <summary>상호작용 대상 (GameObject)</summary>
    InteractNoneHIt,
    #endregion
}

/// <summary>
/// EnemyBlackboardKeys의 성능을 최적화하기 위한 확장 메서드 클래스입니다.
/// </summary>
public static class EnemyBlackboardKeysExtensions
{
    private static readonly Dictionary<EnemyBlackboardKeys, string> _keyCache;

    static EnemyBlackboardKeysExtensions()
    {
        // 모든 Enum 값을 미리 string으로 변환하여 캐싱 (GC Alloc 방지)
        var values = (EnemyBlackboardKeys[])System.Enum.GetValues(typeof(EnemyBlackboardKeys));
        _keyCache = new Dictionary<EnemyBlackboardKeys, string>(values.Length);
        foreach (var v in values)
        {
            _keyCache[v] = v.ToString();
        }
    }

    /// <summary>
    /// Enum을 캐싱된 string 키로 변환합니다. (매번 할당하지 않음)
    /// </summary>
    public static string ToKey(this EnemyBlackboardKeys key)
    {
        if (_keyCache.TryGetValue(key, out string cached))
            return cached;
        return key.ToString();
    }
    
    /// <summary>
    /// string 키를 다시 Enum으로 변환합니다.
    /// </summary>
    public static EnemyBlackboardKeys ToEnum(this string key)
    {
        if (System.Enum.TryParse(key, out EnemyBlackboardKeys result))
            return result;
        
        // // Debug.LogWarning($"[EnemyBlackboardKeys] Invalid key: {key}");
        return default;
    }
}
