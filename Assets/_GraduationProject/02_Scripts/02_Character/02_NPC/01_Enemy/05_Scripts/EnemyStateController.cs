
using System;
using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    private Enemy _owner;
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Beam,
        Die,
        Stunned,
        Rush,
        Hit,
        Discover,
    }
    public EnemyState CurrentState { get; private set; }
    public BlackBoard Blackboard => _owner._aiController._aiBrain.blackboard;
    
    public event Action<EnemyState, EnemyState> OnStateChanged;
    
    public bool IsStateLocked { get; private set; }
    public float LastStunEndTime { get; private set; } = -10f;
    public bool IsRecoveringFromStun => Time.time < LastStunEndTime + 0.5f;

    public void Initialize(Enemy owner)
    {
        _owner = owner;
        IsStateLocked = false;
    }
    void Start()
    {
        
        SetState(EnemyState.Idle);
        Blackboard.SetValue(EnemyBlackboardKeys.CurrentStatus, CurrentState);
    }
    public void SetState(EnemyState newState)
    {
        if (IsStateLocked && newState != EnemyState.Die && newState != EnemyState.Stunned)
        {
            // Debug.Log(string.Format("[StateController : {0}] 상태 변경 거부 (Locked). 요청: {1}, 현재: {2}", _owner.name, newState, CurrentState));
            return;
        }

        if (CurrentState == newState)
        {
            // Debug.Log(string.Format("[StateController : {0}] 상태 변경 시도 (이미 동일 상태): {1}", _owner.name, newState));
            return;
        }
        
        EnemyState previousState = CurrentState;
        CurrentState = newState;
        
        // Debug.Log(string.Format("[StateController : {0}] 상태 변경: {1} -> {2}", _owner.name, previousState, newState));
        
        Blackboard.SetValue(EnemyBlackboardKeys.CurrentStatus, CurrentState);
        OnStateChanged?.Invoke(previousState, newState);
    }

    public void SetLock(bool locked)
    {
        IsStateLocked = locked;
        // Debug.Log(string.Format("[StateController : {0}] State Lock: {1}", _owner.name, locked));
    }

    public void RecordStunEnd()
    {
        LastStunEndTime = Time.time;
        // Debug.Log(string.Format("[StateController : {0}] Stun End Recorded at {1}", _owner.name, LastStunEndTime));
    }
    
    public bool CanTransitionTo(EnemyState targetState)
    {
        if (IsStateLocked && targetState != EnemyState.Die && targetState != EnemyState.Stunned) return false;
        return !(CurrentState == EnemyState.Die && targetState != EnemyState.Die);
    }
}
