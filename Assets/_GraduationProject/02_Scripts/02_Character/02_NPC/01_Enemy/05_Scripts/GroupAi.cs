using System;
using System.Collections.Generic;
using UnityEngine;

public class GroupAi : MonoBehaviour
{
    [Header("Settings")]
    public string GroupName = "DefaultGroup";
    public int MaxAttackTokenCount = 2; 
    public float updateInterval = 0.1f;
    
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
        CombatGroup = true; // [Fix] 발견 신호 발생 시 그룹 전체를 전투 모드로 간주
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                // [핵심 Fix] 블랙보드 키만 설정하는 게 아니라, 실제 전투 상태(CombatEnter)로 강제 진입
                // 이렇게 해야 BT의 '시야 체크' 등의 조건문을 건너뛰고 전투 트리를 탑니다.
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
