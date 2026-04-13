using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;

public class AiBrain
{
    private const bool ENABLE_COOLDOWN_LOG = false;

    public BlackBoard blackboard { get; private set; }
    private Enemy _owner;
    private PlayerController _player;
    private readonly Dictionary<string, float> _lastUsedSkillTimes = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _customCooldownDurations = new Dictionary<string, float>();
    private float _nextSensingTime;

    public EnemyStateController.EnemyState CurrentState => _owner.CurrentState;
    public bool _isCombat { get; private set; } = false;

    public AiBrain(Enemy ai)
    {
        _owner = ai;
        blackboard = new BlackBoard();
        _player = _owner.player;
        ResetBrain();
    }

    public void ResetBrain()
    {
        blackboard.SetValue(EnemyBlackboardKeys.HomePosition, _owner.StartPos);
        blackboard.SetValue(EnemyBlackboardKeys.Self, _owner.gameObject);
        _lastUsedSkillTimes.Clear();
        _customCooldownDurations.Clear();
        _nextSensingTime = 0f;
    }

    public void Tick(float deltaTime) { }

    public void TickSensing(float now, float interval)
    {
        if (_player == null) _player = _owner.player;
        if (_player == null) return;

        float safeInterval = Mathf.Max(0.02f, interval);
        if (now < _nextSensingTime) return;
        _nextSensingTime = now + safeInterval;

        CheckPlayerVisibility();
        PlayerVisibilityEnemy();
    }

    private void CheckPlayerVisibility()
    {
        if (_isCombat || blackboard.GetValue<bool>(EnemyBlackboardKeys.Engage))
        {
            blackboard.SetValue(EnemyBlackboardKeys.IsHasLOS, true);
            blackboard.SetValue(EnemyBlackboardKeys.DetectPlayer, true);
            if (_player != null) blackboard.SetValue(EnemyBlackboardKeys.LastPlayerPos, _player.transform.position);
            return;
        }

        Vector3 myPos = _owner.transform.position;
        Vector3 targetPos = _player.transform.position;
        Vector3 toPlayer = targetPos - myPos;
        float dist = toPlayer.magnitude;

        blackboard.SetValue(EnemyBlackboardKeys.DistanceBetween, dist);
        blackboard.SetValue(EnemyBlackboardKeys.DetectPlayer, dist <= _owner.enemyStat.DetectRange);

        bool hasLos = false;
        if (dist > _owner.enemyStat.CircleSeeRange) hasLos = true;
        else if (dist <= _owner.enemyStat.SeeRange)
        {
            if (Vector3.Angle(_owner.transform.forward, toPlayer.normalized) <= 45f)
            {
                hasLos = true;
                blackboard.SetValue(EnemyBlackboardKeys.LastPlayerPos, targetPos);
            }
        }

        blackboard.SetValue(EnemyBlackboardKeys.IsHasLOS, hasLos);
    }

    private void PlayerVisibilityEnemy()
    {
        if (_player == null) return;
        Vector3 toEnemy = _owner.transform.position - _player.transform.position;
        bool isLooking = Vector3.Angle(_player.transform.forward, toEnemy.normalized) <= 35f;
        blackboard.SetValue(EnemyBlackboardKeys.OnPlayerLooking, isLooking);
    }

    public void CombatEnter(bool combat = true)
    {
        if (_isCombat == combat) return;

        _isCombat = combat;
        blackboard.SetValue(EnemyBlackboardKeys.IsPlayerDetected, _isCombat);
        if (_isCombat)
        {
            blackboard.SetValue(EnemyBlackboardKeys.LastAttackSuccessTime, Time.time);
            blackboard.SetValue(EnemyBlackboardKeys.LastTakeHitTime, Time.time);
        }

        if (_owner.animator != null)
        {
            foreach (var param in _owner.animator.parameters)
            {
                if (param.name == "IsCombat")
                {
                    _owner.AnimationBool("IsCombat", _isCombat);
                    break;
                }
            }
        }

        if (_owner.Shield != null) _owner.Shield.IsActive = _isCombat;
        if (_isCombat) _owner.Movement.StopMovement();
    }

    public bool IsSkillReady(string skillName, float cooldownDuration)
    {
        if (_lastUsedSkillTimes.TryGetValue(skillName, out float lastUsedTime))
        {
            if (_customCooldownDurations.TryGetValue(skillName, out float customDuration))
            {
                float elapsed = Time.time - lastUsedTime;
                bool isReady = elapsed >= customDuration;
                if (ENABLE_COOLDOWN_LOG)
                    BTDebug.Log($"[AiBrain] {skillName} custom cooldown check: elapsed={elapsed:F2}s need={customDuration:F2}s ready={isReady}");
                if (!isReady) return false;
            }

            return Time.time >= lastUsedTime + cooldownDuration;
        }

        return true;
    }

    public bool IsActionable()
    {
        switch (CurrentState)
        {
            case EnemyStateController.EnemyState.Attack:
            case EnemyStateController.EnemyState.Rush:
            case EnemyStateController.EnemyState.Die:
                return true;
            default:
                return false;
        }
    }

    public bool IsInAttackRange(float atkRange)
    {
        return _player != null && (_player.transform.position - _owner.transform.position).sqrMagnitude <= atkRange * atkRange;
    }

    public void StartSkillCooldown(string skillName)
    {
        _lastUsedSkillTimes[skillName] = Time.time;
    }

    public void StartSkillCooldown(string skillName, float customCooldownDuration)
    {
        _lastUsedSkillTimes[skillName] = Time.time;
        _customCooldownDurations[skillName] = customCooldownDuration;
        if (ENABLE_COOLDOWN_LOG)
            BTDebug.Log($"[AiBrain] {skillName} custom cooldown start: {customCooldownDuration:F2}s");
    }

    public void ClearSkillCooldown(string skillName)
    {
        _lastUsedSkillTimes.Remove(skillName);
        _customCooldownDurations.Remove(skillName);
        if (ENABLE_COOLDOWN_LOG)
            BTDebug.Log($"[AiBrain] {skillName} cooldown cleared");
    }

    public float GetLastSkillUseTime(string skillName)
    {
        return _lastUsedSkillTimes.TryGetValue(skillName, out float time) ? time : -1f;
    }

    public void AddEnemyAttackData(EnemyAttackData data)
    {
        blackboard.SetValue(data.AttackName, data);
        _lastUsedSkillTimes[data.AttackName] = -9999f;
    }

    public T getService<T>() where T : ServiceNode
    {
        var controller = _owner != null ? _owner._aiController : null;
        return controller != null ? controller.GetService<T>() : null;
    }
}
