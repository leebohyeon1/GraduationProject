using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// After a failed dragon-gate check, guides the player to unfinished side dungeons.
/// Uses the existing map icons and entrance clear tags; never writes progression.
/// </summary>
public sealed class DungeonDirectionGuide : MonoBehaviour
{
    private sealed class Marker
    {
        public SceneTransitionInteractor entrance;
        public GamePlayTagSO clearTag;
        public RectTransform root, arrow;
        public Vector2 screenPosition, direction;
        public bool edge, visible;
    }

    private static DungeonDirectionGuide _instance;
    private readonly List<Marker> _markers = new List<Marker>(3);
    private readonly List<Marker> _edgeMarkers = new List<Marker>(3);
    private Canvas _canvas;
    private CanvasGroup _group;
    private RectTransform _rect;
    private Camera _camera;
    private PlayerController _player;
    private bool _requested, _needsRebuild;
    private float _retryAt;

    public static void ShowRemainingDungeons()
    {
        if (!_instance)
        {
            var go = new GameObject("DungeonDirectionGuide", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(CanvasGroup));
            _instance = go.AddComponent<DungeonDirectionGuide>();
        }
        _instance._requested = true;
        _instance._needsRebuild = true;
        _instance._retryAt = 0;
    }

    private void Awake()
    {
        if (_instance && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 40;
        _canvas.enabled = false;
        var scaler = GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .5f;
        _group = GetComponent<CanvasGroup>();
        _group.interactable = false;
        _group.blocksRaycasts = false;
        _group.alpha = 0;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InvalidateSceneReferences();
    private void OnSceneUnloaded(Scene scene) => InvalidateSceneReferences();

    private void InvalidateSceneReferences()
    {
        ClearMarkers();
        _camera = null;
        _player = null;
        _needsRebuild = true;
        _retryAt = 0;
        Hide();
    }

    private void LateUpdate()
    {
        if (!_requested) return;
        var loading = SceneLoadingManager.Instance;
        var chunk = loading ? loading.CurrentActiveChunk : null;
        if (chunk && chunk.name == "Title")
        {
            _requested = false;
            ClearMarkers();
            Hide();
            return;
        }
        // Includes all Part2 chunks, loading screens, menus and cutscenes.
        if (!loading || loading.IsTeleporting || !chunk || chunk.name != "Part1" ||
            InputReaderSO.ActiveInputMode != InputReaderSO.InputMode.Gameplay)
        {
            Hide();
            return;
        }
        if (Time.unscaledTime >= _retryAt && (_needsRebuild || !_camera || !_camera.isActiveAndEnabled || !_player))
        {
            _retryAt = Time.unscaledTime + 1f;
            _camera = Camera.main;
            _player = FindFirstObjectByType<PlayerController>();
            if (_needsRebuild) RebuildMarkers();
        }
        if (!_camera || !_camera.isActiveAndEnabled || !_player || !_player.Health || _player.Health.IsDead || !GamePlayTagManager.Instance)
        {
            Hide();
            return;
        }

        float scale = Mathf.Max(.1f, _canvas.scaleFactor);
        Rect bounds = GetIndicatorBounds(Screen.safeArea, _camera.pixelRect, scale);
        bool any = false;
        foreach (var marker in _markers)
        {
            marker.visible = marker.entrance &&
                marker.clearTag && !GamePlayTagManager.Instance.HasTag(marker.clearTag.ID);
            if (marker.root.gameObject.activeSelf != marker.visible) marker.root.gameObject.SetActive(marker.visible);
            if (!marker.visible) continue;
            Vector3 projected = _camera.WorldToScreenPoint(marker.entrance.transform.position + Vector3.up * 2.5f);
            marker.screenPosition = ProjectToBounds(projected, bounds, out marker.edge, out marker.direction);
            any = true;
        }
        // Three entrances can share an edge; spread just those indicators along it.
        SeparateEdgeMarkers(bounds, 76f * scale);
        foreach (var marker in _markers)
        {
            if (!marker.visible) continue;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, marker.screenPosition, null, out var local);
            marker.root.anchoredPosition = local;
            marker.arrow.gameObject.SetActive(marker.edge);
            if (marker.edge)
            {
                marker.arrow.anchoredPosition = marker.direction * 40f;
                marker.arrow.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(marker.direction.y, marker.direction.x) * Mathf.Rad2Deg);
            }
        }
        _canvas.enabled = any;
        _group.alpha = any ? Mathf.MoveTowards(_group.alpha, 1f, Time.unscaledDeltaTime * 4f) : 0f;
    }

    private void RebuildMarkers()
    {
        ClearMarkers();
        var icons = new Dictionary<string, UnityEngine.UI.Image>();
        foreach (var map in FindObjectsByType<MapUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (map.DungeonMarkers == null) continue;
            foreach (var marker in map.DungeonMarkers)
            {
                if (marker == null || !marker.icon || !marker.clearTag) continue;
                var icon = marker.icon.GetComponent<UnityEngine.UI.Image>();
                if (!icon) icon = marker.icon.GetComponentInChildren<UnityEngine.UI.Image>(true);
                if (icon && icon.sprite) icons[marker.clearTag.ID] = icon;
            }
        }
        var seen = new HashSet<string>();
        foreach (var entrance in FindObjectsByType<SceneTransitionInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var target = entrance.TargetSceneData;
            if (!target || (target.name != "Part2-A" && target.name != "Part2-B" && target.name != "Part2-C") ||
                entrance.completesDungeonOnExit || !entrance.completedDungeonTag ||
                !icons.TryGetValue(entrance.completedDungeonTag.ID, out var source) || !seen.Add(entrance.completedDungeonTag.ID)) continue;
            _markers.Add(CreateMarker(entrance, source));
        }
        // Retry only while the scene's map/entrance objects have not appeared yet.
        _needsRebuild = _markers.Count == 0;
    }

