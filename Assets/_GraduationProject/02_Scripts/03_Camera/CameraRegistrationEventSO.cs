using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 런타임 카메라 등록/해제를 위한 데이터 구조입니다.
/// </summary>
public struct CameraRegistrationData
{
    public string channelName;
    public CinemachineCamera camera;
    public bool isRegister; // true: 등록, false: 해제
}

/// <summary>
/// 카메라 채널을 동적으로 등록하거나 해제하기 위한 이벤트 채널입니다.
/// </summary>
[CreateAssetMenu(fileName = "OnCameraRegistration", menuName = "Project/Events/CameraRegistrationEventSO")]
public class CameraRegistrationEventSO : EventSO<CameraRegistrationData>
{
}
