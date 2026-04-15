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
        HashSet<string> tagSet = DataManager.Instance.GetGameData().GamePlayTagIdSet;
        QuestDatabaseSO questDatabase = DataManager.Instance.QuestDatabase;


        bool canAccepted = true;
        foreach (QuestData quest in questDatabase.QuestList)
        {
            canAccepted = false;

            foreach (var clearTag in quest.ClearConditionList)
            {
                if (!tagSet.Contains(clearTag.ID))
                {
                    canAccepted = true; 
                    break;
                }
            }

            foreach (var needTag in quest.AcceptedConditionList)
            {
                if (!tagSet.Contains(needTag.ID))
                {
                    Debug.Log("==================" + needTag.ID);
                    canAccepted = false; 
                    break;
                }
            }
            
            // 수락 가능한 상태면 수락
            if(canAccepted)
            {
                AccpetedQuest(quest);
                break;
            }
        }
    }


}
