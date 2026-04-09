using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 그리드 기반으로 페인팅된 영역을 저장하고 시각화하는 컴포넌트입니다. (브러시 방식)
/// </summary>
public class GroupAiZone : MonoBehaviour
{
    [Header("Group Settings")]
    public GroupAi targetGroupAi;
    public string zoneName = "New Monster Zone";

    [Header("Grid Settings")]
    public float cellSize = 1.0f;
    public LayerMask groundLayer = 1 << 0; // 지면 인식을 위한 레이어
    public float yOffset = 0.05f;          // 지면 위로 얼마나 띄울지 (Z-Fighting 방지)

    // 모든 존을 추적하기 위한 리스트
    private static readonly List<GroupAiZone> _allZones = new List<GroupAiZone>();

    private void OnEnable()
    {
        if (!_allZones.Contains(this)) _allZones.Add(this);
        RefreshCache();
    }

    private void OnDisable()
    {
        _allZones.Remove(this);
    }

    [HideInInspector]
    public List<Vector2Int> paintedCells = new List<Vector2Int>();
    private HashSet<Vector2Int> _cellCache;

    [Header("Visualization")]
    public Color zoneColor = new Color(1, 0, 0, 0.3f);

    public void AddCell(Vector3 worldPos)
    {
        Vector2Int cell = WorldToGrid(worldPos);
        if (_cellCache == null) RefreshCache();

        if (!_cellCache.Contains(cell))
        {
            paintedCells.Add(cell);
            _cellCache.Add(cell);
        }
    }

    /// <summary>
    /// 좌표로 직접 제거 (내부 루프용). 삭제 성공 시 true 반환.
    /// </summary>
    public bool InternalRemoveCell(Vector2Int cell)
    {
        if (_cellCache == null) RefreshCache();
        if (_cellCache.Contains(cell))
        {
            paintedCells.Remove(cell);
            _cellCache.Remove(cell);
            return true;
        }
        return false;
    }

    public void RemoveCell(Vector3 worldPos)
    {
        InternalRemoveCell(WorldToGrid(worldPos));
    }

    public bool IsInZone(Vector3 worldPos)
    {
        if (_cellCache == null) RefreshCache();
        return _cellCache.Contains(WorldToGrid(worldPos));
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.z / cellSize)
        );
    }

    public void RefreshCache()
    {
        _cellCache = new HashSet<Vector2Int>(paintedCells);
    }

    public static bool showAllZones = true;

    private void OnDrawGizmos()
    {
        bool isSelected = false;
#if UNITY_EDITOR
        isSelected = Selection.activeGameObject == gameObject;
#endif
        if (!showAllZones && !isSelected) return;

        if (paintedCells == null || paintedCells.Count == 0) return;

        Color displayColor = zoneColor;
        displayColor.a = isSelected ? zoneColor.a : zoneColor.a * 0.4f;

        Gizmos.color = displayColor;
        Vector3 size = new Vector3(cellSize, 0.05f, cellSize);

        Vector3 centerSum = Vector3.zero;
        foreach (var cell in paintedCells)
        {
            float x = cell.x * cellSize + cellSize * 0.5f;
            float z = cell.y * cellSize + cellSize * 0.5f;

            float finalY = transform.position.y;
            Ray ray = new Ray(new Vector3(x, transform.position.y + 100f, z), Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
            {
                finalY = hit.point.y + yOffset;
            }

            Vector3 center = new Vector3(x, finalY, z);
            Gizmos.DrawCube(center, size);
            centerSum += center;

            if (isSelected)
            {
                Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.8f);
                Gizmos.DrawWireCube(center, size);
                Gizmos.color = displayColor;
            }
        }

#if UNITY_EDITOR
        Vector3 averageCenter = centerSum / paintedCells.Count;
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = isSelected ? Color.white : new Color(1, 1, 1, 0.6f);
        labelStyle.fontSize = isSelected ? 14 : 11;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontStyle = FontStyle.Bold;

        Handles.Label(averageCenter + Vector3.up * 0.5f, zoneName, labelStyle);
#endif
    }
}
