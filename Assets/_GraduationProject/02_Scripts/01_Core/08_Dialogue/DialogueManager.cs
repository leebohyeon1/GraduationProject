using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대화를 관리하는 매니저
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    [SerializeField] private InputReaderSO _inputReader;

    private DialogueDataSO _currentDialogue; 
    private int _currentDialogueIndex = -1;

    public event Action DialogueStarted, DialogueCompleted;
    public event Action<DialogueDataSO.DialogueData> DialogueUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        DialogueStarted = null;
        DialogueCompleted = null;   
        DialogueUpdated = null;
    }

    public void StartDialogue(int groupID)
    {
        if (_currentDialogue != null && _currentDialogue.DialogueList.Count > 1)
        {
            return;
        }

        _inputReader.SubmitEvent += OnSubmit;


        _inputReader.SetInputMode(InputReaderSO.InputMode.UI);
        _currentDialogue = DataManager.Instance.GetDialogueGroupData(groupID);
        _currentDialogueIndex = 0;
        DialogueUpdated?.Invoke(_currentDialogue.DialogueList[_currentDialogueIndex]);

        DialogueStarted?.Invoke();
    }

    private void EndDialogue()
    {
        DataManager.Instance.GetGameData().CompleteDialogueSet.Add(_currentDialogue.DialogueGroupID);

        _inputReader.SubmitEvent -= OnSubmit;
        _inputReader.SetInputMode(InputReaderSO.InputMode.Gameplay);

        _currentDialogue = null;
        DialogueCompleted?.Invoke();
    }

    private void NextDialogue()
    {
        _currentDialogueIndex++;

        if(_currentDialogueIndex >= _currentDialogue.DialogueList.Count)
        {
            EndDialogue();
        }
        else
        {
            DialogueUpdated?.Invoke(_currentDialogue.DialogueList[_currentDialogueIndex]);
        }
    }


    private void OnSubmit()
    {
        NextDialogue();
    }

}
