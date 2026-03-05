using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _speackerNameText;
    [SerializeField] private TMP_Text _dialogueText;

    private void Start()
    {
        DialogueManager.Instance.DialogueStarted += OnDialogueStart;
        DialogueManager.Instance.DialogueUpdated += OnDialogueUpdated;
        DialogueManager.Instance.DialogueCompleted += OnDialogueCompleted;
    }

    private void OnDestroy()
    {
        DialogueManager.Instance.DialogueStarted -= OnDialogueStart;
        DialogueManager.Instance.DialogueUpdated -= OnDialogueUpdated;
        DialogueManager.Instance.DialogueCompleted -= OnDialogueCompleted;
    }

    private void OnDialogueStart()
    {
        _panel.SetActive(true);
    }

    private void OnDialogueUpdated(DialogueDataSO data)
    {
        _speackerNameText.text = data.SpeakerName;
        _dialogueText.text = data.DialogueText;
    }

    private void OnDialogueCompleted()
    {
        _speackerNameText.text = "";
        _dialogueText.text = "";
        _panel.SetActive(false);
    }
}


