using System;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform _interactableUITransform;
    public Transform InteractableUITransform => _interactableUITransform;

    [SerializeField] private InteractableType _interactableType;
    public InteractableType InteractableType => _interactableType;

    [Header("상호작용 옵션")]
    [SerializeField] private bool _onlyOne = false; // 한 번만 상호작용 가능한지 여부
    private bool _hasInteracted = false;

    public UnityEvent OnInteract;
    private PlayerController _playerController;

    public void Interact()
    {
        // 1. 이미 상호작용했다면 리턴
        if (_onlyOne && _hasInteracted) return;

        // 2. 이벤트 실행
        OnInteract?.Invoke();
        _hasInteracted = true;

        // 3. 일회성인 경우 플레이어의 상호작용 대상을 즉시 해제하여 UI를 없앱니다.
        if (_onlyOne && _playerController != null)
        {
            if (_playerController.Interact != null && _playerController.Interact.Interactable == (IInteractable)this)
            {
                _playerController.Interact.SetInteractable(null);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 이미 상호작용이 끝난 일회성 오브젝트라면 무시
        if (_onlyOne && _hasInteracted) return;

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
