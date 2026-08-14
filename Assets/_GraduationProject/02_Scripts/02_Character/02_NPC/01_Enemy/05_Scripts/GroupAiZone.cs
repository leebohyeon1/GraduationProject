using System.Collections;
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
    private const int DefaultInitializationStepsPerFrame = 1;
    private const float DefaultInitializationBudgetMilliseconds = 1f;
    private const int DefaultCacheCellsPerStep = 128;

    private static int _budgetFrame = -1;
    private static int _stepsUsedThisFrame;
    private static float _frameBudgetStartTime;

    [Header("Group Settings")]
    public GroupAi targetGroupAi;
    public string zoneName = "New Monster Zone";

    [Header("Grid Settings")]
    public float cellSize = 1.0f;
    public LayerMask groundLayer = 1 << 0; // 지면 인식을 위한 레이어
    public float yOffset = 0.05f;          // 지면 위로 얼마나 띄울지 (Z-Fighting 방지)

    [Header("Activation Budget")]
    [SerializeField, Min(1)] private int _maxInitializationStepsPerFrame = DefaultInitializationStepsPerFrame;
    [SerializeField, Min(0.1f)] private float _maxInitializationMillisecondsPerFrame = DefaultInitializationBudgetMilliseconds;
    [SerializeField, Min(1)] private int _cacheCellsPerStep = DefaultCacheCellsPerStep;

    // 모든 존을 추적하기 위한 리스트
    private static readonly List<GroupAiZone> _allZones = new List<GroupAiZone>();

    private readonly Queue<EnemyInitializer> _initializationQueue = new Queue<EnemyInitializer>();
    private readonly HashSet<EnemyInitializer> _queuedInitializers = new HashSet<EnemyInitializer>();
    private Coroutine _activationRoutine;
    private int _cacheWarmupIndex;
    private bool _cacheWarmupPending;

    private void OnEnable()
    {
        if (!_allZones.Contains(this)) _allZones.Add(this);

        if (!Application.isPlaying)
        {
            RefreshCache();
            return;
        }

        BeginCacheWarmup();
        EnsureActivationRoutine();
    }

    private void OnDisable()
    {
        _allZones.Remove(this);
        if (_activationRoutine != null)
        {
            StopCoroutine(_activationRoutine);
            _activationRoutine = null;
        }
    }

    [HideInInspector]
    public List<Vector2Int> paintedCells = new List<Vector2Int>();
    private HashSet<Vector2Int> _cellCache;

    [Header("Visualization")]
    public Color zoneColor = new Color(1, 0, 0, 0.3f);

    public void AddCell(Vector3 worldPos)
    {
        Vector2Int cell = WorldToGrid(worldPos);
        EnsureCacheReady();

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
        EnsureCacheReady();
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
        EnsureCacheReady();
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
        _cacheWarmupIndex = paintedCells != null ? paintedCells.Count : 0;
        _cacheWarmupPending = false;
    }

    internal void EnqueueInitialization(EnemyInitializer initializer)
    {
        if (initializer == null || !_queuedInitializers.Add(initializer)) return;

        _initializationQueue.Enqueue(initializer);
        EnsureActivationRoutine();
    }

    private void BeginCacheWarmup()
    {
        _cellCache = new HashSet<Vector2Int>();
        _cacheWarmupIndex = 0;
        _cacheWarmupPending = paintedCells != null && paintedCells.Count > 0;
    }

    private void EnsureCacheReady()
    {
        if (_cellCache == null || _cacheWarmupPending) RefreshCache();
    }

    private void EnsureActivationRoutine()
    {
        if (!isActiveAndEnabled || _activationRoutine != null) return;
        if (!_cacheWarmupPending && _initializationQueue.Count == 0) return;

        _activationRoutine = StartCoroutine(ProcessActivationWork());
    }

    private IEnumerator ProcessActivationWork()
    {
        while (_cacheWarmupPending || _initializationQueue.Count > 0)
        {
            while (CanConsumeGlobalBudget())
            {
                if (!_cacheWarmupPending && _initializationQueue.Count == 0)
                {
                    break;
                }

                // AI가 초기화 직후 IsInZone을 호출해 미완성 캐시를 동기로
                // RefreshCache하지 않도록 셀 캐시를 항상 먼저 완성합니다.
                if (_cacheWarmupPending)
                {
                    WarmCacheStep();
                    RecordGlobalStep();
                    continue;
                }

                EnemyInitializer initializer = _initializationQueue.Dequeue();
                if (initializer == null)
                {
                    _queuedInitializers.Remove(initializer);
                    continue;
                }

                bool complete = initializer.AdvanceDeferredInitialization();
                RecordGlobalStep();

                if (complete)
                {
                    _queuedInitializers.Remove(initializer);
                }
                else
                {
                    _initializationQueue.Enqueue(initializer);
                }
            }

            yield return null;
        }

        _activationRoutine = null;
    }

    private void WarmCacheStep()
    {
        int count = paintedCells != null ? paintedCells.Count : 0;
        int cellsThisStep = _cacheCellsPerStep > 0 ? _cacheCellsPerStep : DefaultCacheCellsPerStep;
        int end = Mathf.Min(_cacheWarmupIndex + cellsThisStep, count);

        for (int i = _cacheWarmupIndex; i < end; i++)
        {
            _cellCache.Add(paintedCells[i]);
        }

        _cacheWarmupIndex = end;
        _cacheWarmupPending = _cacheWarmupIndex < count;
    }

    private bool CanConsumeGlobalBudget()
    {
        if (_budgetFrame != Time.frameCount)
        {
            _budgetFrame = Time.frameCount;
            _stepsUsedThisFrame = 0;
            _frameBudgetStartTime = Time.realtimeSinceStartup;
        }

        int maxSteps = _maxInitializationStepsPerFrame > 0
            ? _maxInitializationStepsPerFrame
            : DefaultInitializationStepsPerFrame;
        float maxMilliseconds = _maxInitializationMillisecondsPerFrame > 0f
            ? _maxInitializationMillisecondsPerFrame
            : DefaultInitializationBudgetMilliseconds;
        float elapsedMilliseconds = (Time.realtimeSinceStartup - _frameBudgetStartTime) * 1000f;
        return _stepsUsedThisFrame < maxSteps && elapsedMilliseconds < maxMilliseconds;
    }

    private static void RecordGlobalStep()
    {
        _stepsUsedThisFrame++;
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
