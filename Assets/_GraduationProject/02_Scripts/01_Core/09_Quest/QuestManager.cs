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

        // 데이터베이스에서 저장된 데이터 불러오기
        if (DataManager.Instance.GetGameData().CurrentQuestId != -1)
        {
            int id = DataManager.Instance.GetGameData().CurrentQuestId;
            AccpetedQuest(DataManager.Instance.GetQuestData(id));
        }
    }

    private void OnApplicationQuit()
    {
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

}
