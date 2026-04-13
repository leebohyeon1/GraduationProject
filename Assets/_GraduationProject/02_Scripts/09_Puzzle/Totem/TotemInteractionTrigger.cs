using System.Collections.Generic;
using UnityEngine;

public class TotemInteractionTrigger : MonoBehaviour, IInteractable
{
    private static readonly List<TotemDissolveTarget> CachedTargets = new List<TotemDissolveTarget>(8);

    [Header("Interactable")]
    [SerializeField] private Transform _interactableUITransform;
    [SerializeField] private InteractableType _interactableType = InteractableType.Environment;

    [Header("Trigger")]
    [SerializeField] private string _targetId;
    [SerializeField] private bool _oneShot = true;
    [SerializeField] private bool _allowInteractWithoutTargets = false;

    [Header("Feedback")]
    [SerializeField] private TotemGimmickFeedbackPlayer _feedbackPlayer;

    private PlayerController _playerController;
    private bool _isUsed;

    public Transform InteractableUITransform => _interactableUITransform;
    public InteractableType InteractableType => _interactableType;

    public void Interact()
    {
        if (_isUsed && _oneShot)
        {
            Debug.Log($"[TotemInteractionTrigger] OneShot already used. object={name}");
            return;
        }

        int targetCount = TotemDissolveTarget.CollectTargets(_targetId, CachedTargets);
        if (targetCount == 0 && !_allowInteractWithoutTargets)
        {
            Debug.Log($"[TotemInteractionTrigger] No target found. targetId={_targetId}, object={name}");
            return;
        }

        Debug.Log($"[TotemInteractionTrigger] Interact success. targetCount={targetCount}, targetId={_targetId}, object={name}");
        _feedbackPlayer?.PlayFeedback(TotemGimmickFeedbackType.Interact, transform.position);

        for (int i = 0; i < CachedTargets.Count; i++)
        {
            CachedTargets[i].BeginDissolve();
        }

        if (_oneShot)
        {
            _isUsed = true;
            if (_playerController != null && _playerController.Interact != null && _playerController.Interact.Interactable == this)
            {
                _playerController.Interact.SetInteractable(null);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerController controller))
        {
            return;
        }

        _playerController = controller;
        if (_playerController.Interact != null)
        {
            _playerController.Interact.SetInteractable(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerController controller) || controller != _playerController)
        {
            return;
        }

        if (_playerController.Interact != null && _playerController.Interact.Interactable == this)
        {
            _playerController.Interact.SetInteractable(null);
        }

        _playerController = null;
    }
}
