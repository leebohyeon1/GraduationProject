using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GroupAi : MonoBehaviour
{
    List<Enemy> enemies = new List<Enemy>();
    public void GroupAdd(Enemy enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
        else
        {
            Debug.LogWarning($"이미 그룹에 존재하는 몬스터 {enemy.gameObject.GetInstanceID()}입니다.");
        }
    }

    public void GroupRemove(Enemy enemy)
    {
        enemies.Remove(enemy);
        enemy._aiController._aiBrain.blackboard.SetValue("PeripheralColleagues", enemies.Count);
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
            enemy._aiController._aiBrain.blackboard.SetValue("PeripheralColleagues", enemies.Count);
        }
        AssignSlots();
    }

    public void CombatReset()
    {
        foreach (var enemy in enemies)
        {
            enemy._aiController.CombatEnter(false);
            enemy._aiController._aiBrain.blackboard.SetValue("PeripheralColleagues", enemies.Count);
        }
    }

    public void AssignSlots()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].EnemyType != Enemy.Enemy_Type.Cunning)
            {
                return;
            }
            // 2. 나의 고유 번호 (Slot Index)
            enemies[i]._aiController._aiBrain.blackboard.SetValue("SquadSlotIndex", i);
            // 3. 포위 명령
            enemies[i]._aiController._aiBrain.blackboard.SetValue("IsSurrounding", true);
        }
    }
}
