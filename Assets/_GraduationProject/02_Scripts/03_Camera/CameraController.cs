using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 락온 여부 및 적 감지 상태에 따라 시네머신 가상 카메라의 우선순위를 조절하고,
/// 각 카메라 목적에 맞는 개별 Target Group을 관리하는 컨트롤러입니다.
/// </summary>
public class CameraController : MonoBehaviour, IEventListener<EnemyStateData>
{
    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineCamera _defaultCamera;
    [SerializeField] private CinemachineCamera _combatCamera;
    [SerializeField] private CinemachineCamera _lockOnCamera;
    [SerializeField] private CinemachineCamera _bossCamera;

    [Header("Target Groups")]
    [Tooltip("전투용 그룹: 플레이어 + 감지된 모든 적을 포함합니다.")]
    [SerializeField] private CinemachineTargetGroup _combatTargetGroup;
    [Tooltip("락온용 그룹: 플레이어 + 현재 락온 대상만 포함합니다.")]
    [SerializeField] private CinemachineTargetGroup _lockOnTargetGroup;
    [Tooltip("보스용 그룹: 플레이어 + 보스를 포함합니다.")]
    [SerializeField] private CinemachineTargetGroup _bossTargetGroup;

    [Header("Combat Group Settings")]
    [SerializeField] private float _combatPlayerWeight = 1f;
    [SerializeField] private float _combatPlayerRadius = 2f;
    [SerializeField] private float _combatEnemyWeight = 0.8f;
    [SerializeField] private float _combatEnemyRadius = 3f;

    [Header("LockOn Group Settings")]
    [SerializeField] private float _lockOnPlayerWeight = 1f;
    [SerializeField] private float _lockOnPlayerRadius = 2f;
    [SerializeField] private float _lockOnEnemyWeight = 1f;
    [SerializeField] private float _lockOnEnemyRadius = 1f;

    [Header("Boss Group Settings")]
    [SerializeField] private float _bossPlayerWeight = 1f;
    [SerializeField] private float _bossPlayerRadius = 3f;
    [SerializeField] private float _bossEnemyWeight = 1.2f;
    [SerializeField] private float _bossEnemyRadius = 5f;

    [Header("Event Channels")]
    [SerializeField] private EnemyStateEventSO _onEnemyStateChanged;

    [Header("References")]
    private PlayerLockOn _playerLockOn;
    private Transform _playerTransform;

    [Header("Priority Settings")]
    [SerializeField] private int _activePriority = 15;
    [SerializeField] private int _inactivePriority = 10;

    // 현재 플레이어를 발견한 적들의 리스트
    private HashSet<Enemy> _detectedEnemies = new HashSet<Enemy>();
    // 보스 리스트 (여러 마리일 경우 대비 혹은 단일 보스 확인용)
    private HashSet<Enemy> _bossEnemies = new HashSet<Enemy>();

    /// <summary>
    /// 외부(CameraManager)에서 플레이어 정보를 설정하고 그룹을 초기화합니다.
    /// </summary>
    public void Setup(PlayerController player)
    {
        _playerLockOn = player.LockOn;
        _playerTransform = player.transform;
        
        if (_playerLockOn != null)
            _playerLockOn.LockOnEvent += HandleLockOnChanged;

        InitializeTargetGroups();
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
            
        SetAllPriorities(_inactivePriority);
    }

    private void OnDestroy()
    {
        if (_playerLockOn != null)
            _playerLockOn.LockOnEvent -= HandleLockOnChanged;
    }

    /// <summary>
    /// 모든 타겟 그룹에 플레이어를 기본 멤버로 추가합니다.
    /// </summary>
    private void InitializeTargetGroups()
    {
        if (_playerTransform == null) return;

        if (_combatTargetGroup != null)
        {
            _combatTargetGroup.Targets.Clear();
            _combatTargetGroup.AddMember(_playerTransform, _combatPlayerWeight, _combatPlayerRadius);
        }

        if (_lockOnTargetGroup != null)
        {
            _lockOnTargetGroup.Targets.Clear();
            _lockOnTargetGroup.AddMember(_playerTransform, _lockOnPlayerWeight, _lockOnPlayerRadius);
        }

        if (_bossTargetGroup != null)
        {
            _bossTargetGroup.Targets.Clear();
            _bossTargetGroup.AddMember(_playerTransform, _bossPlayerWeight, _bossPlayerRadius);
        }
    }

