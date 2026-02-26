using UnityEngine;
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
    /// <summary>
    /// 플레이어 발견 Discover 전
    /// </summary>
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
    
    /// <summary>전투 시작 여부 (bool)</summary>
    IsPlayerDetected,
    
    #endregion
}
public static class EnemyBlackboardKeysExtensions
{
    /// <summary>Enum을 해당하는 string 키로 변환</summary>
    public static string ToKey(this EnemyBlackboardKeys key)
    {
        return key.ToString();  // Enum 이름을 string으로 변환
    }
    
    /// <summary>string 키를 다시 Enum으로 변환 (역변환)</summary>
    public static EnemyBlackboardKeys ToEnum(this string key)
    {
        if (System.Enum.TryParse(key, out EnemyBlackboardKeys result))
            return result;
        
        Debug.LogWarning($"[EnemyBlackboardKeys] Invalid key: {key}");
        return default;
    }
}