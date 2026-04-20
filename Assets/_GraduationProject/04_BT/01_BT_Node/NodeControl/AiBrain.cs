using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using BehaviorTree;

public class AiBrain
{
    public BlackBoard blackboard { get; private set; }
    private Enemy _owner;
    private PlayerController _player;
    private Dictionary<string, float> _lastUsedSkillTimes = new Dictionary<string, float>();
    private Dictionary<string, float> _customCooldownDurations = new Dictionary<string, float>();
    public EnemyStateController.EnemyState CurrentState => _owner.CurrentState;
    private Coroutine _lateUpdateCoroutine;

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
        if (_lateUpdateCoroutine != null) _owner.StopCoroutine(_lateUpdateCoroutine);
        _lateUpdateCoroutine = _owner.StartCoroutine(StaggeredStart());
    }

    private IEnumerator StaggeredStart()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.2f));
        yield return TickCoroutine();
    }

    public void Tick(float deltaTime) { }

    private IEnumerator TickCoroutine()
    {
        var wait = new WaitForSeconds(0.1f);
        while (true)
        {
            if (_player != null) { CheckPlayerVisibility(); PlayerVisibilityEnemy(); }
            yield return wait;
        }
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
        if(dist > _owner.enemyStat.CircleSeeRange) hasLos = true;
        else if(dist <= _owner.enemyStat.SeeRange)
        {
            if (Vector3.Angle(_owner.transform.forward, toPlayer.normalized) <= 90 * 0.5f)
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
        bool isLooking = Vector3.Angle(_player.transform.forward, toEnemy.normalized) <= 70 * 0.5f;
        blackboard.SetValue(EnemyBlackboardKeys.OnPlayerLooking, isLooking);
    }

    public bool _isCombat { get; private set; } = false;

    public void CombatEnter(bool combat = true)
    {
        // [Optimization] ?대? ?대떦 ?곹깭?쇰㈃ 以묐났 泥섎━瑜?諛⑹??섏뿬 'StopMovement'媛 諛섎났 ?몄텧?섎뒗 寃껋쓣 留됱쓬
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
                if (param.name == "IsCombat") { _owner.AnimationBool("IsCombat", _isCombat); break; }
            }
        }

        if (_owner.Shield != null) _owner.Shield.IsActive = _isCombat; 
        
        // ?꾪닾 吏꾩엯 ?쒖뿉留??대룞??硫덉텛怨??ㅼ쓬 BT ?먮떒???湲?
        if (_isCombat) _owner.Movement.StopMovement();
    }

    public bool IsSkillReady(string skillName, float cooldownDuration)
    {
        if (_lastUsedSkillTimes.TryGetValue(skillName, out float lastUsedTime))
        {
            // 而ㅼ뒪? 荑⑤떎?댁씠 ?덈뒗吏 ?뺤씤
            if (_customCooldownDurations.TryGetValue(skillName, out float customDuration))
            {
                float elapsed = Time.time - lastUsedTime;
                bool isReady = elapsed >= customDuration;
                if(!isReady)
                    return isReady;
            }
            return Time.time >= lastUsedTime + cooldownDuration;
        }
        return true;
    }
    public bool IsActionable() {
        switch (CurrentState) { case EnemyStateController.EnemyState.Attack: case EnemyStateController.EnemyState.Rush: case EnemyStateController.EnemyState.Die: return true; default: return false; }
    }
    public bool IsInAttackRange(float atkRange) => _player != null && (_player.transform.position - _owner.transform.position).sqrMagnitude <= atkRange * atkRange;
    public void StartSkillCooldown(string skillName) => _lastUsedSkillTimes[skillName] = Time.time;
    public void StartSkillCooldown(string skillName, float customCooldownDuration) 
    {
        _lastUsedSkillTimes[skillName] = Time.time;
        _customCooldownDurations[skillName] =  customCooldownDuration;
    }
    
    public void ClearSkillCooldown(string skillName)
    {
        _lastUsedSkillTimes.Remove(skillName);
        _customCooldownDurations.Remove(skillName);
    }
    public float GetLastSkillUseTime(string skillName) => _lastUsedSkillTimes.TryGetValue(skillName, out float time) ? time : -1f;
    public void AddEnemyAttackData(EnemyAttackData data) { blackboard.SetValue(data.AttackName, data); _lastUsedSkillTimes[data.AttackName] = -9999f; }

    public T getService<T>() where T : ServiceNode
    {
        var controller = _owner != null ? _owner._aiController : null;
        return controller != null ? controller.GetService<T>() : null;
    }


}
