using UnityEngine;

/// <summary>
/// 게임 플레이 태그 스크립터블 오브젝트
/// 플래그 형태로 사용
/// </summary>
[CreateAssetMenu(fileName = "GamePlayTagSO", menuName = "Project/Tag/GamePlayTag")]
public class GamePlayTagSO : ScriptableObject 
{
    public string TagId;
}
