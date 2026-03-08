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
        _playerController.Health.Heal(gameData.PlayerData.MaxHealth);
        _playerController.Potion.ReloadPotion();

        DataManager.Instance.SaveGame();
        SceneLoadingManager.Instance.TeleportToSceneByName(gameData.LastMainScene);
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
