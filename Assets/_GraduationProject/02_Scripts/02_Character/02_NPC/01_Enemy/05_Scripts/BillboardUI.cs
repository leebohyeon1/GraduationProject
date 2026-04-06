using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class BillboardUI : MonoBehaviour
{
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mainCamera == null) return;

        // 체력바가 카메라와 완전히 평행하게(정면으로) 보이게 합니다.
        // Quad는 기본적으로 뒤집혀 보일 수 있으므로 forward를 카메라와 동일하게 맞춥니다.
        transform.forward = _mainCamera.transform.forward;
    }
}
