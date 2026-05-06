using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

/// <summary>
/// 플레이어의 핵심 제어를 담당하는 클래스 (핵심 로직 및 상태 관리)
/// </summary>
public partial class PlayerController : MonoBehaviour
{
    private StateMachine<PlayerController> _stateMachine;
    private List<IDisposable> _disposableList = new List<IDisposable>();
    
    // Job System 관련 필드
    private NativeArray<bool> _isFallDeadResult;
    private JobHandle _fallCheckJobHandle;

    public StateMachine<PlayerController> FSM => _stateMachine;
    public PlayerData RuntimeData { get; private set; }

    private async void Start()
    {        
        // NativeArray 초기화 (Persistent: 객체 수명 동안 유지)
        _isFallDeadResult = new NativeArray<bool>(1, Allocator.Persistent);

        // 1. 참조 및 데이터 초기화 (PlayerController.Initialization.cs)
        await InitializeReferences();
        
        // 2. FSM 초기화 (PlayerController.FSM.cs)
        InitializeFSM();

        // 3. 초기화 후 스폰 이벤트 발행
        if (playerSpawnedSO != null)
        {
            playerSpawnedSO.Publish(this);
        }
    }

    private void Update()
    {
        // Y축 추락 사망 체크 (Job System 활용)
        ScheduleFallCheckJob();

        // FSM Update
        _stateMachine?.Update(); 
    }

    private void LateUpdate()
    {
        // Job 완료 확인 및 처리
        CompleteFallCheckJob();
    }

    /// <summary>
    /// Job System을 사용하여 Y축 위치 체크 작업을 예약합니다.
    /// </summary>
    private void ScheduleFallCheckJob()
    {
        if (RuntimeData == null || Health == null || Health.IsDead)
        {
            return;
        }

        FallCheckJob job = new FallCheckJob
        {
            Position = transform.position,
            Threshold = RuntimeData.FallThresholdY,
            Result = _isFallDeadResult
        };

        _fallCheckJobHandle = job.Schedule();
    }

    /// <summary>
    /// 예약된 Job을 완료하고 결과에 따라 사망 처리를 수행합니다.
    /// </summary>
    private void CompleteFallCheckJob()
    {
        if (Health == null || Health.IsDead) return;

        // Job 완료 대기
        _fallCheckJobHandle.Complete();

        if (_isFallDeadResult[0])
        {
            Health.Die();
            _isFallDeadResult[0] = false; // 결과 초기화
        }
    }

    private void FixedUpdate()
    {
        // FSM FixedUpdate
        _stateMachine?.FixedUpdate();
    }

    private void OnDestroy()
    {
        Dispose();
    }

    /// <summary>
    /// 객체 폐기 및 자원 해제
    /// </summary>
    public void Dispose()
    {
        // NativeArray 해제
        if (_isFallDeadResult.IsCreated)
        {
            _fallCheckJobHandle.Complete(); // 해제 전 작업 완료 보장
            _isFallDeadResult.Dispose();
        }

        _stateMachine?.Dispose();

        foreach(IDisposable disposable in _disposableList)
        {
            disposable.Dispose();
        }

        if (_events != null)
        {
            _events.ClearAllEvents();
        }
        
        _disposableList.Clear();
    }

    /// <summary>
    /// Y축 추락 체크를 위한 Job 구조체
    /// </summary>
    public struct FallCheckJob : IJob
    {
        [ReadOnly] public Vector3 Position;
        [ReadOnly] public float Threshold;
        public NativeArray<bool> Result;

        public void Execute()
        {
            Result[0] = Position.y < Threshold;
        }
    }

    /// <summary>
    /// Disposable 객체 등록 (상태나 컴포넌트에서 해제가 필요한 경우)
    /// </summary>
    /// <param name="disposable">구독할 객체</param>
    public void RegisterDisposable(IDisposable disposable)
    {
        if(_disposableList.Contains(disposable))
        {
            return;
        }

        _disposableList.Add(disposable);
    }
}
