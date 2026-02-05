using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStat", menuName = "Character/EnemyStat")]
public class EnemyStat : ScriptableObject
{
    public int Maxhealth;
    public float MoveSpeed;
    public float SeeRange;
    public float DetectRange;
    public float CircleSeeRange;
    public int MoneyReward = 10;
    [Tooltip("약한 경직 시간")]
    public float _weakStiffnessTime = 1.5f;
    [Tooltip("강한 경직 시간")]
    public float _stiffnessTime = 3f;
}