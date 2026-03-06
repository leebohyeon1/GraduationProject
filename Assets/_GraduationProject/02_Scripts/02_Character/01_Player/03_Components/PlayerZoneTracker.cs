using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 플레이어가 현재 발을 들이고 있는 모든 ZoneArea를 추적하고,
/// 그 중 가장 우선순위가 높은 구역의 정보를 제공합니다.
/// </summary>
public class PlayerZoneTracker : MonoBehaviour
{
    private readonly HashSet<ZoneArea> _activeZones = new HashSet<ZoneArea>();

    [Header("Debug Info")]
    [SerializeField] private int _primaryZoneId = -1;

    /// <summary>
    /// 현재 위치한 구역들 중 가장 우선순위가 높은 구역의 ID를 반환합니다.
    /// </summary>
    public int CurrentZoneId
    {
        get
        {
            if (_activeZones.Count == 0) return -1;
            return _activeZones.OrderByDescending(z => z.priority).First().zoneId;
        }
    }

    /// <summary>
    /// 플레이어가 특정 구역 ID를 밟고 있는지 확인합니다.
    /// </summary>
    public bool IsInZone(int zoneId)
    {
        return _activeZones.Any(z => z.zoneId == zoneId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ZoneArea>(out var zone))
        {
            _activeZones.Add(zone);
            _primaryZoneId = CurrentZoneId;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<ZoneArea>(out var zone))
        {
            _activeZones.Remove(zone);
            _primaryZoneId = CurrentZoneId;
        }
    }
}
