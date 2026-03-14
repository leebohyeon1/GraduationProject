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

    private void Start()
    {
        if (GamePlayTagManager.Instance != null)
        {
            GamePlayTagManager.Instance.UpdateTag += OnUpdateTag;
        }
    }

    private void OnDestroy()
    {
        if (GamePlayTagManager.Instance != null)
        {
            GamePlayTagManager.Instance.UpdateTag -= OnUpdateTag;
        }
    }

    private void OnUpdateTag(GamePlayTagSO tag)
    {
        CheckNarrationDialogue();
    }

    private void CheckNarrationDialogue()
    {
        if (_currentDialogue != null) return;

        var database = DataManager.Instance.DialogueDatabase;
        if (database == null) return;

        foreach (var dialogue in database.DialogueDataList)
        {
            if (dialogue.DialogueType != DialogueType.Narration) continue;
            if (DataManager.Instance.GetGameData().CompleteDialogueSet.Contains(dialogue.DialogueGroupID)) continue;

            bool allConditionsMet = true;
            foreach (var condition in dialogue.NeedConditionList)
            {
                if (!GamePlayTagManager.Instance.HasTag(condition))
                {
                    allConditionsMet = false;
                    break;
                }
            }

            if (allConditionsMet)
            {
                StartDialogue(dialogue.DialogueGroupID);
                break;
            }
        }
    }

    public void StartDialogue(int groupID)
    {
        if (_currentDialogue != null && _currentDialogue.DialogueList.Count > 1)
        {
            return;
        }

        _currentDialogue = DataManager.Instance.GetDialogueGroupData(groupID);

        if (_currentDialogue.DialogueType == DialogueType.Dialogue)
        {
            _inputReader.SubmitEvent += OnSubmit;
            _inputReader.SetInputMode(InputReaderSO.InputMode.UI);
        }
        else if (_currentDialogue.DialogueType == DialogueType.Narration)
        {
            // 나레이션은 별도의 입력 없이 일정 시간 후 넘어가거나 할 수 있지만, 
            // 현재 구조상 Submit으로 넘기는 기능을 유지하려면 Gameplay 모드에서도 Submit 이벤트를 받아야 함.
            // 일단 Gameplay 모드를 유지하면서 Submit 이벤트를 연결함.
            _inputReader.SubmitEvent += OnSubmit;
        }

        _currentDialogueIndex = 0;
        DialogueUpdated?.Invoke(_currentDialogue.DialogueList[_currentDialogueIndex]);

        DialogueStarted?.Invoke();
    }

    private void EndDialogue()
    {
        DataManager.Instance.GetGameData().CompleteDialogueSet.Add(_currentDialogue.DialogueGroupID);

        _inputReader.SubmitEvent -= OnSubmit;

        if (_currentDialogue.DialogueType == DialogueType.Dialogue)
        {
            _inputReader.SetInputMode(InputReaderSO.InputMode.Gameplay);
        }

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
