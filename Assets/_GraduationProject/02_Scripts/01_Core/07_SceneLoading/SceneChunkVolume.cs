using UnityEngine;

// 이 스크립트는 해당 씬 안에 배치되어, 플레이어가 이 구역 안에 들어왔음을 감지합니다.
public class SceneChunkVolume : MonoBehaviour
{
    [Header("현재 구역의 씬 데이터")]
    public SceneDataSO thisChunkData;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 이 구역 안으로 깊숙이 들어왔다면!
        if (other.TryGetComponent<PlayerController>(out var component))
        {
            // 매니저에게 "이제부터 여기가 메인(Active) 구역이야!" 라고 알려줍니다.
            SceneLoadingManager.Instance.SetActiveChunk(thisChunkData);
        }
    }
}