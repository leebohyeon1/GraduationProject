using UnityEngine;
using BehaviorTree;
using MoreMountains.Tools;
using System.Collections.Generic;
public class AiBrain
{
    private Enemy _owner;
    private ActionTree _behaviorTree;
    private Dictionary<string, float> _lastUsedSkillTimes = new Dictionary<string, float>();
    public Enemy.EnemyState CurrentState => _owner.CurrentState;
    public AiBrain(ActionTree behaviorTree, Enemy ai)
    {
        _owner = ai;
        _behaviorTree = behaviorTree.Clone();
        _behaviorTree.SetRunner(ai,this);
        _behaviorTree.rootNode?.initNode();

    }
    public void Tick(float deltaTime)
    {
        _behaviorTree?.rootNode?.Evaluate();
    }


    #region Behavior Tree Condition
    public bool IsSkillReady(string skillName, float cooldownDuration)
    {
        if (_lastUsedSkillTimes.TryGetValue(skillName, out float lastUsedTime))
        {
            return Time.time >= lastUsedTime + cooldownDuration;
        }
        return true;
    }

        public bool IsActionable()
    {
        switch (CurrentState)
        {
            case Enemy.EnemyState.Attack:
            case Enemy.EnemyState.Rush:
            case Enemy.EnemyState.Stunned:
            case Enemy.EnemyState.Die:
                return true; // 이 상태에서는 행동을 중단할 수 없습니다.
            default:
                return false; // Idle, Patrol 등은 행동을 중단할 수 있습니다.

        }
    }

    public bool IsInAttackRange(float atkRange)
    {
        return Vector3.Distance(_owner.transform.position, _owner.player.transform.position) <= atkRange;
    }

    // IsInDetectionRange, IsInChaseRange 등도 동일한 방식으로 이전

    // --- BT Node가 요청할 상태 변경 로직 ---
    public void StartSkillCooldown(string skillName)
    {
        _lastUsedSkillTimes[skillName] = Time.time;
    }





    public float GetLastSkillUseTime(string skillName)
    {
        if (_lastUsedSkillTimes.TryGetValue(skillName, out float time))
        {
            return time;
        }
        return -1f;
    }
    public bool _isCombat { get; private set; } = false;
    
    public void CombatEnter()
    {
        if (!_isCombat)
        {
            _owner.animator.SetTrigger("Discover_Player");
            _owner.Movement.StopMovement();
            _isCombat = true;
        }
    }
    public bool _isStunned { get; private set; } = false;

    #endregion
}
