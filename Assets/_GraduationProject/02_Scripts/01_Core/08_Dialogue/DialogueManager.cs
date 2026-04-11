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
    [SerializeField] private AudioSource _audioSource;

    private DialogueDataSO _currentDialogue; 
    private int _currentDialogueIndex = -1;
    private Coroutine _autoNextCoroutine;

    public event Action DialogueStarted, DialogueCompleted;
    public event Action<DialogueDataSO.DialogueData> DialogueUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
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
        if (_currentDialogue != null)
        {
            EndDialogue();
        }

        _currentDialogue = DataManager.Instance.GetDialogueGroupData(groupID);

        if (_currentDialogue.NeedConditionList.Count == 0)
        {
            Debug.LogWarning("대화 그룹 " + groupID + "의 필요 조건이 없습니다. 대화가 시작되지 않습니다.");
            return;
        }

        if (_currentDialogue.DialogueType == DialogueType.Dialogue)
        {
            _inputReader.SubmitEvent += OnSubmit;
            _inputReader.SetInputMode(InputReaderSO.InputMode.UI);
        }
        else if (_currentDialogue.DialogueType == DialogueType.Narration)
        {
            _inputReader.SubmitEvent += OnSubmit;
        }

        _currentDialogueIndex = 0;
        UpdateDialogue();

        DialogueStarted?.Invoke();
    }

    private void EndDialogue()
    {
        StopAutoNext();

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
        StopAutoNext();
        _currentDialogueIndex++;

        if(_currentDialogueIndex >= _currentDialogue.DialogueList.Count)
        {
            EndDialogue();
        }
        else
        {
            UpdateDialogue();
        }
    }

    private void UpdateDialogue()
    {
        var data = _currentDialogue.DialogueList[_currentDialogueIndex];
        DialogueUpdated?.Invoke(data);

        if (_currentDialogue.DialogueType == DialogueType.Narration)
        {
            StartAutoNext(data);
        }
    }

    private void StartAutoNext(DialogueDataSO.DialogueData data)
    {
        StopAutoNext();
        _autoNextCoroutine = StartCoroutine(AutoNextProcess(data));
    }

    private void StopAutoNext()
    {
        if (_autoNextCoroutine != null)
        {
            StopCoroutine(_autoNextCoroutine);
            _autoNextCoroutine = null;
        }
        
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    private System.Collections.IEnumerator AutoNextProcess(DialogueDataSO.DialogueData data)
    {
        if (data.Sound != null && _audioSource != null)
        {
            _audioSource.clip = data.Sound;
            _audioSource.Play();
            yield return new WaitWhile(() => _audioSource.isPlaying);
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        NextDialogue();
    }

    private void OnSubmit()
    {
        NextDialogue();
    }
}
