using UnityEngine;

[CreateAssetMenu(fileName = "TresuerBoxReward", menuName = "Project/TresuerBoxReward")]
public class RewardSO : ScriptableObject
{   
    public int MoneyAmount; // 보물 상자에서 얻을 수 있는 골드 양
    public int SpecialMoneyAmount; // 보물 상자에서 얻을 수 있는 골드 양
    public int MaxPotionAmount; // 보물 상자에서 얻을 수 있는 포션 양
}
