using UnityEngine;

public enum StunType { None, Weak, Full, Any }

public enum ImmunityLevel
{
    None,
    Minor,
    Major,
}

public class ParrySystem : MonoBehaviour, IParryable, ICounterable
{
    public bool IsCounterable { get; private set; } = false;
    float _stunExitTime = -Mathf.Infinity;
    public StunType CurrentStun { get; private set; } = StunType.None;
    public bool _isStunned => CurrentStun != StunType.None;
    public float StunExitTime => _stunExitTime;

    public bool WasParriedThisFrame { get; private set; } = false;

    public enum EnemyState
    {
        Normal,
        Stunned,
        StunnedExit
    }

    public EnemyState CurrentState { get; private set; } = EnemyState.Normal;

    [SerializeField] private float _stunTime = 3f;
    private Enemy _owner;

    public void Initialize(Enemy enemy)
    {
        _owner = enemy;
        IsCounterable = false;
        ClearStun();
        CurrentState = EnemyState.Normal;
    }

    public void SetCounterAttack(bool value)
    {
        IsCounterable = value;
    }

    public void ExecuteCounterEffect()
    {
        SetCounterAttack(false);
    }

    public bool Parry(AttackType attackType)
    {
        if (_owner.EnemyHealth.CheckStunImmunity != null && _owner.EnemyHealth.CheckStunImmunity(attackType))
        {
            Debug.Log("[ParrySystem] 면역 상태로 인해 경직이 적용되지 않습니다.");
            return false;
        }

        bool isNormalCounter = attackType == AttackType.Normal_Counter;

        if (isNormalCounter)
        {
            if (_owner?.player?.Data?.NormalAttackConfigList != null && _owner.player.Data.NormalAttackConfigList.Count > 0)
            {
                _owner.StiffnessSystem.AddStiffness(_owner.player.Data.NormalAttackConfigList[0].Stiffness, attackType: AttackType.Normal_Counter);
            }
        }
        else
        {
            if (_owner?.player?.Data?.HeavyAttackConfigList != null && _owner.player.Data.HeavyAttackConfigList.Count > 0)
            {
                _owner.StiffnessSystem.AddStiffness(_owner.player.Data.HeavyAttackConfigList[0].Stiffness, attackType: AttackType.Strong_Counter);
            }
        }

        DeactivateImmunity();
        return true;
    }

    public void ApplyStun(float stunDuration, bool isCounterAttack = false)
    {
        if (!isCounterAttack)
        {
            return;
        }

        if (_isStunned || _owner.EnemyHealth.IsDead) return;

        Debug.Log($"[ParrySystem] ApplyStun {_owner.name} duration={stunDuration}");
        CurrentStun = StunType.Full;
        _stunExitTime = Time.time + stunDuration;

        _owner.Movement.StopMovement();
        _owner.animHandler?.ResetAllFlags();
        _owner._animationBridge?.ResetAllTriggers();
        _owner._animationBridge?.ClearIsAttacking();

        _owner.AnimationBool("Stun", true);
        CurrentState = EnemyState.Stunned;
        _owner.SetState(EnemyStateController.EnemyState.Stunned);
    }

    public void ApplyWeakStun(float stunDuration)
    {
        if (_isStunned || _owner.EnemyHealth.IsDead) return;

        Debug.Log($"[ParrySystem] ApplyWeakStun {_owner.name} duration={stunDuration}");
        CurrentStun = StunType.Weak;
        _stunExitTime = Time.time + stunDuration;
        WasParriedThisFrame = true;

        _owner.Movement.StopMovement();
        _owner.animHandler?.ResetAllFlags();
        _owner._animationBridge?.ResetAllTriggers();
        _owner._animationBridge?.ClearIsAttacking();

        _owner.AnimationBool("WeakStun", true);
        CurrentState = EnemyState.Stunned;
        _owner.SetState(EnemyStateController.EnemyState.Stunned);
    }

    public void ClearStun()
    {
        if (_owner == null) return;

        Debug.Log($"[ParrySystem] ClearStun {_owner.name}");
        _owner.AnimationBool("Stun", false);
        _owner.AnimationBool("WeakStun", false);
        CurrentStun = StunType.None;
        CurrentState = EnemyState.StunnedExit;
        _owner.SetState(EnemyStateController.EnemyState.Idle);
    }

    public void StateNormal()
    {
        CurrentState = EnemyState.Normal;
    }

    public void ActivateMinorImmunity()
    {
        Debug.Log("[ParrySystem] 경경직 면역이 활성화되었습니다.");
        _owner.EnemyHealth.SetImmunityLevel(ImmunityLevel.Minor);
    }

    public void ActivateMajorImmunity()
    {
        _owner.EnemyHealth.SetImmunityLevel(ImmunityLevel.Major);
    }

    public void DeactivateImmunity()
    {
        _owner.EnemyHealth.SetImmunityLevel(ImmunityLevel.None);
        Debug.Log("[ParrySystem] 면역이 해제되었습니다.:" + _owner.EnemyHealth._currentImmunityLevel);
    }

    public void ResetParriedFlag()
    {
        Debug.Log("[ParrySystem] ResetParriedFlag called");
        WasParriedThisFrame = false;
    }
}
