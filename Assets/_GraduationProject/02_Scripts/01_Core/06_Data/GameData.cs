using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GameData : ISerializationCallbackReceiver
{
    public GameData()
    {
        PlayerData = new PlayerData();
    }

    public string LastSaveTime;
    public string StageName;

    public string LastMainScene; 
    public PlayerData PlayerData;

    //==========================================================================================================================
    // Quest Data ==============================================================================================================
    //==========================================================================================================================
    [NonSerialized] public HashSet<int> QuestIdSet = new HashSet<int>();
    [SerializeField] private List<int> _serializedQuestIdList = new List<int>();

    public int CurrentQuestId = -1;

    public void AddQuestID(int id)
    {
        QuestIdSet.Add(id);
    }

    //==========================================================================================================================
    // Game Play Tag Data ======================================================================================================
    //==========================================================================================================================
    [NonSerialized] public HashSet<string> GamePlayTagIdSet = new HashSet<string>();
    [SerializeField] private List<string> _serializedGamePlayTagIdList = new List<string>();

    public void AddGamePlayTag(string id)
    {
        GamePlayTagIdSet.Add(id);
    }

    public void RemoveGamePlayTag(string id)
    {
        GamePlayTagIdSet.Remove(id);
    }

    //==========================================================================================================================
    // Chest Data ==============================================================================================================
    //==========================================================================================================================
    [NonSerialized] public HashSet<string> OpenedChestSet = new HashSet<string>();
    [SerializeField] private List<string> _serializedOpenedChestList = new List<string>();

    /// <summary>
    /// 열린 상자 추가
    /// </summary>
    /// <param name="chestId">상자 ID</param>
    public void AddOpendChest(string chestId)
    {
        OpenedChestSet.Add(chestId);
    }

    /// <summary>
    /// 상자 열려있는지 여부 확인
    /// </summary>
    /// <param name="chestId">상자 ID</param>
    /// <returns>열려있는가</returns>
    public bool IsChestOpened(string chestId)
    {
        return OpenedChestSet.Contains(chestId);
    }

    //==========================================================================================================================
    // Dialogue Data ==============================================================================================================
    //==========================================================================================================================
    [NonSerialized] public HashSet<int> CompleteDialogueSet = new HashSet<int>();
    [SerializeField] private List<int> _serializedCompleteDialogueList = new List<int>();

    public void CompleteDialogue(int groupId)
    {
        CompleteDialogueSet.Add (groupId);
    }

    public bool IsCompleteDialogue(int groupId)
    {
        return CompleteDialogueSet.Contains(groupId);
    }

    //==========================================================================================================================
    // Monster Death Data ======================================================================================================
    //==========================================================================================================================
    [NonSerialized] public HashSet<string> DeadMonsterSet = new HashSet<string>();
    [SerializeField] private List<string> _serializedDeadMonsterList = new List<string>();

    /// <summary>
    /// 죽은 몬스터 추가
    /// </summary>
    /// <param name="monsterId">몬스터 고유 ID</param>
    public void AddDeadMonster(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId)) return;
        DeadMonsterSet.Add(monsterId);
    }

    /// <summary>
    /// 몬스터가 죽었는지 확인
    /// </summary>
    /// <param name="monsterId">몬스터 고유 ID</param>
    /// <returns>사망 여부</returns>
    public bool IsMonsterDead(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId)) return false;
        return DeadMonsterSet.Contains(monsterId);
    }

    /// <summary>
    /// 죽은 몬스터 목록 초기화 (리스폰 시 사용)
    /// </summary>
    public void ClearDeadMonsters()
    {
        DeadMonsterSet.Clear();
    }

    //==========================================================================================================================
    // Scene Visit Data ========================================================================================================
    //==========================================================================================================================
    public List<string> VisitedScenes = new List<string>();

    /// <summary>
    /// 해당 씬을 처음 방문하는지 확인
    /// </summary>
    public bool IsFirstVisit(string sceneName)
    {
        return !VisitedScenes.Contains(sceneName);
    }

    /// <summary>
    /// 씬을 방문한 것으로 표시
    /// </summary>
    public void MarkSceneAsVisited(string sceneName)
    {
        if (!VisitedScenes.Contains(sceneName))
        {
            VisitedScenes.Add(sceneName);
        }
    }

    //==========================================================================================================================
    // Serialization Callbacks =================================================================================================
    //==========================================================================================================================

    public void OnBeforeSerialize()
    {
        // HashSet 데이터를 List로 복사하여 JSON 직렬화가 가능하게 함
        _serializedQuestIdList = new List<int>(QuestIdSet);
        _serializedGamePlayTagIdList = new List<string>(GamePlayTagIdSet);
        _serializedOpenedChestList = new List<string>(OpenedChestSet);
        _serializedCompleteDialogueList = new List<int>(CompleteDialogueSet);
        _serializedDeadMonsterList = new List<string>(DeadMonsterSet);
    }

    public void OnAfterDeserialize()
    {
        // JSON 로드 후 List 데이터를 HashSet으로 복구
        QuestIdSet = new HashSet<int>(_serializedQuestIdList);
        GamePlayTagIdSet = new HashSet<string>(_serializedGamePlayTagIdList);
        OpenedChestSet = new HashSet<string>(_serializedOpenedChestList);
        CompleteDialogueSet = new HashSet<int>(_serializedCompleteDialogueList);
        DeadMonsterSet = new HashSet<string>(_serializedDeadMonsterList);
    }
}

[Serializable]
public class SaveDataContainer
{
    public List<GameData> DataList;

    // 생성자
    public SaveDataContainer(List<GameData> dataList)
    {
        this.DataList = dataList;
    }
}
