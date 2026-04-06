using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 핵심 제어를 담당하는 클래스 (핵심 로직 및 상태 관리)
/// </summary>
public partial class PlayerController : MonoBehaviour
{
    private StateMachine<PlayerController> _stateMachine;
    private List<IDisposable> _disposableList = new List<IDisposable>();
    
    public StateMachine<PlayerController> FSM => _stateMachine;
    public PlayerData RuntimeData { get; private set; }

    private async void Start()
    {        
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
        // FSM Update
        _stateMachine?.Update(); 
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
