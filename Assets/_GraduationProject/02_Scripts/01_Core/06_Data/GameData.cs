using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GameData 
{
    public string LastSaveTime;
    public string StageName;

    public PlayerData PlayerData;

    public List<string> GamePlayTagIdList = new List<string>();

    public GameData()
    {
        PlayerData = new PlayerData();
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
