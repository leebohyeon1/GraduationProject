using MoreMountains.Feedbacks;
using UnityEngine;

public class ElevatorTrigger : MonoBehaviour, IInteractable
{
    private PlayerController _playerController;
    [SerializeField] private Elevator _elevator;

    [SerializeField] private Transform _interactableUITransform;
    [SerializeField] private InteractableType _interactableType;

    public Transform InteractableUITransform => _interactableUITransform;

    public InteractableType InteractableType => _interactableType;

    public void Interact()
    {
        _elevator.Move();
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
            if (_playerController.Interact != null && _playerController.Interact.Interactable.Equals(this))
            {
                _playerController.Interact.SetInteractable(null);
            }
        }
    }
}
