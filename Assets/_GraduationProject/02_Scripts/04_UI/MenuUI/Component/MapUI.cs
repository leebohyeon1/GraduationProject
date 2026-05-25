using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

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

    // Job 관련 변수
    private NativeArray<float3> _playerData; // [0]: Position, [1]: Rotation (Euler)
    private NativeArray<float3> _mapAreaData; // [0]: Position, [1]: Rotation (Euler), [2]: Scale
    private NativeArray<float2> _resultData; // [0]: AnchoredPosition, [1]: IconRotation, [2]: MapRotation
    private JobHandle _jobHandle;

    [BurstCompile]
    struct MapUIJob : IJob
    {
        [ReadOnly] public float3 PlayerPos;
        [ReadOnly] public float3 PlayerRot;
        [ReadOnly] public float3 MapAreaPos;
        [ReadOnly] public float3 MapAreaRot;
        [ReadOnly] public float3 MapAreaScale;
        [ReadOnly] public float2 MapSize;
        [ReadOnly] public float2 MapPivot;
        [ReadOnly] public float2 WorldMin;
        [ReadOnly] public float2 WorldMax;
        [ReadOnly] public bool UseMapArea;
        [ReadOnly] public bool RotateMap;
        [ReadOnly] public bool RotateIcon;

        public NativeArray<float2> Results;

        public void Execute()
        {
            float normalizedX, normalizedY;

            if (UseMapArea)
            {
                // InverseTransformPoint 수동 계산
                float3 relativePos = PlayerPos - MapAreaPos;
                quaternion rotation = quaternion.Euler(math.radians(MapAreaRot));
                float3 localPos = math.mul(math.inverse(rotation), relativePos);
                localPos /= MapAreaScale;

                normalizedX = localPos.x + 0.5f;
                normalizedY = localPos.z + 0.5f;
            }
            else
            {
                normalizedX = (PlayerPos.x - WorldMin.x) / (WorldMax.x - WorldMin.x);
                normalizedY = (PlayerPos.z - WorldMin.y) / (WorldMax.y - WorldMin.y);
            }

            float anchoredX = (normalizedX - MapPivot.x) * MapSize.x;
            float anchoredY = (normalizedY - MapPivot.y) * MapSize.y;

            float areaRotationY = MapAreaRot.y;
            float playerRotationY = PlayerRot.y;

            if (RotateMap)
            {
                Results[0] = new float2(anchoredX, anchoredY);
                Results[1] = 0;
                Results[2] = playerRotationY - areaRotationY;
            }
            else
            {
                Results[0] = new float2(anchoredX, anchoredY);
                if (RotateIcon)
                {
                    Results[1] = -(playerRotationY - areaRotationY);
                }
                else
                {
                    Results[1] = 0;
                }
                Results[2] = 0;
            }
        }
    }

    public override void Initialize(MenuUI menu)
    {
        if (_mapAreaTransform == null)
        {
            return;
        }

        base.Initialize(menu);
        _player = menu.Player;

        _resultData = new NativeArray<float2>(3, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (_resultData.IsCreated) _resultData.Dispose();
    }

    private void LateUpdate()
    {
        if (_mapAreaTransform == null)
        {
            return;
        }

        if (_player == null || _mapRect == null || _playerIcon == null) return;

        // Job 데이터 준비 및 스케줄링
        MapUIJob job = new MapUIJob
        {
            PlayerPos = _player.transform.position,
            PlayerRot = _player.transform.eulerAngles,
            MapAreaPos = _mapAreaTransform != null ? (float3)_mapAreaTransform.position : float3.zero,
            MapAreaRot = _mapAreaTransform != null ? (float3)_mapAreaTransform.eulerAngles : float3.zero,
            MapAreaScale = _mapAreaTransform != null ? (float3)_mapAreaTransform.localScale : new float3(1, 1, 1),
            MapSize = _mapRect.rect.size,
            MapPivot = _mapRect.pivot,
            WorldMin = _worldMin,
            WorldMax = _worldMax,
            UseMapArea = _mapAreaTransform != null,
            RotateMap = _rotateMap,
            RotateIcon = _rotateIcon,
            Results = _resultData
        };

        _jobHandle = job.Schedule();
    }

    private void Update()
    {
        if(_mapAreaTransform == null)
        {
            return;
        }

        // 이전 프레임의 LateUpdate에서 예약된 Job이 완료되었는지 확인하고 결과 적용
        _jobHandle.Complete();

        if (_rotateMap)
        {
            _playerIcon.anchoredPosition = Vector2.zero;
            _mapRect.anchoredPosition = new Vector2(-_resultData[0].x, -_resultData[0].y);
            _mapRect.localEulerAngles = new Vector3(0, 0, _resultData[2].x);
            _playerIcon.localEulerAngles = Vector3.zero;
        }
        else
        {
            _playerIcon.anchoredPosition = _resultData[0];
            _playerIcon.localEulerAngles = new Vector3(0, 0, _resultData[1].x);
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
