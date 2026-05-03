using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 락온 여부 및 적 감지 상태에 따라 시네머신 가상 카메라의 우선순위를 조절하고,
/// 전투 카메라 시 Target Group을 통해 적들을 화면에 담는 컨트롤러입니다.
/// CameraManager에 의해 활성화/비활성화 될 수 있습니다.
/// </summary>
public class CameraController : MonoBehaviour, IEventListener<EnemyStateData>
{
    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineCamera _defaultCamera;
    [SerializeField] private CinemachineCamera _combatCamera;
    [SerializeField] private CinemachineCamera _lockOnCamera;

    [Header("Target Group (For Combat Camera)")]
    [SerializeField] private CinemachineTargetGroup _targetGroup;
    [SerializeField] private float _playerWeight = 1f;
    [SerializeField] private float _playerRadius = 2f;
    [SerializeField] private float _enemyWeight = 0.8f;
    [SerializeField] private float _enemyRadius = 3f;

    [Header("Event Channels")]
    [SerializeField] private EnemyStateEventSO _onEnemyStateChanged;

    [Header("References")]
    private PlayerLockOn _playerLockOn;
    private Transform _playerTransform;

    [Header("Priority Settings")]
    [SerializeField] private int _activePriority = 15;
    [SerializeField] private int _inactivePriority = 10;

    // 현재 나를 발견한 적들의 리스트를 관리합니다.
    private HashSet<Enemy> _detectedEnemies = new HashSet<Enemy>();

    /// <summary>
    /// 외부(CameraManager)에서 플레이어 정보를 설정합니다.
    /// </summary>
    public void Setup(PlayerController player)
    {
        _playerLockOn = player.LockOn;
        _playerTransform = player.transform;

        _defaultCamera.Follow = _playerTransform;

        if (_playerLockOn != null)
        {
            _playerLockOn.LockOnEvent += HandleLockOnChanged;
        }
        InitializeTargetGroup();
    }

    private void OnEnable()
    {
        if (_onEnemyStateChanged != null)
            _onEnemyStateChanged.Subscribe(this);
            
        UpdateCameraPriorities();
    }

    private void OnDisable()
    {
        if (_onEnemyStateChanged != null)
            _onEnemyStateChanged.Unsubscribe(this);
            
        // 비활성화 시 모든 카메라 우선순위 낮춤
        SetAllPriorities(_inactivePriority);
    }

    private void OnDestroy()
    {
        if (_playerLockOn != null)
            _playerLockOn.LockOnEvent -= HandleLockOnChanged;
    }

    private void InitializeTargetGroup()
    {
        if (_targetGroup == null || _playerTransform == null) return;

        _targetGroup.Targets.Clear();
        _targetGroup.AddMember(_playerTransform, _playerWeight, _playerRadius);
    }

    public void OnEventTrigger(EnemyStateData data)
    {
        if (_targetGroup == null) return;

        switch (data.stateType)
        {
            case EnemyStateType.Detected:
                if (_detectedEnemies.Add(data.enemy))
                    _targetGroup.AddMember(data.enemy.transform, _enemyWeight, _enemyRadius);
                break;
            case EnemyStateType.Lost:
            case EnemyStateType.Dead:
                if (_detectedEnemies.Remove(data.enemy))
                    _targetGroup.RemoveMember(data.enemy.transform);
                break;
        }

        if (enabled) UpdateCameraPriorities();
    }

    private void HandleLockOnChanged(bool isLockOn)
    {
        if (enabled) UpdateCameraPriorities();
    }

    private void SetAllPriorities(int priority)
    {
        if (_defaultCamera) _defaultCamera.Priority = priority;
        if (_combatCamera) _combatCamera.Priority = priority;
        if (_lockOnCamera) _lockOnCamera.Priority = priority;
    }

    public void UpdateCameraPriorities()
    {
        if (!enabled || _defaultCamera == null || _combatCamera == null || _lockOnCamera == null) return;

        SetAllPriorities(_inactivePriority);

        // 1순위: 락온
        if (_playerLockOn != null && _playerLockOn.IsLockOn)
        {
            _lockOnCamera.Priority = _activePriority;
        }
        // 2순위: 전투
        else if (_detectedEnemies.Count > 0)
        {
            _combatCamera.Priority = _activePriority;
        }
        // 3순위: 기본
        else
        {
            _defaultCamera.Priority = _activePriority;
        }
    }
}
