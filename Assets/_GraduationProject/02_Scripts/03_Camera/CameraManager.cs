using BH_Lib.Log;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraTarget
{
    public int Priority = 0;
    public Transform Transform { get; private set; }
    public float Radius { get; private set; }
    public float Weight { get; private set; }

    public CameraTarget(int priority, Transform transform, float radius = 2, float weight = 0.5f)
    {
        Priority = priority;
        Transform = transform;
        Radius = radius;
        Weight = weight;
    }
}

public class CameraManager : MonoBehaviour, IEventListener<CameraTarget>, IEventListener<bool>
{
    [SerializeField] private CinemachineBrain _cinemachineBrain; // 시네머신 브레인    
    [SerializeField] private CinemachineCamera _playerFollowCamera; // 플레이어 추적 카메라
    [SerializeField] private CinemachineTargetGroup _lockOnTargetGroup; // 락온 목표 그룹

    [SerializeField] private OnCameraInitializeSO _onCameraInitializeSO; // 카메라 초기화 이벤트
    [SerializeField] private OnLockOnSO _onLockOnSO; // 락온 이벤트


    private List<CameraTarget> _targetList = new List<CameraTarget>();

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


    public void OnEventTrigger(CameraTarget value)
    {
        _targetList.Add(value);

        _targetList.Sort(new PriorityComparer());

        _lockOnTargetGroup.Targets.Clear();
        foreach (var target in _targetList)
        {
            _lockOnTargetGroup.AddMember(target.Transform, target.Weight, target.Radius);

            if (target.Transform.CompareTag("Player"))
            {
                _playerFollowCamera.Target.TrackingTarget = target.Transform;
            }
        }

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

public class PriorityComparer : IComparer<CameraTarget>
{
    public int Compare(CameraTarget x, CameraTarget y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        return x.Priority.CompareTo(y.Priority); // 내림차순
    }
}
