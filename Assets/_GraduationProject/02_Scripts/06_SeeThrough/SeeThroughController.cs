using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// 장애물 투명화 컨트롤러 (Job System을 사용한 최적화 버전)
/// </summary>
public class SeeThroughController : MonoBehaviour, IEventListener<SeeThroughTargetTransform>
{
    [SerializeField] private OnRegisterSeeThroughTargetSO _onRegisterCutOutTargetSO;

    [Header("Target Settings")]
    private List<Transform> _targetObjects = new List<Transform>(); // Job 처리를 위해 List로 변경

    [Header("Layer Settings")]
    public LayerMask _wallLayer;

    private Camera _mainCamera;
    private HashSet<SeeThroughObject> _previouslyHitObjects = new HashSet<SeeThroughObject>();

    public const float VALUE_VISIBLE = 0.0f;
    public const float VALUE_INVISIBLE = 1.0f;

    // Job System 관련 변수
    private NativeArray<RaycastCommand> _commands;
    private NativeArray<RaycastHit> _results;
    private const int MAX_HITS_PER_RAY = 5; // 한 레이당 최대 감지할 장애물 수

    private void Awake()
    {
        _onRegisterCutOutTargetSO.Subscribe(this);
    }

    private void OnEnable()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_targetObjects.Count == 0)
        {
            ResetAllOcclusion();
            return;
        }

        HandleOcclusionJob();
    }

    private void OnDisable()
    {
        _onRegisterCutOutTargetSO.Unsubscribe(this);
        _targetObjects.Clear();
        
        if (_commands.IsCreated) _commands.Dispose();
        if (_results.IsCreated) _results.Dispose();
    }

    private void ResetAllOcclusion()
    {
        if (_previouslyHitObjects.Count > 0)
        {
            foreach (var obj in _previouslyHitObjects)
            {
                if (obj != null) obj.SetOcclusionStatus(false);
            }
            _previouslyHitObjects.Clear();
        }
    }

    void HandleOcclusionJob()
    {
        int targetCount = _targetObjects.Count;
        
        // NativeArray 할당 및 재할당 체크
        if (!_commands.IsCreated || _commands.Length != targetCount)
        {
            if (_commands.IsCreated) _commands.Dispose();
            if (_results.IsCreated) _results.Dispose();

            _commands = new NativeArray<RaycastCommand>(targetCount, Allocator.Persistent);
            _results = new NativeArray<RaycastHit>(targetCount * MAX_HITS_PER_RAY, Allocator.Persistent);
        }

        Vector3 camPos = _mainCamera.transform.position;
        QueryParameters queryParams = new QueryParameters(_wallLayer, false, QueryTriggerInteraction.Collide, false);

        // 1. RaycastCommand 준비 (메인 스레드)
        for (int i = 0; i < targetCount; i++)
        {
            Vector3 targetPos = _targetObjects[i].position;
            Vector3 dir = targetPos - camPos;
            float dist = dir.magnitude;
            
            _commands[i] = new RaycastCommand(camPos, dir.normalized, queryParams, dist);
        }

        // 2. Job 실행 (멀티 스레드 병렬 처리)
        JobHandle handle = RaycastCommand.ScheduleBatch(_commands, _results, 1, MAX_HITS_PER_RAY);
        handle.Complete(); // 결과를 즉시 활용하기 위해 대기

        // 3. 결과 처리 (메인 스레드)
        HashSet<SeeThroughObject> newHits = new HashSet<SeeThroughObject>();

        for (int i = 0; i < targetCount; i++)
        {
            // 각 레이당 여러 히트 결과 확인
            for (int j = 0; j < MAX_HITS_PER_RAY; j++)
            {
                RaycastHit hit = _results[i * MAX_HITS_PER_RAY + j];
                if (hit.collider == null) break;

                SeeThroughObject cutOutObject = hit.collider.GetComponent<SeeThroughObject>();
                if (cutOutObject != null)
                {
                    newHits.Add(cutOutObject);
                    cutOutObject.SetTarget(_targetObjects[i]);
                    cutOutObject.SetOcclusionStatus(true);
                }
            }
        }

        // 4. 이전 상태 복구
        foreach (SeeThroughObject prevHit in _previouslyHitObjects)
        {
            if (prevHit != null && !newHits.Contains(prevHit))
            {
                prevHit.SetOcclusionStatus(false);
            }
        }
        
        _previouslyHitObjects = newHits;
    }
    
    public void OnEventTrigger(SeeThroughTargetTransform value)
    {
        if (value.IsRegister)
        {
            if (!_targetObjects.Contains(value.Target))
                _targetObjects.Add(value.Target);
        }
        else
        {
            _targetObjects.Remove(value.Target);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (_targetObjects != null && _mainCamera != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform t in _targetObjects) {
                if(t != null) Gizmos.DrawLine(_mainCamera.transform.position, t.position);
            } 
        }
    }
}
