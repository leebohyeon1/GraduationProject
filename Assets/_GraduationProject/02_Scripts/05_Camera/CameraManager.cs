using BH_Lib.Log;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour, IEventListener<Transform>
{
    [SerializeField] private CinemachineCamera _playerFollowCamera; // 플레이어 추적 카메라
    [SerializeField] private CinemachineTargetGroup _lockOnTargetGroup; // 락온 목표 그룹

    [SerializeField] private OnCameraInitializeSO _onCameraInitializeSO; // 카메라 초기화 이벤트



    private List<Transform> _targetList = new List<Transform>();

    private void OnEnable()
    {
        _onCameraInitializeSO.Subscribe(this);
    }
        
    private void OnDisable()
    {
        _onCameraInitializeSO.Unsubscribe(this);
    }


    public void OnEventTrigger(Transform value)
    {
        Log.Print($"CameraManager - OnEventTrigger: {value.name}"); 
        _targetList.Add(value);

        if (_playerFollowCamera.Target.TrackingTarget == null)
        {
            _playerFollowCamera.Target.TrackingTarget = value;
        }

        _lockOnTargetGroup.AddMember(value, 1f, 2f);
    }
}
