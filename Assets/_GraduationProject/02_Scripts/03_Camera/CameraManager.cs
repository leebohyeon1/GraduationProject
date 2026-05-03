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
/// </summary>
public class CameraManager : MonoBehaviour, IEventListener<string>, IEventListener<PlayerController>, IDisposable
{
    [SerializeField] private CinemachineBrain _cinemachineBrain;
    [SerializeField] private CameraController _autoCameraController; // 자동 전환 컨트롤러 추가
    [SerializeField] private List<CameraChannel> _channelList = new List<CameraChannel>();

    private CameraChannel _currentChannel;
    private PlayerController _player;

    [SerializeField] private OnCameraChangeSO _onCameraChangeSO;
    [SerializeField] private OnPlayerSpawnedSO _onPlayerSpawnedSO;

    private void OnEnable()
    {
        _onCameraChangeSO.Subscribe(this);
        _onPlayerSpawnedSO.Subscribe(this);

        // 초기화: 모든 채널 카메라 우선순위 낮춤
        foreach (var channel in _channelList)
        {
            channel.Camera.Priority = 0;
        }

        // 기본적으로 자동 컨트롤러 활성화
        EnableAutoCamera(true);
    }

    private void OnDisable()
    {
        _onCameraChangeSO.Unsubscribe(this);
        _onPlayerSpawnedSO.Unsubscribe(this);
    }

    public void Dispose()
    {
        _player = null;
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
                _currentChannel = null; // 수동 채널 해제
                _autoCameraController.UpdateCameraPriorities();
            }
        }
    }

    #region CameraChannel
    
    public void ChangeChannel(string cameraChannelName)
    {
        // "Default" 또는 "Auto" 요청 시 자동 모드로 복귀
        if (cameraChannelName == "Default" || cameraChannelName == "Auto")
        {
            EnableAutoCamera(true);
            return;
        }

        foreach (var channel in _channelList)
        {
            if (channel.ChannelName == cameraChannelName)
            {
                ChangeChannel(channel);
                break;
            }
        }
    }

    public void ChangeChannel(CameraChannel cameraChannel)
    {
        // 수동 채널 변경 시 자동 모드 비활성화
        EnableAutoCamera(false);

        // 기존 채널 우선순위 낮춤
        if (_currentChannel != null)
        {
            _currentChannel.Camera.Priority = 0;
        }

        // 새 채널 우선순위 높임 (CameraController보다 높은 값 권장)
        cameraChannel.Camera.Priority = 50; 
        _currentChannel = cameraChannel;
    }

    public CameraChannel GetCameraChannel(string cameraChannelName)
    {
        foreach (var channel in _channelList)
        {
            if (channel.ChannelName == cameraChannelName)
                return channel;
        }
        return null;
    }

    #endregion

    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================

    public void OnEventTrigger(string cameraChannel)
    {
        ChangeChannel(cameraChannel);
    }

    public void OnEventTrigger(PlayerController player)
    {
        if (_player == null)
        {
            _player = player;

            // CameraController 초기 설정
            if (_autoCameraController != null)
            {
                _autoCameraController.Setup(_player);
            }

            player.RegisterDisposable(this);
        }
    }
}
