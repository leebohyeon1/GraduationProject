using UnityEngine;

public class ResetTotem : MonoBehaviour,IInteractable
{

    private PlayerController _playerController;
    [Header("Interactable")]
    public Transform InteractableUITransform => _interactableUITransform;
    public InteractableType InteractableType => _interactableType;
    [SerializeField] private Transform _interactableUITransform;
    [SerializeField] private InteractableType _interactableType = InteractableType.Environment;

    
    public void Interact()
    {
        PuzzleGridManager.Instance.ResetPuzzle();
        Debug.Log("[ResetTotem] Puzzle reset triggered.");
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
