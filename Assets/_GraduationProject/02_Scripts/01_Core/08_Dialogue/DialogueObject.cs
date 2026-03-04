using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

[Serializable]
public class DialogueItem
{
    public List<GamePlayTagSO> Conditions;
    public int DialogueGroupID;

    public bool CheckCondition()
    {
        for(int i=0; i<Conditions.Count; i++)
        {
            if (!GamePlayTagManager.Instance.HasTag(Conditions[i]))
            {
                return false;
            }
        }

        DialogueDataSO initialDialogue = DataManager.Instance.GetDialogueGroupData(DialogueGroupID)[0];

        int id;
        for(int i = 0; i < initialDialogue.NeedDialogueIDList.Count; i++)
        {
            id = initialDialogue.NeedDialogueIDList[i];
            if (!DataManager.Instance.GetGameData().CompleteDialogueSet.Contains(id))
            {
                return false;
            }
        }

        return true;
    }
}

public class DialogueObject : MonoBehaviour, IInteractable
{
    private PlayerController _playerController;

    [SerializeField] private Transform _interactableUITransform;
    public Transform InteractableUITransform => _interactableUITransform;

    [SerializeField] private InteractableType _interactableType;
    public InteractableType InteractableType => _interactableType;

    [SerializeField] private DialogueItem _defaultDialogueData;
    [SerializeField] private List<DialogueItem> _dialogueData;

    public void Interact()
    {
        for (int i = 0; i < _dialogueData.Count; i++)
        {
            if (_dialogueData[i].CheckCondition())
            {
                DialogueManager.Instance.StartDialogue(_dialogueData[i].DialogueGroupID);
                return;
            }
        }

        if (_defaultDialogueData != null)
        {
            DialogueManager.Instance.StartDialogue(_defaultDialogueData.DialogueGroupID);
        }
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
