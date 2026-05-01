using UnityEngine;

/// <summary>
/// 상호작용 시 플레이어를 특정 위치로 이동시키는 컴포넌트입니다.
/// InteractableObject의 OnInteract 이벤트에 연결하여 사용합니다.
/// </summary>
public class TeleportInteractor : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("플레이어가 이동할 목적지 Transform입니다.")]
    [SerializeField] private Transform _targetPoint; 
    
    [Tooltip("목적지로 이동할 때 플레이어의 회전값도 목적지와 맞출지 여부입니다.")]
    [SerializeField] private bool _copyRotation = true;

    /// <summary>
    /// 플레이어를 목적지로 이동시킵니다.
    /// InteractableObject 컴포넌트의 OnInteract() 이벤트에서 이 메서드를 호출하세요.
    /// </summary>
    public void Teleport()
    {
        if (_targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] TeleportInteractor: 목적지(Target Point)가 지정되지 않았습니다.");
            return;
        }

        // 현재 씬에서 PlayerController를 찾습니다.
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        
        if (player != null)
        {
            // CharacterController는 활성화된 상태에서 transform.position을 직접 수정하면 
            // 물리 연산 결과로 인해 위치가 다시 돌아가거나 떨리는 현상이 발생할 수 있습니다.
            CharacterController cc = player.GetComponent<CharacterController>();
            
            if (cc != null)
            {
                // 1. 캐릭터 컨트롤러 잠시 비활성화
                cc.enabled = false;

                // 2. 위치 및 회전 변경
                player.transform.position = _targetPoint.position;
                if (_copyRotation)
                {
                    player.transform.rotation = _targetPoint.rotation;
                }

                // 3. 캐릭터 컨트롤러 다시 활성화
                cc.enabled = true;

                Debug.Log($"[Teleport] 플레이어를 {_targetPoint.name} 위치로 이동시켰습니다.");
            }
            else
            {
                // CharacterController가 없는 경우 (일반적인 경우엔 발생하지 않음)
                player.transform.position = _targetPoint.position;
                if (_copyRotation)
                {
                    player.transform.rotation = _targetPoint.rotation;
                }
            }
        }
        else
        {
            Debug.LogError("[Teleport] 씬에서 PlayerController를 찾을 수 없습니다.");
        }
    }
}
