using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueDataSO", menuName = "Project/DialogueDataSO")]
public class DialogueDataSO : ScriptableObject
{
    [System.Serializable]
    public struct DialogueData
    {
        public string SpeakerName;
        public string DialogueText;
    }

    public int DialogueGroupID;
    public List<GamePlayTagSO> NeedConditionList;
    public List<DialogueData> DialogueList;
}


