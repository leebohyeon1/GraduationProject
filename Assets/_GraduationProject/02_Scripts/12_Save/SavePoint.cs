using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
    private PlayerController _playerController;

    [SerializeField] private Transform _interactableUITransform;
    public Transform InteractableUITransform => _interactableUITransform;

    [SerializeField] private InteractableType _interactableType;
    public InteractableType InteractableType => _interactableType;

    public void Interact()
    {
        GameData gameData = DataManager.Instance.GetGameData();
        gameData.PlayerData.RespawnPostion = _playerController.transform.position;
        _playerController.Health.Heal((int)gameData.PlayerData.Health.Value);
        _playerController.Potion.ReloadPotion();

        // 죽은 몬스터 목록 초기화 (리스폰)
        gameData.ClearDeadMonsters();

        DataManager.Instance.SaveGame();
        SceneLoadingManager.Instance.TeleportToSceneByName(gameData.LastMainScene, SceneLoadingManager.SpawnMode.LastPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out _playerController))
        {
            if (_playerController.Interact != null)
            {
                _playerController.Interact.SetInteractable(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController controller) && controller == _playerController)
        {
            if (_playerController.Interact != null && _playerController.Interact.Interactable == (IInteractable)this)
            {
                _playerController.Interact.SetInteractable(null);
            }

            _playerController = null;
        }
    }
}
