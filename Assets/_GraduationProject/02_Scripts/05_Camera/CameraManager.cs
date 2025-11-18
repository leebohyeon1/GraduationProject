using UnityEngine;

public class CameraManager : MonoBehaviour, IEventListener<Transform>
{
    [SerializeField] private OnCameraInitializeSO _onCameraInitializeSO; // 카메라 초기화 이벤트

    private Transform _defaultTarget;

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
        _defaultTarget = value;
    }
}
