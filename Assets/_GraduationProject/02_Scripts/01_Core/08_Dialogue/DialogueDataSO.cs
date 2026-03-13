using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueDataSO", menuName = "Project/DialogueDataSO")]
public class DialogueDataSO : ScriptableObject
{
    public int DialogueGroupID;
    public int SequenceIndex;
    public List<GamePlayTagSO> NeedConditionList;
    public string SpeakerName;
    public string DialogueText;
}
