using BH_Lib.Log;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour, IEventListener<Transform>, IEventListener<bool>
{
    [SerializeField] private CinemachineBrain _cinemachineBrain; // 시네머신 브레인    
    [SerializeField] private CinemachineCamera _playerFollowCamera; // 플레이어 추적 카메라
    [SerializeField] private CinemachineTargetGroup _lockOnTargetGroup; // 락온 목표 그룹

    [SerializeField] private OnCameraInitializeSO _onCameraInitializeSO; // 카메라 초기화 이벤트
    [SerializeField] private OnLockOnSO _onLockOnSO; // 락온 이벤트


    private List<Transform> _targetList = new List<Transform>();

    private void OnEnable()
    {
        _onCameraInitializeSO.Subscribe(this);
        _onLockOnSO.Subscribe(this);
    }
        
    private void OnDisable()
    {
        _onCameraInitializeSO.Unsubscribe(this);
        _onLockOnSO.Unsubscribe(this);
    }


    public void OnEventTrigger(Transform value)
    {
        _targetList.Add(value);

        if (_playerFollowCamera.Target.TrackingTarget == null)
        {
            _playerFollowCamera.Target.TrackingTarget = value;
        }

        _lockOnTargetGroup.AddMember(value, 1f, 2f);
    }

    public void OnEventTrigger(bool value)
    {
        if(value)
        {
            // 락온 모드
            _cinemachineBrain.ChannelMask = OutputChannels.Channel01;
            return;
        }
        else
        {
            _cinemachineBrain.ChannelMask = OutputChannels.Default;
        }
    }
}
