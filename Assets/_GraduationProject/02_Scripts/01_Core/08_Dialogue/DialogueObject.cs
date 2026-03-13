using System.Collections.Generic;
using UnityEngine;

public class DialogueObject : MonoBehaviour, IInteractable
{
    private PlayerController _playerController;

    [SerializeField] private Transform _interactableUITransform;
    public Transform InteractableUITransform => _interactableUITransform;

    [SerializeField] private InteractableType _interactableType;
    public InteractableType InteractableType => _interactableType;

    [SerializeField] private List<int> _dialogueGroupIDList;

    public void Interact()
    {
        for (int i = 0; i < _dialogueGroupIDList.Count; i++)
        {
            if (CheckConditions(_dialogueGroupIDList[i]))
            {
                DialogueManager.Instance.StartDialogue(_dialogueGroupIDList[i]);
                return;
            }
        }
    }

    private bool CheckConditions(int id)
    {
        DialogueDataSO initialDialogue = DataManager.Instance.GetDialogueGroupData(id)[0];

        for (int i = 0; i < initialDialogue.NeedConditionList.Count; i++)
        {
            if (!GamePlayTagManager.Instance.HasTag(initialDialogue.NeedConditionList[i]))
            {
                return false;
            }
        }

        return true;
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
