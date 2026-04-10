using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터 그룹의 AI 협업을 관리합니다.
/// </summary>
public class GroupAi : MonoBehaviour
{
    [Header("Settings")]
    public string GroupName = "DefaultGroup";
    public int MaxAttackTokenCount = 2; 
    public float updateInterval = 1.0f;
    
    private List<Enemy> enemies = new List<Enemy>();
    
    public string KEY_TOKEN = "HasAttackToken";
    public string KEY_THREAT = "IsTargetAimingMe";
    public string KEY_TARGET_LOC = "TargetLocation";
    public string KEY_COLLEAGUES = "PeripheralColleagues";

    private class EnemyCandidate { public Enemy enemy; public bool isThreatened; public float distance; }
    private float _updateTimer;
    private bool CombatGroup = false;

    public void GroupAdd(Enemy enemy)
    {
        if (!enemies.Contains(enemy)) { enemies.Add(enemy); UpdateColleaguesCount(); }
    }

    public void GroupRemove(Enemy enemy)
    {
        if (enemies.Contains(enemy)) { enemies.Remove(enemy); UpdateColleaguesCount(); }
    }

    private void UpdateColleaguesCount()
    {
        foreach (var enemy in enemies) if (enemy != null) enemy._aiController._aiBrain.blackboard.SetValue(KEY_COLLEAGUES, enemies.Count);
    }

    public void CombatAll()
    {
        CombatGroup = true;
        foreach (var enemy in enemies) if (enemy != null) enemy._aiController.CombatEnter();
        UpdateColleaguesCount(); 
        AssignSlots();
    }

    public void EngageCombatAll()
    {
        CombatGroup = true;
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.Engage, true);
                enemy._aiController.CombatEnter(true); 
            }
        }
    }

    public void CombatReset()
    {
        CombatGroup = false;
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy._aiController.CombatEnter(false);
                enemy._aiController._aiBrain.blackboard.SetValue(KEY_TOKEN, false);
                enemy._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.Engage, false);
            }
        }
    }

    public void AssignSlots()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;
            enemies[i]._aiController._aiBrain.blackboard.SetValue("SquadSlotIndex", i);
            enemies[i]._aiController._aiBrain.blackboard.SetValue("IsSurrounding", true);
        }
    }

    private void Update()
    {
        if (enemies.Count == 0 || !CombatGroup) return;
        _updateTimer += Time.deltaTime;
        if (_updateTimer >= updateInterval) { UpdateAttackToken(); _updateTimer = 0f; }
    }

    private void UpdateAttackToken()
    {
        List<EnemyCandidate> candidates = new List<EnemyCandidate>();
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.EnemyHealth.IsDead) continue;
            var bb = enemy._aiController._aiBrain.blackboard;
            candidates.Add(new EnemyCandidate { 
                enemy = enemy, 
                isThreatened = bb.GetValue<bool>(KEY_THREAT), 
                distance = Vector3.Distance(enemy.transform.position, enemy.player.transform.position) 
            });
        }
        candidates.Sort((a, b) => {
            if (a.isThreatened != b.isThreatened) return b.isThreatened.CompareTo(a.isThreatened);
            return a.distance.CompareTo(b.distance);
        });
        int tokenGivenCount = 0;
        foreach (var c in candidates) {
            bool giveToken = (tokenGivenCount < MaxAttackTokenCount);
            if (giveToken) tokenGivenCount++;
            c.enemy._aiController._aiBrain.blackboard.SetValue(KEY_TOKEN, giveToken);
        }
    }
}