    /// <summary>
    /// 몬스터 상태 변화 시 전투 타겟 그룹 멤버를 갱신합니다.
    /// </summary>
    public void OnEventTrigger(EnemyStateData data)
    {
        switch (data.stateType)
        {
            case EnemyStateType.Detected:
                if (_detectedEnemies.Add(data.enemy))
                {
                    if (_combatTargetGroup != null)
                        _combatTargetGroup.AddMember(data.enemy.transform, _combatEnemyWeight, _combatEnemyRadius);
                }
                break;
            case EnemyStateType.Lost:
            case EnemyStateType.Dead:
                // 일반 적 목록에서 제거
                if (_detectedEnemies.Remove(data.enemy))
                {
                    if (_combatTargetGroup != null)
                        _combatTargetGroup.RemoveMember(data.enemy.transform);
                }
                // 보스 목록에서 제거 (보스가 죽었거나 놓쳤을 때)
                if (_bossEnemies.Remove(data.enemy))
                {
                    if (_bossTargetGroup != null)
                        _bossTargetGroup.RemoveMember(data.enemy.transform);
                }
                break;
            case EnemyStateType.SummonBoss:
                if (_bossEnemies.Add(data.enemy))
                {
                    if (_bossTargetGroup != null)
                        _bossTargetGroup.AddMember(data.enemy.transform, _bossEnemyWeight, _bossEnemyRadius);
                }
                break;
        }

        if (enabled) UpdateCameraPriorities();
    }

    /// <summary>
    /// 락온 상태 변화 시 락온 타겟 그룹 멤버를 갱신합니다.
    /// </summary>
    private void HandleLockOnChanged(bool isLockOn)
    {
        UpdateLockOnTargetGroup(isLockOn);
        if (enabled) UpdateCameraPriorities();
    }

    /// <summary>
    /// 락온 타겟 그룹 멤버를 갱신합니다 (플레이어 + 현재 락온 대상).
    /// </summary>
    private void UpdateLockOnTargetGroup(bool isLockOn)
    {
        if (_lockOnTargetGroup == null || _playerTransform == null) return;

        // 그룹 비우고 플레이어 다시 추가
        _lockOnTargetGroup.Targets.Clear();
        _lockOnTargetGroup.AddMember(_playerTransform, _lockOnPlayerWeight, _lockOnPlayerRadius);

        // 락온 중이고 유효한 타겟이 있다면 타겟 추가
        if (isLockOn && _playerLockOn != null && _playerLockOn.CurrentTarget != null)
        {
            // 타겟이 플레이어 자신이 아닐 경우에만 추가
            if (_playerLockOn.CurrentTarget != _playerTransform)
            {
                _lockOnTargetGroup.AddMember(_playerLockOn.CurrentTarget, _lockOnEnemyWeight, _lockOnEnemyRadius);
            }
        }
    }

    private void SetAllPriorities(int priority)
    {
        if (_defaultCamera) _defaultCamera.Priority = priority;
        if (_combatCamera) _combatCamera.Priority = priority;
        if (_lockOnCamera) _lockOnCamera.Priority = priority;
        if (_bossCamera) _bossCamera.Priority = priority;
    }

    /// <summary>
    /// 상황에 맞게 카메라 우선순위를 갱신합니다.
    /// </summary>
    public void UpdateCameraPriorities()
    {
        if (!enabled || _defaultCamera == null || _combatCamera == null || _lockOnCamera == null) return;

        SetAllPriorities(_inactivePriority);

        // 1순위: 보스 등장 상태
        if (_bossEnemies.Count > 0 && _bossCamera != null)
        {
            _bossCamera.Priority = _activePriority;
        }
        // 2순위: 락온 상태
        else if (_playerLockOn != null && _playerLockOn.IsLockOn)
        {
            _lockOnCamera.Priority = _activePriority;
        }
        // 3순위: 주변 적 감지 상태
        else if (_detectedEnemies.Count > 0)
        {
            _combatCamera.Priority = _activePriority;
        }
        // 4순위: 평상시
        else
        {
            _defaultCamera.Priority = _activePriority;
        }
    }
}