    private Marker CreateMarker(SceneTransitionInteractor entrance, UnityEngine.UI.Image source)
    {
        var root = CreateRect("DungeonGuide_" + entrance.TargetSceneData.name, _rect, new Vector2(54, 54));
        var icon = root.gameObject.AddComponent<UnityEngine.UI.Image>();
        icon.sprite = source.sprite;
        icon.color = source.color;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var shadow = root.gameObject.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = new Color(0, 0, 0, .75f);
        shadow.effectDistance = new Vector2(2, -2);
        var arrow = CreateRect("OffscreenArrow", root, new Vector2(20, 20));
        for (int i = 0; i < 2; i++)
        {
            var line = CreateRect("Chevron", arrow, new Vector2(13, 3));
            line.anchoredPosition = new Vector2(0, i == 0 ? 4f : -4f);
            line.localRotation = Quaternion.Euler(0, 0, i == 0 ? -45f : 45f);
            var image = line.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.white;
            image.raycastTarget = false;
            var outline = line.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            outline.effectColor = new Color(0, 0, 0, .75f);
            outline.effectDistance = new Vector2(1, -1);
        }
        return new Marker { entrance = entrance, clearTag = entrance.completedDungeonTag, root = root, arrow = arrow };
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        return rect;
    }

    public static Rect GetIndicatorBounds(Rect safeArea, Rect cameraPixels, float uiScale)
    {
        Rect bounds = Rect.MinMaxRect(Mathf.Max(safeArea.xMin, cameraPixels.xMin), Mathf.Max(safeArea.yMin, cameraPixels.yMin),
            Mathf.Min(safeArea.xMax, cameraPixels.xMax), Mathf.Min(safeArea.yMax, cameraPixels.yMax));
        float side = Mathf.Min(84f * uiScale, bounds.width * .25f);
        float bottom = Mathf.Min(150f * uiScale, bounds.height * .25f);
        float top = Mathf.Min(84f * uiScale, bounds.height * .25f);
        return Rect.MinMaxRect(bounds.xMin + side, bounds.yMin + bottom, bounds.xMax - side, bounds.yMax - top);
    }

    public static Vector2 ProjectToBounds(Vector3 screenPoint, Rect bounds, out bool offscreen, out Vector2 direction)
    {
        Vector2 point = screenPoint;
        direction = point - bounds.center;
        offscreen = screenPoint.z <= 0 || !bounds.Contains(point);
        if (!offscreen) { direction = Vector2.zero; return point; }
        if (screenPoint.z <= 0) direction = -direction;
        if (direction.sqrMagnitude < .001f) direction = Vector2.down;
        direction.Normalize();
        float x = Mathf.Abs(direction.x) > .0001f ? bounds.width * .5f / Mathf.Abs(direction.x) : float.PositiveInfinity;
        float y = Mathf.Abs(direction.y) > .0001f ? bounds.height * .5f / Mathf.Abs(direction.y) : float.PositiveInfinity;
        return bounds.center + direction * Mathf.Min(x, y);
    }

    private void SeparateEdgeMarkers(Rect bounds, float spacing)
    {
        for (int edge = 0; edge < 4; edge++)
        {
            _edgeMarkers.Clear();
            bool vertical = edge < 2;
            foreach (var marker in _markers)
            {
                if (!marker.visible || !marker.edge || EdgeOf(marker.screenPosition, bounds) != edge) continue;
                float coordinate = vertical ? marker.screenPosition.y : marker.screenPosition.x;
                int index = 0;
                while (index < _edgeMarkers.Count && (vertical ? _edgeMarkers[index].screenPosition.y : _edgeMarkers[index].screenPosition.x) <= coordinate) index++;
                _edgeMarkers.Insert(index, marker);
            }
            float min = vertical ? bounds.yMin : bounds.xMin, max = vertical ? bounds.yMax : bounds.xMax;
            float gap = Mathf.Min(spacing, (max - min) / Mathf.Max(1, _edgeMarkers.Count - 1));
            float previous = min - gap;
            foreach (var marker in _edgeMarkers)
            {
                float coordinate = Mathf.Max(previous + gap, vertical ? marker.screenPosition.y : marker.screenPosition.x);
                if (vertical) marker.screenPosition.y = coordinate; else marker.screenPosition.x = coordinate;
                previous = coordinate;
            }
            float shift = Mathf.Max(0, previous - max);
            foreach (var marker in _edgeMarkers)
                if (vertical) marker.screenPosition.y -= shift; else marker.screenPosition.x -= shift;
        }
    }

    private static int EdgeOf(Vector2 position, Rect bounds)
    {
        if (Mathf.Abs(position.x - bounds.xMin) < 1f) return 0;
        if (Mathf.Abs(position.x - bounds.xMax) < 1f) return 1;
        return Mathf.Abs(position.y - bounds.yMin) < 1f ? 2 : 3;
    }

    private void Hide()
    {
        if (_canvas) _canvas.enabled = false;
        if (_group) _group.alpha = 0;
    }

    private void ClearMarkers()
    {
        foreach (var marker in _markers) if (marker.root)
        {
            marker.root.gameObject.SetActive(false);
            Destroy(marker.root.gameObject);
        }
        _markers.Clear();
        _edgeMarkers.Clear();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        if (_instance == this) _instance = null;
    }
}
