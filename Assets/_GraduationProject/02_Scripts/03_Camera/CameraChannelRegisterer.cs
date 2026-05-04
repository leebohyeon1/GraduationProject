using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 이 컴포넌트가 부착된 오브젝트의 Cinemachine 가상 카메라를 
/// CameraManager의 채널 목록에 자동으로 등록하고 해제합니다.
/// </summary>
[RequireComponent(typeof(CinemachineCamera))]
public class CameraChannelRegisterer : MonoBehaviour
{
    [Header("Registration Settings")]
    [Tooltip("CameraManager에서 사용할 채널 이름입니다.")]
    [SerializeField] private string _channelName;
    [SerializeField] private CinemachineCamera _camera;

    [Header("Event Channels")]
    [SerializeField] private CameraRegistrationEventSO _onCameraRegistrationSO;



    private void Awake()
    {
        if (_camera == null)
        {
            _camera = GetComponent<CinemachineCamera>();
        }
    }

    private void OnEnable()
    {
        Register(true);
    }

    private void OnDisable()
    {
        Register(false);
    }

    /// <summary>
    /// 이벤트를 통해 카메라 등록 또는 해제 정보를 전송합니다.
    /// </summary>
    private void Register(bool isRegister)
    {
        if (_onCameraRegistrationSO == null || string.IsNullOrEmpty(_channelName) || _camera == null)
        {
            return;
        }

        CameraRegistrationData data = new CameraRegistrationData
        {
            channelName = _channelName,
            camera = _camera,
            isRegister = isRegister
        };

        // 초기화 대기를 위해 약간의 지연을 두고 이벤트 발행
        _onCameraRegistrationSO.Publish(data, 1f);
    }

    /// <summary>
    /// 에디터에서 컴포넌트 추가 시 오브젝트 이름을 기본 채널명으로 설정합니다.
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_channelName))
        {
            _channelName = gameObject.name;
        }
    }
}
