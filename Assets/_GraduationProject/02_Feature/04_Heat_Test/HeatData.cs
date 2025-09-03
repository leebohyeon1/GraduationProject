using System.Collections.Generic;
using UnityEngine;

public enum ActorType { Player, Monster, Object }
public enum ModeType { Instant, Pulse }

[System.Serializable]
public class HeatData
{
    public string RuleID;
    public bool HeatType;
    public ActorType TargetType;
    public int TierID;
    public ModeType ModeType;
    public int Heating;
    public int ManaCost;
    public float Duration_s;
    public float Tick_s;
}


[CreateAssetMenu(fileName = "HeatData", menuName = "GameData/HeatData")]
public class HeatDataBase : ScriptableObject
{
    public List<HeatData> heatDataList;
    public HeatData GetHeatData(string ruleID, int tierID = 0)
    {
        return heatDataList.Find(data => data.RuleID == ruleID && data.TierID == tierID);
    }
    //몹,오브젝트에 따른 데미지를 위해 ↑위에 내용 안쓰고 이것만 사용하는 편이 버그가 없을꺼같아보임(위에 껄 쓰면 데미지가 똑같을 때)
    public HeatData GetHeatData(string ruleID, ActorType targetType, int tierID = 0)
    {
        return heatDataList.Find(data => data.RuleID == ruleID && data.TargetType == targetType && data.TierID == tierID);
    }
}
