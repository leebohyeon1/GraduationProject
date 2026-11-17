using UnityEngine;

public class MapUI : MenuUIComponent
{
    [System.Serializable]
    public class DungeonMarker
    {
        public GameObject icon;
        public GamePlayTagSO clearTag;
    }
    [SerializeField] private DungeonMarker[] _dungeonMarkers = System.Array.Empty<DungeonMarker>();

    public void RefreshDungeonMarkers()
    {
        var tags = GamePlayTagManager.Instance;
        if (!tags) return;
        foreach (var marker in _dungeonMarkers)
        {
            if (marker == null || !marker.icon || !marker.clearTag) continue;
            bool visible = !tags.HasTag(marker.clearTag.ID);
            if (marker.icon.activeSelf != visible) marker.icon.SetActive(visible);
        }
    }
    [Header("UI Elements")]
    [SerializeField] private RectTransform _mapRect;
    [SerializeField] private RectTransform _playerIcon;
    [Header("World Mapping")]
    [SerializeField] private Transform _mapAreaTransform;
    [SerializeField] private Vector2 _worldMin = new Vector2(-100, -100);
    [SerializeField] private Vector2 _worldMax = new Vector2(100, 100);
    [Header("Options")]
    [SerializeField] private bool _rotateIcon = true;
    [SerializeField] private bool _rotateMap;

    [Header("Three-point calibration (matching order)")]
    [SerializeField] private Transform[] _worldCalibrationPoints = new Transform[3];
    [SerializeField] private RectTransform[] _mapCalibrationPoints = new RectTransform[3];

    public bool TryGetCalibratedMapPosition(Vector3 worldPosition, out Vector3 mapPosition)
    {
        mapPosition = default;
        if (!_mapRect || _worldCalibrationPoints == null || _mapCalibrationPoints == null ||
            _worldCalibrationPoints.Length != 3 || _mapCalibrationPoints.Length != 3) return false;
        for (int i = 0; i < 3; i++)
            if (!_worldCalibrationPoints[i] || !_mapCalibrationPoints[i]) return false;
        Vector3 origin = _worldCalibrationPoints[0].position;
        Vector3 x = _worldCalibrationPoints[1].position - origin;
        Vector3 y = _worldCalibrationPoints[2].position - origin;
        float det = x.x * y.z - x.z * y.x;
        if (Mathf.Abs(det) < 0.0001f) return false;
        Vector3 p = worldPosition - origin;
        float u = (p.x * y.z - p.z * y.x) / det;
        float v = (x.x * p.z - x.z * p.x) / det;
        Vector3 a = _mapRect.InverseTransformPoint(_mapCalibrationPoints[0].position);
        Vector3 b = _mapRect.InverseTransformPoint(_mapCalibrationPoints[1].position);
        Vector3 c = _mapRect.InverseTransformPoint(_mapCalibrationPoints[2].position);
        mapPosition = a + u * (b - a) + v * (c - a);
        mapPosition.z = 0;
        return true;
    }

    private MenuUI _menu;
    private Vector2 _initialMapPosition;
    private Quaternion _initialMapRotation;
    private bool _initialized;

    public override void Initialize(MenuUI menu)
    {
        base.Initialize(menu);
        _menu = menu;
        var background=transform.Find("Map_BG");
        if(background) background.SetAsFirstSibling();
        if (!_mapRect && transform.Find("Map_Image")) _mapRect = transform.Find("Map_Image").GetComponent<RectTransform>();
        if (!_playerIcon && transform.Find("Player_Icon")) _playerIcon = transform.Find("Player_Icon").GetComponent<RectTransform>();
        if (_mapRect && !_initialized)
        {
            _initialMapPosition = _mapRect.anchoredPosition;
            _initialMapRotation = _mapRect.localRotation;
            _initialized = true;
        }
        RefreshPosition();
    }

    public override void OnOpen() => RefreshPosition();
    private void LateUpdate() => RefreshPosition();

    public void RefreshPosition()
    {
        RefreshDungeonMarkers();
        // The player can spawn after MenuUI.Start or be replaced during scene changes.
        var player = _menu ? _menu.Player : null;
        if (!player || !_mapRect || !_playerIcon) return;

        Vector3 position = player.transform.position;
        Vector2 uv;
        float heading;
        if (_mapAreaTransform)
        {
            if (Mathf.Abs(_mapAreaTransform.lossyScale.x) < 0.0001f ||
                Mathf.Abs(_mapAreaTransform.lossyScale.z) < 0.0001f) return;
            Vector3 local = _mapAreaTransform.InverseTransformPoint(position);
            uv = new Vector2(local.x + 0.5f, local.z + 0.5f);
            Vector3 forward = _mapAreaTransform.InverseTransformDirection(player.transform.forward);
            heading = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        }
        else
        {
            Vector2 span = _worldMax - _worldMin;
            if (Mathf.Abs(span.x) < 0.0001f || Mathf.Abs(span.y) < 0.0001f) return;
            uv = new Vector2((position.x - _worldMin.x) / span.x,
                             (position.z - _worldMin.y) / span.y);
            heading = player.transform.eulerAngles.y;
        }

        Rect rect = _mapRect.rect;
        Vector3 mapPoint = new Vector3(rect.xMin + uv.x * rect.width,
                                      rect.yMin + uv.y * rect.height, 0f);
        if (TryGetCalibratedMapPosition(position, out var calibrated))
        {
            mapPoint = calibrated;
            if (TryGetCalibratedMapPosition(position + player.transform.forward, out var ahead))
            {
                Vector3 direction = ahead - calibrated;
                heading = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            }
        }
        if (_rotateMap && _initialized)
        {
            _mapRect.localRotation = _initialMapRotation * Quaternion.Euler(0, 0, heading);
            _mapRect.anchoredPosition = _initialMapPosition;
            var parent = _mapRect.parent;
            Vector3 pointInParent = parent.InverseTransformPoint(_mapRect.TransformPoint(mapPoint));
            Vector3 centerInParent = parent.InverseTransformPoint(_mapRect.TransformPoint(rect.center));
            _mapRect.localPosition += centerInParent - pointInParent;
        }
        // World-space conversion supports an icon that is a sibling of the map,
        // different anchors/pivots, and scaled UI containers.
        _playerIcon.position = _mapRect.TransformPoint(mapPoint);
        _playerIcon.rotation = _rotateMap ? _mapRect.parent.rotation :
            _mapRect.rotation * Quaternion.Euler(0, 0, _rotateIcon ? -heading : 0);
    }

    public override void Dispose()
    {
        if (_mapRect && _initialized)
        {
            _mapRect.anchoredPosition = _initialMapPosition;
            _mapRect.localRotation = _initialMapRotation;
        }
        _menu = null;
        base.Dispose();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (_mapAreaTransform)
        {
            Gizmos.matrix = _mapAreaTransform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
        else
        {
            Vector2 center = (_worldMin + _worldMax) * 0.5f;
            Vector2 size = _worldMax - _worldMin;
            Gizmos.DrawWireCube(new Vector3(center.x, 0, center.y), new Vector3(size.x, 1, size.y));
        }
    }
}
