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
/// 카메라를 관리하는 매니저
/// </summary>
public class CameraManager : MonoBehaviour, IEventListener<string>, IEventListener<PlayerController>, IDisposable
{
    [SerializeField] private CinemachineBrain _cinemachineBrain; // 시네머신 브레인    
    [SerializeField] private List<CameraChannel> _channelList = new List<CameraChannel>();

    private CameraChannel _currentChannel;          // 현재 카메라
    private PlayerController _player;

    [SerializeField] private OnCameraChangeSO _onCameraChangeSO;
    [SerializeField] private OnPlayerSpawnedSO _onPlayerSpawnedSO;

    private void OnEnable()
    {
        _onCameraChangeSO.Subscribe(this);
        _onPlayerSpawnedSO.Subscribe(this);

        // 모든 카메라 Off
        foreach (var channel in _channelList)
        {
            channel.Camera.gameObject.SetActive(false);
        }

        // 기본 카메라를 현재 카메라로 설정
        ChangeChannel(GetCameraChannel("DefaultCamera"));
    }

    private void OnDisable()
    {
        _onCameraChangeSO.Unsubscribe(this);
        _onPlayerSpawnedSO.Unsubscribe(this);
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public void Dispose()
    {
        if (_player != null)
        {
            _player.LockOn.LockOnEvent -= OnLockOnEvent;

            _player = null;
        }
    }

    #region CameraChannel
    /// <summary>
    /// 카메라 채널 바꾸기
    /// </summary>
    /// <param name="cameraChannelName">바꾸려는 카메라 채널 이름</param>
    public void ChangeChannel(string cameraChannelName)
    {
        foreach (var channel in _channelList)
        {
            // 채널 명이 같으면
            if(channel.ChannelName == cameraChannelName)
            {
                ChangeChannel(channel); // 채널 변경
                break;
            }
        }
    }

    /// <summary>
    /// 카메라 채널 바꾸기
    /// </summary>
    /// <param name="cameraChannel">바꿀 카메라 채널</param>
    public void ChangeChannel(CameraChannel cameraChannel)
    {
        // 카메라 우선순위 변경
        cameraChannel.Camera.gameObject.SetActive(true);
        if(_currentChannel != null)
        {
            _currentChannel.Camera.gameObject.SetActive(false);
        }

        // 현재 카메라 교체
        _currentChannel = cameraChannel;
    }

    /// <summary>
    /// 카메라 반환 함수
    /// </summary>
    /// <param name="cameraChannelName">카메라 채널 이름</param>
    /// <returns>찾은 카메라 채널</returns>
    public CameraChannel GetCameraChannel(string cameraChannelName)
    {
        foreach (var channel in _channelList)
        {
            // 채널 명이 같으면
            if (channel.ChannelName == cameraChannelName)
            {
                return channel;
            }
        }

        return null;
    }

    #endregion

    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// string 이벤트 처리
    /// </summary>
    /// <param name="cameraChannel">카메라 채널 명</param>
    public void OnEventTrigger(string cameraChannel)
    {
        ChangeChannel(cameraChannel);
    }

    /// <summary>
    /// 플레이어 스폰 이벤트 처리
    /// </summary>
    /// <param name="player">플레이어</param>
    public void OnEventTrigger(PlayerController player)
    {
        if(_player == null)
        {
            _player = player;

            _player.LockOn.LockOnEvent += OnLockOnEvent;

            // 기본 카메라 타겟 설정
            GetCameraChannel("DefaultCamera").Camera.Target.TrackingTarget = _player.transform;

            // 락온 카메라 타겟 설정
            Transform targetGroup = GetCameraChannel("LockOnCamera").Camera.Target.TrackingTarget;
            if(targetGroup.TryGetComponent<CinemachineTargetGroup>(out CinemachineTargetGroup var))
            {
                var.AddMember(_player.transform, 0.5f, 1f);
                var.AddMember(_player.LockOn.LockOnIndicator.transform, 0.5f, 1f);
            }

            player.RegisterDisposable(this);
        }
    }

    /// <summary>
    /// 락온 이벤트 처리
    /// </summary>
    /// <param name="islockOn">락온 여부</param>
    private void OnLockOnEvent(bool islockOn)
    {
        if(islockOn)
        {
            ChangeChannel("LockOnCamera");
        }
        else
        {
            ChangeChannel("DefaultCamera");
        }
    }

}
