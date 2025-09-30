using System.Collections.Generic;
using UnityEngine;

public class GroupAi : MonoBehaviour
{
    List<Enemy> enemies = new List<Enemy>();
    public void GroupAdd(Enemy enemy)
    {
        enemies.Add(enemy);
    }
    public void GroupRemove(Enemy enemy)
    {
        enemies.Remove(enemy);
    }
    public bool OnlyCowardly()
    {
        if (enemies.Count == 1)
        {
            foreach (var enemy in enemies)
            {
                if (enemy.EnemyType == Enemy.Enemy_Type.Cowardly)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public void CombatAll()
    {
        foreach (var enemy in enemies)
        {
            
            enemy._aiController.CombatEnter();
            
        }
    }
}
