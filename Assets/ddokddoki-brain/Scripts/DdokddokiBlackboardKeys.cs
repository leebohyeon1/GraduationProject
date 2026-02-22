using UnityEngine;

public enum DdokddokiBlackboardKeys
{
    #region State

    /// <summary>현재 상태 (Idle, Chase, Attack 등)</summary>
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

    /// <summary>플레이어가 자신을 바라보는지 (bool)</summary>
    OnPlayerLooking,

    /// <summary>마지막으로 확인된 플레이어 위치 (Vector3)</summary>
    LastPlayerPos,

    #endregion

    #region Position

    /// <summary>시작 위치 (Vector3)</summary>
    HomePosition,

    /// <summary>자신의 GameObject (GameObject)</summary>
    Self,

    #endregion

    #region Combat

    /// <summary>전투 시작 여부 (bool)</summary>
    IsPlayerDetected,

    #endregion
}

public static class DdokddokiBlackboardKeysExtensions
{
    /// <summary>Enum을 해당하는 string 키로 변환</summary>
    public static string ToKey(this DdokddokiBlackboardKeys key)
    {
        return key.ToString();
    }

    /// <summary>string 키를 다시 Enum으로 변환 (역변환)</summary>
    public static DdokddokiBlackboardKeys ToEnum(this string key)
    {
        if (System.Enum.TryParse(key, out DdokddokiBlackboardKeys result))
            return result;

        Debug.LogWarning($"[DdokddokiBlackboardKeys] Invalid key: {key}");
        return default;
    }
}
