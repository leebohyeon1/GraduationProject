using UnityEngine;

/// <summary>
/// 플레이어 상호작용을 통해 다른 씬으로 이동시키는 컴포넌트입니다.
/// 리스폰 방식(기본, 마지막 위치, 커스텀)을 선택할 수 있습니다.
/// InteractableObject와 함께 사용해야 합니다.
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class SceneTransitionInteractor : MonoBehaviour
{
    public enum SpawnType { Default, LastPosition, Custom }

    [Header("이동할 씬 정보")]
    [SerializeField] private SceneDataSO _targetSceneData; 

    [Header("스폰 설정")]
    [SerializeField] private SpawnType _spawnType = SpawnType.Default;

    [Header("커스텀 좌표 (Custom 선택 시에만 사용)")]
    [SerializeField] private Vector3 _customPosition;
    
    private InteractableObject _interactable;

    private void Awake()
    {
        _interactable = GetComponent<InteractableObject>();
    }

    private void OnEnable()
    {
        if (_interactable != null)
        {
            _interactable.OnInteract.AddListener(TransitionToScene);
        }
    }

    private void OnDisable()
    {
        if (_interactable != null)
        {
            _interactable.OnInteract.RemoveListener(TransitionToScene);
        }
    }

    private void TransitionToScene()
    {
        if (_targetSceneData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 이동할 SceneDataSO가 할당되지 않았습니다!");
            return;
        }

        if (SceneLoadingManager.Instance != null)
        {
            switch (_spawnType)
            {
                case SpawnType.Custom:
                    Debug.Log($"상호작용: '{_targetSceneData.SceneName}' 씬의 커스텀 좌표({_customPosition})로 이동.");
                    SceneLoadingManager.Instance.TeleportToScene(_targetSceneData, _customPosition);
                    break;

                case SpawnType.LastPosition:
                    Debug.Log($"상호작용: '{_targetSceneData.SceneName}' 씬의 마지막 저장 위치로 이동.");
                    SceneLoadingManager.Instance.TeleportToScene(_targetSceneData, SceneLoadingManager.SpawnMode.LastPosition);
                    break;

                case SpawnType.Default:
                default:
                    Debug.Log($"상호작용: '{_targetSceneData.SceneName}' 씬의 기본 위치로 이동.");
                    SceneLoadingManager.Instance.TeleportToScene(_targetSceneData, SceneLoadingManager.SpawnMode.Default);
                    break;
            }
        }
        else
        {
            Debug.LogError("SceneLoadingManager.Instance를 찾을 수 없습니다.");
        }
    }
}
