using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터 그룹의 AI 협업을 관리합니다.
/// </summary>
public class GroupAi : MonoBehaviour
{
    private static int s_lastEnableFrame = -1;
    private static int s_enabledThisFrame = 0;

    [Header("Settings")]
    public string GroupName = "DefaultGroup";
    public int MaxAttackTokenCount = 2; 
    public float updateInterval = 1.0f;

    [Header("AI Auto Activation (Wave 무관)")]
    [SerializeField] private bool _useAutoActivation = true;
    [SerializeField] private float _activationCheckInterval = 0.2f;
    [SerializeField] private float _enableDistance = 40f;
    [SerializeField] private float _disableDistance = 65f;
    [SerializeField] private float _viewMargin = 0.35f;
    [SerializeField] private int _maxEnablePerCheck = 2;
    [SerializeField] private int _globalMaxEnablePerFrame = 3;
    
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

        if (_useAutoActivation)
        {
            _activationTimer += Time.deltaTime;
            if (_activationTimer >= Mathf.Max(0.05f, _activationCheckInterval))
            {
                _activationTimer = 0f;
                UpdateAiActivation();
            }
        }

        if (!CombatGroup) return;
        _updateTimer += Time.deltaTime;
        if (_updateTimer >= updateInterval) { UpdateAttackToken(); _updateTimer = 0f; }
    }

    private void UpdateAiActivation()
    {
        if (Time.frameCount != s_lastEnableFrame)
        {
            s_lastEnableFrame = Time.frameCount;
            s_enabledThisFrame = 0;
        }

        float enableDistSq = _enableDistance * _enableDistance;
        float disableDist = Mathf.Max(_enableDistance + 1f, _disableDistance);
        float disableDistSq = disableDist * disableDist;
        int enableBudget = Mathf.Max(1, _maxEnablePerCheck);
        int globalBudget = Mathf.Max(1, _globalMaxEnablePerFrame) - s_enabledThisFrame;
        if (globalBudget <= 0) return;

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.EnemyHealth == null || enemy.EnemyHealth.IsDead) continue;

            AiController ai = enemy._aiController;
            if (ai == null) continue;

            if (enemy.CurrentState == EnemyStateController.EnemyState.Attack ||
                enemy.CurrentState == EnemyStateController.EnemyState.Rush ||
                enemy.CurrentState == EnemyStateController.EnemyState.Hit ||
                enemy.CurrentState == EnemyStateController.EnemyState.Stunned)
            {
                if (!ai.enabled) ai.enabled = true;
                continue;
            }

            bool inView = IsInExtendedView(enemy);
            var player = enemy.player;
            float distSq = player != null
                ? (enemy.transform.position - player.transform.position).sqrMagnitude
                : float.MaxValue;

            bool shouldEnable = inView || distSq <= enableDistSq;
            bool shouldDisable = !inView && distSq >= disableDistSq;

            if (!ai.enabled)
            {
                if (shouldEnable && enableBudget > 0 && globalBudget > 0)
                {
                    ai.enabled = true;
                    enableBudget--;
                    globalBudget--;
                    s_enabledThisFrame++;
                }
            }
            else if (shouldDisable)
            {
                ai.enabled = false;
            }
        }
    }

    private bool IsInExtendedView(Enemy enemy)
    {
        if (_mainCam == null) _mainCam = Camera.main;
        if (_mainCam == null) return true;

        if (enemy != null)
        {
            var r = enemy.GetComponentInChildren<Renderer>();
            if (r != null && r.isVisible) return true;
        }

        Vector3 worldPos = enemy != null ? enemy.transform.position : Vector3.zero;
        Vector3 viewPos = _mainCam.WorldToViewportPoint(worldPos);
        return viewPos.z > 0f &&
               viewPos.x > -_viewMargin && viewPos.x < 1f + _viewMargin &&
               viewPos.y > -_viewMargin && viewPos.y < 1f + _viewMargin;
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
