using UnityEngine;
using Unity.Cinemachine;

public class CinemachineTrigger : MonoBehaviour
{
    [Header("활성화할 가상 카메라")]
    [Tooltip("플레이어가 이 구역에 진입했을 때 활성화할 시네머신 가상 카메라를 지정합니다.")]
    public CinemachineCamera targetCamera;

    [Header("플레이어 태그")]
    [Tooltip("카메라를 전환시킬 오브젝트의 태그를 지정합니다.")]
    public string playerTag = "Player";
    
    private int originalPriority;

    private void Awake()
    {
        if (targetCamera == null)
        {
            Debug.LogError("Target Camera가 할당되지 않았습니다!");
            this.enabled = false;
            return;
        }
        originalPriority = targetCamera.Priority;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();

            // --- 여기가 최종 수정된 부분입니다 --- //

            // 1. 현재 활성화된 카메라가 CinemachineCamera 타입인지 확인하고, 맞다면 concreteCamera 변수에 담습니다.
            if (brain != null && brain.ActiveVirtualCamera is CinemachineCamera concreteCamera)
            {
                // 2. 이제 concreteCamera 변수를 통해 Priority에 안전하게 접근할 수 있습니다.
                int currentActivePriority = concreteCamera.Priority;
                targetCamera.Priority = currentActivePriority + 1;
            }
            else
            {
                // 예외 상황 처리
                targetCamera.Priority = originalPriority + 10;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetCamera.Priority = originalPriority;
        }
    }
}