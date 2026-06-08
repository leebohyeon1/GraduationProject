using UnityEngine;
public enum StunType { None, Weak, Full, Any }

    public enum ImmunityLevel
{
    None,       // 면역 없음
    Minor,      // 소경직 면역 (기본 공격 무시)
    Major,      // 대경직/차지 공격 면역 (기본 + 차지 공격 무시)
}
public class ParrySystem : MonoBehaviour, IParryable, ICounterable
{
    // Parry system implementation
    public bool IsCounterable { get; private set; } = false;
    float _stunExitTime = -Mathf.Infinity;
    public StunType CurrentStun { get; private set; } = StunType.None;
    public bool _isStunned => CurrentStun != StunType.None;
    public float StunExitTime => _stunExitTime;
    
    // 패리 발생 추적용 플래그 (BaseAttackNode에서 사용)
    
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
        // _owner.AnimationEvent("CounterAttack"); 
        // _owner.TakeDamage(30); // 카운터 공격 시 데미지 적용 
    }
    
    public bool Parry(AttackType attackType)
    {
        if(_owner.EnemyHealth.CheckStunImmunity!= null)
        {
            if(_owner.EnemyHealth.CheckStunImmunity(attackType))
            {
                Debug.Log("[ParrySystem] 면역 상태로 인해 경직이 적용되지 않았습니다.");
                return false;
            }
        }   
        if(attackType == AttackType.Normal_Counter)
        {
            Debug.Log("[ParrySystem] 카운터 공격이 성공했습니다!");
            DeactivateImmunity();
            
        }
        else
        {
            Debug.Log("[ParrySystem] 경직이 적용되었습니다!");
            DeactivateImmunity();
        }
        return true;
    }


    public void ApplyStun(float stunDuration, bool isCounterAttack = false)
    {
        if(!isCounterAttack)
        {
            return;
        }
        if (_isStunned || _owner.EnemyHealth.IsDead) return; // 이미 스턴 상태라면 무시
        Debug.Log($"[ParrySystem] ApplyStun {_owner.name} duration={stunDuration}");
        CurrentStun = StunType.Full;
        _stunExitTime = Time.time + stunDuration; // 스턴 지속 시간 설정  
        _owner.Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        _owner.AnimationBool("Stun", true); // 스턴 애니메이션 트리거
        _owner.animHandler.PlayFeedback("Stun_FB"); // 스턴 피드백 재생
        _owner._stateController.SetState(EnemyStateController.EnemyState.Stunned);

        // _owner.SetState(EnemyStateController.EnemyState.Stunned);
    }
    public void ApplyWeakStun(float stunDuration)
    {
        if (_isStunned || _owner.EnemyHealth.IsDead) return; // 이미 스턴 상태라면 무시
        Debug.Log($"[ParrySystem] ApplyWeakStun {_owner.name} duration={stunDuration}");
        CurrentStun = StunType.Weak;
        _stunExitTime = Time.time + stunDuration;
        _owner.Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        // _owner.AnimationBool("WeakStun", true); // 스턴 애니메이션 트리거
        _owner.ParrySystem.DeactivateImmunity();
        _owner._stateController.SetState(EnemyStateController.EnemyState.Stunned);
        

        // _owner.SetState(EnemyStateController.EnemyState.Stunned);
    }
    public void ClearStun()
    {
        Debug.Log($"[ParrySystem] ClearStun {_owner.name}");
        _owner.AnimationBool("Stun", false);
        _owner.AnimationBool("WeakStun", false);
        CurrentStun = StunType.None;
        CurrentState = EnemyState.StunnedExit;
        _owner._stateController.SetLock(false);
        _owner.SetState(EnemyStateController.EnemyState.Idle);
    }
    public void StateNormal()
    {
        CurrentState = EnemyState.Normal;
    }
    public void ActivateMinorImmunity()
    {
        // 소경직 면역 활성화
        Debug.Log("[ParrySystem] 소경직 면역이 활성화되었습니다.");
        _owner.EnemyHealth.SetImmunityLevel(ImmunityLevel.Minor);
    }

    public void ActivateMajorImmunity()
    {
        // 차지 공격 면역 활성화
        _owner.EnemyHealth.SetImmunityLevel(ImmunityLevel.Major);
    }

    public void DeactivateImmunity()
    {
        // 면역 해제
        _owner.EnemyHealth.SetImmunityLevel(ImmunityLevel.None);
        Debug.Log("[ParrySystem] 면역이 해제되었습니다.:"+ _owner.EnemyHealth._currentImmunityLevel);
    }


}
