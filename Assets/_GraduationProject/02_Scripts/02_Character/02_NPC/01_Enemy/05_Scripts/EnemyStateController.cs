
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
        RunAway
    }
    public EnemyState CurrentState { get; private set; }
    public BlackBoard Blackboard => _owner._aiController._aiBrain.blackboard;
    
    public event Action<EnemyState, EnemyState> OnStateChanged;
    
    /// <summary> 상태 고정 여부 (BT 공격 노드 등에서 사용) </summary>
    public bool IsStateLocked { get; private set; }

    public void Initialize(Enemy owner)
    {
        _owner = owner;
        CurrentState = EnemyState.Idle;
        IsStateLocked = false;
    }
    
    public void SetState(EnemyState newState)
    {
        // 상태가 고정되어 있고, 죽는 상태가 아니면 무시
        if (IsStateLocked && newState != EnemyState.Die)
        {
            // Debug.Log("[EnemyStateController] State is LOCKED. Ignoring transition to " + newState);
            return;
        }

        if (CurrentState == newState) return;
        
        EnemyState previousState = CurrentState;
        CurrentState = newState;
        
        // 블랙보드 동기화는 이 컨트롤러가 담당
        Blackboard.SetValue(EnemyBlackboardKeys.CurrentStatus, CurrentState);
        
        // 이벤트로 상태 변경 알림
        OnStateChanged?.Invoke(previousState, newState);
    }

    /// <summary>
    /// 상태 전환을 강제로 고정하거나 해제합니다.
    /// </summary>
    public void SetLock(bool locked)
    {
        IsStateLocked = locked;
        // Debug.Log("[EnemyStateController] State Lock: " + locked);
    }
    
    public bool CanTransitionTo(EnemyState targetState)
    {
        if (IsStateLocked && targetState != EnemyState.Die) return false;
        return !(CurrentState == EnemyState.Die && targetState != EnemyState.Die);
    }
}
