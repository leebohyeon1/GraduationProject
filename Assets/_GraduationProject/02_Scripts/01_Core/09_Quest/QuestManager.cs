using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-998)]
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set;  }
    public QuestData CurrentQuestData { get; private set; }

    public event Action<QuestData> QuestAccepted, QuestCompleted;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        DataManager.Instance.InitQuestEvent();
        GamePlayTagManager.Instance.UpdateTag += OnUpdateTag;

        // 데이터베이스에서 저장된 데이터 불러오기
        if (DataManager.Instance.GetGameData().CurrentQuestId != -1)
        {
            int id = DataManager.Instance.GetGameData().CurrentQuestId;
            AccpetedQuest(DataManager.Instance.GetQuestData(id));
        }
    }

    private void OnApplicationQuit()
    {
        if (GamePlayTagManager.Instance != null)
        {
            GamePlayTagManager.Instance.UpdateTag -= OnUpdateTag;
        }

        QuestAccepted = null;
        QuestCompleted = null;
    }

    public void AccpetedQuest(QuestData quest)
    {
        CurrentQuestData = quest;
        QuestAccepted?.Invoke(CurrentQuestData);
    }

    /// <summary>
    /// 퀘스트 상태를 초기화합니다. 새 게임 시작 시 호출됩니다.
    /// </summary>
    public void ResetQuest()
    {
        CurrentQuestData = null;
        Debug.Log("<color=yellow>[QuestManager]</color> 퀘스트 상태가 초기화되었습니다.");
    }

    public void CompleteQuest()
    {
        QuestCompleted?.Invoke(CurrentQuestData);

        QuestData nextQuest = DataManager.Instance.GetQuestData(CurrentQuestData.NextQuestID);

        if (nextQuest == null)
        {
            Debug.Log("다음 퀘스트가 존재하지 않습니다.");
        }
        else
        {
            AccpetedQuest(nextQuest);
        }
        
    }


    private void OnUpdateTag(GamePlayTagSO sO)
    {
        if (DataManager.Instance == null || DataManager.Instance.GetGameData() == null) return;

        HashSet<string> tagSet = DataManager.Instance.GetGameData().GamePlayTagIdSet;
        QuestDatabaseSO questDatabase = DataManager.Instance.QuestDatabase;

        if (questDatabase == null || questDatabase.QuestList == null)
        {
            Debug.LogWarning("QuestManager: QuestDatabase or QuestList is not assigned in DataManager.");
            return;
        }

        // 1. 현재 진행 중인 퀘스트가 있다면 완료 조건 체크
        if (CurrentQuestData != null)
        {
            bool isAllClearTagsCollected = true;
            foreach (var clearTag in CurrentQuestData.ClearConditionList)
            {
                if (!tagSet.Contains(clearTag.ID))
                {
                    isAllClearTagsCollected = false;
                    break;
                }
            }

            // 모든 클리어 태그를 모았다면 퀘스트 완료 처리
            if (isAllClearTagsCollected)
            {
                Debug.Log($"<color=green>[QuestManager]</color> 현재 퀘스트 완료: {CurrentQuestData.ID}");
                CompleteQuest();
                return; // 완료 처리 후 다음 퀘스트는 CompleteQuest 내부에서 처리되므로 종료
            }
        }

        // 2. 현재 퀘스트가 없거나 아직 완료되지 않았다면, 새로 수락 가능한 퀘스트가 있는지 확인
        // (이미 완료한 퀘스트나 현재 진행 중인 퀘스트는 제외)
        HashSet<int> completedQuests = DataManager.Instance.GetGameData().QuestIdSet;

        foreach (QuestData quest in questDatabase.QuestList)
        {
            // 이미 완료했거나 현재 진행 중인 퀘스트는 스킵
            if (completedQuests.Contains(quest.ID) || (CurrentQuestData != null && CurrentQuestData.ID == quest.ID))
                continue;

            bool canAccept = true;

            if(quest.AcceptedConditionList == null || quest.AcceptedConditionList.Count == 0)
            {
                canAccept = false;
            }
            else
            {
                // 수락 조건 태그 확인
                foreach (var needTag in quest.AcceptedConditionList)
                {
                    if (needTag != null && !tagSet.Contains(needTag.ID))
                    {
                        canAccept = false;
                        break;
                    }
                }
            }
  

            if (canAccept)
            {
                Debug.Log($"<color=yellow>[QuestManager]</color> 새 퀘스트 자동 수락: {quest.ID}");
                AccpetedQuest(quest);
                break; // 한 번에 하나의 퀘스트만 수락
            }
        }
    }


}
