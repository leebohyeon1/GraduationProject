using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyRewardSO", menuName = "Character/EnemyRewardSO")]
public class EnemyRewardSO : ScriptableObject
{
    public Dictionary<string, int> enemyExtraMoney = new Dictionary<string, int>();

    public void AddMoneyToEnemies(string enemy, float amount)
    {
        if (enemyExtraMoney.ContainsKey(enemy))
        {
            enemyExtraMoney[enemy] += (int)amount;
        }
        else
        {
            enemyExtraMoney[enemy] = (int)amount;
            Debug.Log($"Added {amount} money to enemy {enemy}. Total extra money: {enemyExtraMoney[enemy]}");
        }
    }

    public void RemoveMoneyFromEnemies(string enemy)
    {
        if (enemyExtraMoney.ContainsKey(enemy))
        {
            enemyExtraMoney[enemy] = 0; // 음수 방지
            enemyExtraMoney.Remove(enemy);
        }
    }

    public void ResetAllData()
    {
        enemyExtraMoney.Clear();
    }
}