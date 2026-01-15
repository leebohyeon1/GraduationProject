using System;
using DG.Tweening;
using UnityEditor.Build.Pipeline;
using UnityEngine;
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
    public bool _isStunned { get; private set; } = false;
    public float StunExitTime => _stunExitTime;
    
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

    private void OnDisable()
    {
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
                return false;
            }
        }   
        if(attackType == AttackType.NormalCounter)
        {
            _owner.StiffnessSystem.AddStiffness(0);
            DeactivateImmunity();
        }
        else
        {
            DeactivateImmunity();
            _owner.StiffnessSystem.AddStiffness(100);
        }
        return true;
    }

    public void ApplyStun()
    {
        if (_isStunned || _owner.EnemyHealth.IsDead) return; // 이미 스턴 상태라면 무시
        _isStunned = true;
        _stunExitTime = Time.time + _stunTime;
        _owner.Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        _owner.animator.SetBool("Stun", true); // 스턴 애니메이션 트리거
        CurrentState = EnemyState.Stunned;
    }
    public void ApplyStun(float stunDuration)
    {
        if (_isStunned || _owner.EnemyHealth.IsDead) return; // 이미 스턴 상태라면 무시
        _isStunned = true;
        _stunExitTime = Time.time + stunDuration;
        _owner.Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        _owner.animator.SetBool("Stun", true); // 스턴 애니메이션 트리거
        CurrentState = EnemyState.Stunned;
    }
    public void ApplyWeakStun(float stunDuration)
    {
        if (_isStunned || _owner.EnemyHealth.IsDead) return; // 이미 스턴 상태라면 무시
        _isStunned = true;
        _stunExitTime = Time.time + stunDuration;
        _owner.Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        _owner.animator.SetBool("WeakStun", true); // 스턴 애니메이션 트리거
        CurrentState = EnemyState.Stunned;
    }
    public void ClearStun()
    {
        _owner.animator.SetBool("Stun", false);
        _owner.animator.SetBool("WeakStun", false);
        _isStunned = false;
        CurrentState = EnemyState.StunnedExit;
    }
    public void StateNormal()
    {
        CurrentState = EnemyState.Normal;
    }
    public void ActivateMinorImmunity()
    {
        // 소경직 면역 활성화
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
    }
}