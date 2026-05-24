using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PuzzleGridManager : MonoBehaviour
{
    public static PuzzleGridManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int _gridSize = 5;
    [SerializeField] private float _cellSize = 2.0f;
    [SerializeField] private Transform _originPoint;
    public UnityEvent ClearEvent;

    [Header("Debug")]
    [SerializeField] private bool _showGridGizmos = true;

    // 위치 기반 검색용 딕셔너리
    private Dictionary<Vector2Int, TotemBase> _gridObjects = new Dictionary<Vector2Int, TotemBase>();
    // 전체 리스트 (리셋용)
    private List<TotemBase> _allTotems = new List<TotemBase>();
    private List<ObjectTotem> _objectTotems = new List<ObjectTotem>();


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterTotem(TotemBase totem, Vector2Int pos)
    {
        if (_gridObjects.ContainsKey(pos))
        {
            Debug.LogError($"[PuzzleGridManager] Position {pos} is already occupied!");
            return;
        }

        _gridObjects[pos] = totem;
        _allTotems.Add(totem);
        
        if (totem is ObjectTotem objTotem)
        {
            _objectTotems.Add(objTotem);
        }
    }

    public void UpdateTotemPosition(TotemBase totem, Vector2Int oldPos, Vector2Int newPos)
    {
        if (_gridObjects.ContainsKey(oldPos) && _gridObjects[oldPos] == totem)
        {
            _gridObjects.Remove(oldPos);
        }
        _gridObjects[newPos] = totem;
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _gridSize && pos.y >= 0 && pos.y < _gridSize;
    }

    public bool IsOccupied(Vector2Int pos)
    {
        return _gridObjects.ContainsKey(pos);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        if (_originPoint == null) return Vector3.zero;
        float x = gridPos.x * _cellSize;
        float z = gridPos.y * _cellSize;
        Vector3 localPos = new Vector3(x, 0, z);

        return _originPoint.TransformPoint(localPos);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        if (_originPoint == null) return Vector2Int.zero;
        Vector3 localPos = _originPoint.InverseTransformPoint(worldPos)
        ;
        int x = Mathf.RoundToInt(localPos.x / _cellSize);
        int z = Mathf.RoundToInt(localPos.z / _cellSize);
        return new Vector2Int(x, z);
    }

    public Vector2Int GetSlideTargetPosition(Vector2Int startPos, Vector2Int direction)
    {
        Vector2Int current = startPos;
        Vector2Int next = current + direction;

        // 대각선 이동 시에도 동일 로직 (칸 단위 점프)
        // 만약 대각선 경로의 '사이'를 체크해야 한다면 로직 변경 필요하지만,
        // 현재는 그리드 칸 단위로만 체크함.
        while (IsValidPosition(next) && !IsOccupied(next))
        {
            current = next;
            next += direction;
        }
        return current;
    }
    
    public void CheckWinCondition()
    {
        if (_objectTotems.Count == 0) return;

        foreach (var obj in _objectTotems)
        {
            if(obj.Type == TotemType.Obstacle)
            {
                continue;
            }

            if (!obj.IsAtTarget)
            {
                return;
            }
        }
        
        Debug.Log("🎉 PUZZLE SOLVED! Door Opens.");
        ClearEvent.Invoke();
    }
    public void BreakAllTotems()
    {
        Debug.Log("[PuzzleGridManager] Breaking all totems...");
        foreach (var totem in _allTotems)
        {
            totem.DestroyTotem();
        }
    }
    /// <summary>
    /// 퍼즐 전체 초기화
    /// </summary>
    public void ResetPuzzle()
    {
        Debug.Log("[PuzzleGridManager] Resetting Puzzle...");
        
        // 1. 그리드 맵 초기화
        _gridObjects.Clear();

        // 2. 모든 토템 초기 위치로 이동 및 재등록
        foreach (var totem in _allTotems)
        {
            totem.ResetToStart();
            
            // 초기 위치 기반 재등록
            Vector2Int startPos = WorldToGrid(totem.transform.position);
            _gridObjects[startPos] = totem;
        }
    }

    private void OnDrawGizmos()
    {
        if (!_showGridGizmos || _originPoint == null) return;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        
        Gizmos.color = Color.cyan;
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                Vector3 pos = new Vector3 (x * _cellSize, 0, y * _cellSize);
            
                // DrawWireCube의 위치와 크기도 이제 로컬 기준입니다.
                Gizmos.DrawWireCube(pos, new Vector3(_cellSize * 0.9f, 0.1f, _cellSize * 0.9f));
        }
    }

    // 4. 매트릭스 복구 (다른 기즈모에 영향을 주지 않기 위해 필수!)
    Gizmos.matrix = oldMatrix;
    }
}
