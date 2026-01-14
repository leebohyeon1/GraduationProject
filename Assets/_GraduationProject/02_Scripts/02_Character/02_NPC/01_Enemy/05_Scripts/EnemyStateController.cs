
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
        Noise,
        Die,
        Stunned, // 스턴 상태 추가
        Rush,
        Hit,
        RunAway
    }
    public EnemyState CurrentState { get; private set; }
    public BlackBoard Blackboard => _owner._aiController._aiBrain.blackboard;
    
    public event Action<EnemyState, EnemyState> OnStateChanged;
    
    public void Initialize(Enemy owner)
    {
        _owner = owner;
        CurrentState = EnemyState.Idle;
    }
    
    public void SetState(EnemyState newState)
    {
        if (CurrentState == newState) return;
        
        EnemyState previousState = CurrentState;
        CurrentState = newState;
        
        // 블랙보드 동기화는 이 컨트롤러가 담당
        Blackboard.SetValue(EnemyBlackboardKeys.CurrentStatus, CurrentState);
        
        // 이벤트로 상태 변경 알림
        OnStateChanged?.Invoke(previousState, newState);
    }
    
    public bool CanTransitionTo(EnemyState targetState)
    {
        // 상태 전환 규칙 정의
        return !(CurrentState == EnemyState.Die && targetState != EnemyState.Die);
    }
}
