using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestData 
{
    public int ID;
    public List<GamePlayTagSO> QuestConditionList;

    public int NextQuestID;

    public string Title;
    [TextArea]
    public string DescriptionSummary;
    [TextArea]
    public string DescriptionDetail;
    [TextArea]
    public string GoalText;

}
