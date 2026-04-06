using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueDatabaseSO", menuName = "Project/Database/DialogueDatabaseSO")]
public class DialogueDatabaseSO : ScriptableObject
{
    public List<DialogueDataSO> DialogueDataList;
}
