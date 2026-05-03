using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 카메라 채널
/// </summary>
[Serializable]
public class CameraChannel
{
    public string ChannelName;  // 채널 이름
    public CinemachineCamera Camera;
}

/// <summary>
/// 카메라를 관리하는 매니저.
/// 평상시에는 CameraController를 통해 자동 전환을 수행하며,
/// 이벤트 발생 시 특정 채널로 강제 전환할 수 있습니다.
/// 런타임에 동적으로 카메라 채널을 등록/해제할 수 있는 기능을 제공합니다.
/// </summary>
public class CameraManager : MonoBehaviour, 
    IEventListener<string>, 
    IEventListener<PlayerController>, 
    IEventListener<CameraRegistrationData>,
    IDisposable
{
    [SerializeField] private CinemachineBrain _cinemachineBrain;
    [SerializeField] private CameraController _autoCameraController;
    [SerializeField] private List<CameraChannel> _channelList = new List<CameraChannel>();

    // 등록된 모든 채널을 관리하는 맵 (런타임 동적 등록 포함)
    private Dictionary<string, CinemachineCamera> _channelMap = new Dictionary<string, CinemachineCamera>();

    private CinemachineCamera _activeManualCamera;
    private string _currentChannelName;
    private PlayerController _player;

    [Header("Event Channels")]
    [SerializeField] private OnCameraChangeSO _onCameraChangeSO;
    [SerializeField] private OnPlayerSpawnedSO _onPlayerSpawnedSO;
    [SerializeField] private CameraRegistrationEventSO _onCameraRegistrationSO;

    private void Awake()
    {
        InitializeStaticChannels();
    }

    private void OnEnable()
    {
        _onCameraChangeSO.Subscribe(this);
        _onPlayerSpawnedSO.Subscribe(this);
        _onCameraRegistrationSO.Subscribe(this);

        // 기본적으로 자동 컨트롤러 활성화
        EnableAutoCamera(true);
    }

    private void OnDisable()
    {
        _onCameraChangeSO.Unsubscribe(this);
        _onPlayerSpawnedSO.Unsubscribe(this);
        _onCameraRegistrationSO.Unsubscribe(this);
    }

    public void Dispose()
    {
        _player = null;
        _channelMap.Clear();
    }

    /// <summary>
    /// 인스펙터에 등록된 정적 채널들을 맵에 초기화합니다.
    /// </summary>
    private void InitializeStaticChannels()
    {
        _channelMap.Clear();
        foreach (var channel in _channelList)
        {
            if (channel != null && !string.IsNullOrEmpty(channel.ChannelName) && channel.Camera != null)
            {
                _channelMap[channel.ChannelName] = channel.Camera;
                channel.Camera.Priority = 0;
            }
        }
    }

    /// <summary>
    /// 자동 카메라 모드 활성화/비활성화
    /// </summary>
    private void EnableAutoCamera(bool enable)
    {
        if (_autoCameraController != null)
        {
            _autoCameraController.enabled = enable;
            if (enable)
            {
                // 수동 채널 해제 시 우선순위 초기화
                if (_activeManualCamera != null)
                {
                    _activeManualCamera.Priority = 0;
                }
                _activeManualCamera = null;
                _currentChannelName = null;
                _autoCameraController.UpdateCameraPriorities();
            }
        }
    }

    #region CameraChannel Control
    
    public void ChangeChannel(string cameraChannelName)
    {
        // "Default" 또는 "Auto" 요청 시 자동 모드로 복귀
        if (cameraChannelName == "Default" || cameraChannelName == "Auto")
        {
            EnableAutoCamera(true);
            return;
        }

        if (_channelMap.TryGetValue(cameraChannelName, out var targetCamera))
        {
            ExecuteChannelChange(cameraChannelName, targetCamera);
        }
        else
        {
            Debug.LogWarning($"[CameraManager] Channel '{cameraChannelName}' is not registered.");
        }
    }

    private void ExecuteChannelChange(string channelName, CinemachineCamera camera)
    {
        // 수동 채널 변경 시 자동 모드 비활성화
        EnableAutoCamera(false);

        // 기존 수동 채널 우선순위 낮춤
        if (_activeManualCamera != null)
        {
            _activeManualCamera.Priority = 0;
        }

        // 새 채널 우선순위 높임 (CameraController보다 높은 값)
        camera.Priority = 50; 
        _activeManualCamera = camera;
        _currentChannelName = channelName;
    }

    #endregion

    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 채널 변경 이벤트 처리
    /// </summary>
    public void OnEventTrigger(string cameraChannel)
    {
        ChangeChannel(cameraChannel);
    }

    /// <summary>
    /// 플레이어 스폰 이벤트 처리 및 컨트롤러 초기화
    /// </summary>
    public void OnEventTrigger(PlayerController player)
    {
        if (_player == null)
        {
            _player = player;

            if (_autoCameraController != null)
            {
                _autoCameraController.Setup(_player);
            }

            player.RegisterDisposable(this);
        }
    }

    /// <summary>
    /// 카메라 채널 동적 등록/해제 이벤트 처리
    /// </summary>
    public void OnEventTrigger(CameraRegistrationData data)
    {
        if (data.isRegister)
        {
            if (data.camera != null && !string.IsNullOrEmpty(data.channelName))
            {
                _channelMap[data.channelName] = data.camera;
                data.camera.Priority = 0;
                // Debug.Log($"[CameraManager] Registered channel: {data.channelName}");
            }
        }
        else
        {
            if (_channelMap.ContainsKey(data.channelName))
            {
                // 현재 사용 중인 채널이 해제되면 자동 모드로 복구
                if (_currentChannelName == data.channelName)
                {
                    EnableAutoCamera(true);
                }
                _channelMap.Remove(data.channelName);
                // Debug.Log($"[CameraManager] Unregistered channel: {data.channelName}");
            }
        }
    }
}
