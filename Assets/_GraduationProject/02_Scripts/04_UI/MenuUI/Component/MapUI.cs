using UnityEngine;

public class MapUI : MenuUIComponent
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform _mapRect;      // 전체 지도 이미지의 RectTransform
    [SerializeField] private RectTransform _playerIcon;   // 지도 위에 표시될 플레이어 아이콘

    [Header("Setup via Object (Recommended)")]
    [Tooltip("실제 게임 월드에서 맵의 범위를 나타낼 빈 오브젝트를 넣어주세요. (Cube를 만들어서 맞춘 뒤 MeshRenderer만 꺼도 됩니다)")]
    [SerializeField] private Transform _mapAreaTransform;

    [Header("World Settings")]
    [Tooltip("지도의 왼쪽 하단(Left-Bottom) 모서리가 대응하는 실제 세계의 X, Z 좌표")]
    [SerializeField] private Vector2 _worldMin = new Vector2(-100, -100);

    [Tooltip("지도의 오른쪽 상단(Right-Top) 모서리가 대응하는 실제 세계의 X, Z 좌표")]
    [SerializeField] private Vector2 _worldMax = new Vector2(100, 100);

    [Header("Options")]
    [SerializeField] private bool _rotateIcon = true;     // 플레이어 방향에 맞춰 아이콘 회전
    [SerializeField] private bool _rotateMap = false;     // 플레이어 방향에 맞춰 지도를 회전 (아이콘은 고정)

    private PlayerController _player;

    public override void Initialize(MenuUI menu)
    {
        base.Initialize(menu);
        _player = menu.Player;
    }

    private void LateUpdate()
    {
        if (_player == null || _mapRect == null || _playerIcon == null) return;

        UpdateMinimap();
    }

    private void UpdateMinimap()
    {
        Vector3 playerPos = _player.transform.position;
        float normalizedX, normalizedY;

        if (_mapAreaTransform != null)
        {
            // 1. Map Area 오브젝트의 로컬 좌표로 플레이어 위치 변환 (회전 대응 가능!)
            Vector3 localPos = _mapAreaTransform.InverseTransformPoint(playerPos);

            // 2. 오브젝트의 Scale이 1일 때, 로컬 좌표는 -0.5 ~ 0.5 범위를 가집니다.
            // 이를 0~1 사이의 정규화된 값으로 변환합니다.
            normalizedX = localPos.x + 0.5f;
            normalizedY = localPos.z + 0.5f;
        }
        else
        {
            // 수동 좌표 방식 (회전 대응 불가)
            normalizedX = Mathf.InverseLerp(_worldMin.x, _worldMax.x, playerPos.x);
            normalizedY = Mathf.InverseLerp(_worldMin.y, _worldMax.y, playerPos.z);
        }

        // 3. 지도 UI의 실제 크기 가져오기
        Vector2 mapSize = _mapRect.rect.size;

        // 4. RectTransform의 Pivot을 고려하여 아이콘의 AnchoredPosition 계산
        float anchoredX = (normalizedX - _mapRect.pivot.x) * mapSize.x;
        float anchoredY = (normalizedY - _mapRect.pivot.y) * mapSize.y;

        float areaRotationY = _mapAreaTransform != null ? _mapAreaTransform.eulerAngles.y : 0f;

        if (_rotateMap)
        {
            // 지도가 회전하는 방식 (플레이어는 중앙 고정)
            _playerIcon.anchoredPosition = Vector2.zero;
            _mapRect.anchoredPosition = new Vector2(-anchoredX, -anchoredY);

            float relativeRotation = _player.transform.eulerAngles.y - areaRotationY;
            _mapRect.localEulerAngles = new Vector3(0, 0, relativeRotation);
            _playerIcon.localEulerAngles = Vector3.zero;
        }
        else
        {
            // 아이콘이 움직이는 방식
            _playerIcon.anchoredPosition = new Vector2(anchoredX, anchoredY);

            if (_rotateIcon)
            {
                float relativeRotation = _player.transform.eulerAngles.y - areaRotationY;
                _playerIcon.localEulerAngles = new Vector3(0, 0, -relativeRotation);
            }
        }
    }

    // 인스펙터에서 범위를 직관적으로 확인하기 위한 기즈모
    private void OnDrawGizmos()
    {
        if (_mapAreaTransform != null)
        {
            Gizmos.matrix = _mapAreaTransform.localToWorldMatrix;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
        else
        {
            Gizmos.color = Color.green;
            Vector3 center = new Vector3((_worldMin.x + _worldMax.x) * 0.5f, 0, (_worldMin.y + _worldMax.y) * 0.5f);
            Vector3 size = new Vector3(_worldMax.x - _worldMin.x, 1, _worldMax.y - _worldMin.y);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
