using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestData 
{
    public int ID;
    public List<GamePlayTagSO> AcceptedConditionList;
    public List<GamePlayTagSO> ClearConditionList;

    public int NextQuestID;

    public string Title;
    [TextArea]
    public string DescriptionSummary;
    [TextArea]
    public string DescriptionDetail;
    [TextArea]
    public string GoalText;

}
