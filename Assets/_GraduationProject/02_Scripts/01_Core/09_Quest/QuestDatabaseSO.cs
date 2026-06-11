using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestDatabase", menuName = "Project/Database/Quest Database")]
public class QuestDatabaseSO : ScriptableObject
{
    public List<QuestData> QuestList = new List<QuestData>();
}
