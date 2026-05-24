using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 유니티 Job 시스템(RaycastCommand)을 사용하여 비동기로 레이어를 감지하는 컴포넌트
/// </summary>
public class LayerRaycastJobDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Vector3 _rayOffset = Vector3.zero;
    [SerializeField] private Vector3 _rayDirection = Vector3.down;
    [SerializeField] private float _rayDistance = 1.0f;
    [SerializeField] private LayerMask _hitMask = -1;
    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.UseGlobal;

    [Header("Detection Target")]
    [SerializeField] private string _targetLayerName = "Water";

    [Header("Response Settings")]
    [SerializeField] private GameObject _targetObject;
    [SerializeField] private bool _invertLogic = false;

    [Header("Events")]
    public UnityEvent OnTargetLayerDetected;
    public UnityEvent OnTargetLayerLost;

    private int _targetLayerIndex;
    private bool _isDetected = false;

    // Job 관련 필드
    private NativeArray<RaycastCommand> _commands;
    private NativeArray<RaycastHit> _results;
    private JobHandle _jobHandle;
    private bool _isJobScheduled = false;

    private void Awake()
    {
        _targetLayerIndex = LayerMask.NameToLayer(_targetLayerName);
        
        // NativeArray 초기화 (1개 분량)
        _commands = new NativeArray<RaycastCommand>(1, Allocator.Persistent);
        _results = new NativeArray<RaycastHit>(1, Allocator.Persistent);
    }

    private void Update()
    {
        // 1. RaycastCommand 준비 및 스케줄링
        Vector3 origin = transform.position + _rayOffset;
        
        // RaycastCommand는 구조체이므로 직접 생성하여 할당
        _commands[0] = new RaycastCommand(origin, _rayDirection, _rayDistance, _hitMask, (int)_triggerInteraction);
        
        // 물리 엔진 Job 스케줄링
        _jobHandle = RaycastCommand.ScheduleBatch(_commands, _results, 1);
        _isJobScheduled = true;
    }

    private void LateUpdate()
    {
        if (!_isJobScheduled) return;

        // 2. Job 완료 대기
        _jobHandle.Complete();
        _isJobScheduled = false;

        // 3. 결과 처리
        ProcessResults();
    }

    private void ProcessResults()
    {
        if (_targetLayerIndex == -1) return;

        RaycastHit hit = _results[0];
        bool currentlyDetected = false;

        // 충돌 결과 확인 (RaycastHit.collider가 null이 아니면 충돌한 것)
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == _targetLayerIndex)
            {
                currentlyDetected = true;
            }
        }

        // 상태 변화 체크
        if (currentlyDetected != _isDetected)
        {
            _isDetected = currentlyDetected;
            ApplyResponse(_isDetected);
        }
    }

    private void ApplyResponse(bool detected)
    {
        if (detected)
        {
            OnTargetLayerDetected?.Invoke();
            if (_targetObject != null) _targetObject.SetActive(!_invertLogic);
        }
        else
        {
            OnTargetLayerLost?.Invoke();
            if (_targetObject != null) _targetObject.SetActive(_invertLogic);
        }
    }

    private void OnDestroy()
    {
        // Job 완료 보장 및 메모리 해제
        if (_isJobScheduled) _jobHandle.Complete();
        
        if (_commands.IsCreated) _commands.Dispose();
        if (_results.IsCreated) _results.Dispose();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isDetected ? Color.green : Color.yellow;
        Vector3 origin = transform.position + _rayOffset;
        Gizmos.DrawRay(origin, _rayDirection * _rayDistance);
        Gizmos.DrawWireSphere(origin + _rayDirection * _rayDistance, 0.1f);
    }

    public bool IsDetected => _isDetected;
}
