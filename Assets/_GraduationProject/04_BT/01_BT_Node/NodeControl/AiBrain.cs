using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class AiBrain
{
    public BlackBoard blackboard { get; private set; }
    private Enemy _owner;
    private Player _player;
    private Dictionary<string, float> _lastUsedSkillTimes = new Dictionary<string, float>();
    public Enemy.EnemyState CurrentState => _owner.CurrentState;
    public Coroutine lateUpdateCoroutine;
    public AiBrain(Enemy ai)
    {
        _owner = ai;

        blackboard = new BlackBoard();
        _player = _owner.player;

        blackboard.SetValue("HomePosition", _owner.StartPos);
        lateUpdateCoroutine = _owner.StartCoroutine(TickCoroutine());
    }

    public void Tick(float deltaTime)
    {

    }
    int counter = 0;
    private IEnumerator TickCoroutine()
    {
        while (true)
        {
            if (_player != null)
            {
                float distance = Vector3.Distance(_owner.transform.position, _player.transform.position);
                blackboard.SetValue("DistanceBetween", distance);
                bool IsHasLOS = CheckPlayerVisibility();
                blackboard.SetValue("IsHasLOS", IsHasLOS);
                bool OnPlayerLooking = PlayerVisibilityEnemy();
                blackboard.SetValue("OnPlayerLooking", OnPlayerLooking);
                // counter++;
                // if (counter >= 10)
                // {
                //     counter = 0;
                //     blackboard.LogAllValues();
                // }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private bool CheckPlayerVisibility()
    {
        Vector3 toPlayer = _player.transform.position - _owner.transform.position;

        if (Vector3.Angle(_owner.transform.forward, toPlayer.normalized) > 90 * 0.5f)
        {
            return false;
        }
        blackboard.SetValue("LastPlayerPos", _player.transform.position);
        return true;
    }
    private bool PlayerVisibilityEnemy()
    {
        Vector3 toPlayer = _owner.transform.position - _player.transform.position;

        if (Vector3.Angle(_player.transform.forward, toPlayer.normalized) > 70 * 0.5f)
        {
            return false;
        }
        return true;
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
        return Vector3.Distance(_owner.transform.position, _player.transform.position) <= atkRange;
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

    public void CombatEnter(bool combat = true)
    {
        _isCombat = combat;
        if (!_isCombat)
        {
            blackboard.SetValue("IsPlayerDetected", false);

        }
        if (_isCombat)
        {
            _owner.animator.SetTrigger("Discover_Player");
            _owner.Movement.StopMovement();
            blackboard.SetValue("IsPlayerDetected", true);
        }
    }

    internal void AddEnemyAttackData(EnemyAttackData enemyAttackData)
    {
        blackboard.SetValue(enemyAttackData.AttackName, enemyAttackData);
    }

    public bool _isStunned { get; private set; } = false;

    #endregion
}
