using DG.Tweening;
using MoreMountains.Tools;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 회피 상태입니다.
/// </summary>
public class PlayerDodgeState : PlayerBaseState
{
    public PlayerDodgeState(StateMachine<PlayerController> stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        // 1. 물리 및 전투 로직 강제 중단
        p_owner.Combat.CancelAttack();    // 공격 히트박스 비활성화 및 관련 로직 중단

        // 2. 애니메이션 파라미터 청소
        p_animator.ResetTrigger("Attack"); // 공격 예약 트리거 제거
        p_animator.ResetTrigger("Counter"); 

        base.OnEnter();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // 카메라 방향 기준 벡터 반환
        Vector3 moveInput = p_owner.InputHandler.MoveInput;

        // 입력이 있을 때만 회전 처리
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 dodgeDirection = p_owner.Movement.GetRelativeVectorToCamera(moveInput);

            // 회전은 따로 처리
            p_owner.Movement.Rotate(dodgeDirection, p_owner.Movement.DodgeConfig.MoveConfig.StepRotateSpeed, Time.fixedDeltaTime);
        }
    }

    public override void OnExit()
    {
        base.OnExit();

    }

    #region Setup Function
    protected override void SetupEvents()
    {
        base.SetupEvents();

        p_owner.Events.DodgeStarted += OnDodgeStarted;
    }

    protected override void SetupStats()
    {
        base.SetupStats();

        // 스테미나 소모 (필요 시 데이터에서 가져오도록 수정 가능)
        p_owner.Stamina.UseStamina(p_owner.Movement.DodgeConfig.StaminaConsumption.Value);

        // [강화] 모든 전투 상태 리셋
        p_owner.Combat.CancelAttack();

        // [추가] 선입력 시스템 초기화 (대시 이후 이전 입력 실행 방지)
        p_owner.Events.TriggerBufferInputEnded(); 
    }
    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Dodge);       // 애니메이션 상태 설정
        p_animator.SetInteger("DodgeType", (int)p_owner.Movement.DodgeConfig.Type); // 회피 타입 설정

        p_animator.Play(p_owner.Movement.DodgeConfig.AnimationStateName, 0, 0f);    // 0부터 재생
    }
    #endregion

    #region Clear Function
    protected override void ClearEvents()
    {
        base.ClearEvents();

        p_owner.Events.DodgeStarted -= OnDodgeStarted;

        DOTween.Kill(this);
    }

    protected override void ClearStats()
    {
        base.ClearStats();

        // 전투 상태일 때 구르기만 전투 상태 유지
        if (p_owner.Combat.IsBattleState)
        {
            p_owner.Combat.TriggerBattleStateChanged(true);
        }

        p_owner.Events.TriggerDodgeFinished();
    }

    protected override void ClearAnimator()
    {
        base.ClearAnimator();

        p_animator.SetInteger("DodgeType", -1);
    }
    #endregion

    #region InputEventHandle
    protected override void OnDodge()
    {
        // 회피 중에는 추가 회피 입력을 무시하여 애니메이션 꼬임 방지
        return;
    }
    #endregion

    #region EventHandle
    public void OnDodgeStarted()
    {
        // 전투 상태일 때 구르기만 전투 상태 유지
        if (p_owner.Combat.IsBattleState)
        {
            p_owner.Combat.TriggerBattleStateChanged(true);
        }

        if (p_owner.Movement.DodgeConfig.IsInvincible)
        {
            p_owner.Ability.AddTag(p_owner.Movement.InvincibleSO);
        }

        switch(p_owner.Movement.DodgeConfig.Type)
        {
            case DodgeData.DodgeType.Roll:
                p_owner.Movement.Roll(this, 
                    () => 
                    {
                        OnDodgeFinished();
                        p_stateMachine.ChangeState<PlayerIdleState>(); 
                    });
                break;
            case DodgeData.DodgeType.Step:
                Vector3 moveInput = p_owner.InputHandler.MoveInput;
                Vector3 dodgeDirection;
                if (moveInput == Vector3.zero)
                {
                    dodgeDirection = p_owner.transform.forward;
                }
                else
                {
                    dodgeDirection = p_owner.Movement.GetRelativeVectorToCamera(moveInput);
                }

                p_owner.Movement.Step(dodgeDirection, this, false, 
                    () =>
                    {
                        OnDodgeFinished();
                        p_stateMachine.ChangeState<PlayerIdleState>();
                    });
                break;
        }
    }

    /// <summary>
    /// 회피 애니메이션 종료 시 호출됩니다.
    /// </summary>
    public void OnDodgeFinished()
    {
        p_owner.Movement.SetLastDodgeEndTime(); // 쿨타임 타이머 시작

        if (p_owner.Movement.DodgeConfig.IsInvincible)
        {
            p_owner.Ability.RemoveTag(p_owner.Movement.InvincibleSO);
        }
    }
    #endregion

}