using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public GameData()
    {
        PlayerData = new PlayerData();
    }

    public string LastSaveTime;
    public string StageName;

    public PlayerData PlayerData;

    public List<string> GamePlayTagIdList = new List<string>();

    //==========================================================================================================================
    // Chest Data ==============================================================================================================
    //==========================================================================================================================

    public HashSet<string> OpenedChestSet = new HashSet<string>();
    /// <summary>
    /// 열리 상자 추가
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
