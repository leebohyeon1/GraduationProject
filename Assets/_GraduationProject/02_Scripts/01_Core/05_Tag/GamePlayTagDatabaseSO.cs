using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GamePlayTagDatabaseSO", menuName = "Project/Database/GamePlayTagDatabase")]
public class GamePlayTagDatabaseSO : ScriptableObject
{
    public List<GamePlayTagSO> GamePlayTagList;   
}
