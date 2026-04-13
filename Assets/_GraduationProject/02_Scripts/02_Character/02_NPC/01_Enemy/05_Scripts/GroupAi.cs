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

    [Header("AI Activation Culling")]
    [SerializeField] private bool _useAiActivationCulling = true;
    [SerializeField] private float _activationCheckInterval = 0.2f;
    [SerializeField] private float _enableDistance = 42f;
    [SerializeField] private float _disableDistance = 68f;
    [SerializeField] private float _prewarmViewMargin = 0.25f;
    [SerializeField] private int _maxEnablePerCheck = 4;
    
    private List<Enemy> enemies = new List<Enemy>();
    
    public string KEY_TOKEN = "HasAttackToken";
    public string KEY_THREAT = "IsTargetAimingMe";
    public string KEY_TARGET_LOC = "TargetLocation";
    public string KEY_COLLEAGUES = "PeripheralColleagues";

    private class EnemyCandidate { public Enemy enemy; public bool isThreatened; public float distance; }
    private float _updateTimer;
    private float _activationTimer;
    private bool CombatGroup = false;
    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

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
        if (enemies.Count == 0) return;

        if (_useAiActivationCulling)
        {
            _activationTimer += Time.deltaTime;
            if (_activationTimer >= Mathf.Max(0.05f, _activationCheckInterval))
            {
                _activationTimer = 0f;
                UpdateAiControllerActivation();
            }
        }

        if (!CombatGroup) return;
        _updateTimer += Time.deltaTime;
        if (_updateTimer >= updateInterval) { UpdateAttackToken(); _updateTimer = 0f; }
    }

    private void UpdateAiControllerActivation()
    {
        float enableDistSq = _enableDistance * _enableDistance;
        float disableDistSq = Mathf.Max(_enableDistance + 1f, _disableDistance) * Mathf.Max(_enableDistance + 1f, _disableDistance);
        int enableBudget = Mathf.Max(1, _maxEnablePerCheck);

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.EnemyHealth == null || enemy.EnemyHealth.IsDead) continue;

            AiController ai = enemy._aiController;
            if (ai == null) continue;

            if (ShouldForceEnable(enemy))
            {
                if (!ai.enabled) ai.enabled = true;
                continue;
            }

            var player = enemy.player;
            if (player == null)
            {
                if (!ai.enabled) ai.enabled = true;
                continue;
            }

            float distSq = (enemy.transform.position - player.transform.position).sqrMagnitude;
            bool prewarmVisible = IsInsidePrewarmView(enemy.transform.position);

            bool shouldEnable = distSq <= enableDistSq || prewarmVisible;
            bool shouldDisable = distSq >= disableDistSq && !prewarmVisible;

            if (ai.enabled)
            {
                if (shouldDisable) ai.enabled = false;
            }
            else
            {
                if (shouldEnable && enableBudget > 0)
                {
                    ai.enabled = true;
                    enableBudget--;
                }
            }
        }
    }

    private bool ShouldForceEnable(Enemy enemy)
    {
        if (enemy.CurrentState == EnemyStateController.EnemyState.Stunned ||
            enemy.CurrentState == EnemyStateController.EnemyState.Hit ||
            enemy.CurrentState == EnemyStateController.EnemyState.Attack)
            return true;
        return enemy.CurrentState == EnemyStateController.EnemyState.Rush;
    }

    private bool IsInsidePrewarmView(Vector3 worldPos)
    {
        if (_mainCam == null) _mainCam = Camera.main;
        if (_mainCam == null) return true;

        Vector3 viewPos = _mainCam.WorldToViewportPoint(worldPos);
        return viewPos.z > 0f &&
               viewPos.x > -_prewarmViewMargin && viewPos.x < 1f + _prewarmViewMargin &&
               viewPos.y > -_prewarmViewMargin && viewPos.y < 1f + _prewarmViewMargin;
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
