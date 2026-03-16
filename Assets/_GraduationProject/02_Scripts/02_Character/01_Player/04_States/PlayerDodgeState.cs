using DG.Tweening;
using MoreMountains.Tools;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 회피 상태입니다.
/// </summary>
public class PlayerDodgeState : PlayerBaseState
{
    public PlayerDodgeState(StateMachine<PlayerController> stateMachine)
    : base(stateMachine) { }

    public override void OnEnter()
    {
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

        p_owner.Combat.ResetNormalAttackComboIndex();       // 일반 공격 콤보 순서 초기화
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

        if (p_owner.Movement.DodgeConfig.isInivicible)
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
                Vector3 dodgeDirection = p_owner.Movement.GetRelativeVectorToCamera(moveInput);

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

        if (p_owner.Movement.DodgeConfig.isInivicible)
        {
            Debug.Log("회피 종료 - 무적 해제");
            p_owner.Ability.RemoveTag(p_owner.Movement.InvincibleSO);
        }
    }
    #endregion

}