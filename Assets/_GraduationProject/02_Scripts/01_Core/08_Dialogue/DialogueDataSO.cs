using System.Collections.Generic;
using UnityEngine;

public enum DialogueType
{
    Narration,
    Dialogue,
}

[CreateAssetMenu(fileName = "DialogueDataSO", menuName = "Project/DialogueDataSO")]
public class DialogueDataSO : ScriptableObject
{
    [System.Serializable]
    public struct DialogueData
    {
        public string SpeakerName;
        [TextArea]
        public string DialogueText;
        public AudioClip Sound;
    }

    public int DialogueGroupID;
    public DialogueType DialogueType;
    public List<GamePlayTagSO> NeedConditionList;
    public List<DialogueData> DialogueList;
    public List<GamePlayTagSO> ClearAddTagList;
}


