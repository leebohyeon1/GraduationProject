using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 회피 상태입니다.
/// </summary>
public class PlayerDodgeState : PlayerBaseState
{
    private Vector3 _dodgeDirection; // 회피 방향

    public PlayerDodgeState(StateMachine<PlayerController> stateMachine)
    : base(stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();

        // 입력 방향에 따라 회피 방향 결정
        if (p_owner.InputHandler.MoveInput == Vector3.zero)
        {
            _dodgeDirection = p_owner.transform.forward;
        }
        else
        {
            _dodgeDirection = new Vector3(p_owner.InputHandler.MoveInput.x, 0, p_owner.InputHandler.MoveInput.y).normalized;
        }

        p_owner.Movement.Rotate(_dodgeDirection, 1);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
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
        p_owner.Events.DodgeFinished += OnDodgeFinished;
    }

    protected override void SetupStats()
    {
        base.SetupStats();

        // p_owner.Health.IncreaseDamageReduction(_dodgeTagSO.DamageReduction);
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Dodge);   // 애니메이션 상태 설정
        p_animator.Play("Roll", 0, 0f);                                    // 0부터 재생
    }
    #endregion

    #region Clear Function
    protected override void ClearEvents()
    {
        base.ClearEvents();

        p_owner.Events.DodgeStarted -= OnDodgeStarted;
        p_owner.Events.DodgeFinished -= OnDodgeFinished;

        DOTween.Kill(this);
    }

    protected override void ClearStats()
    {
        base.ClearStats();

        // 전투 상태일 때 구르기만 전투 상태 유지
        if (p_owner.Combat.IsBattleState)
        {
            p_owner.Events.TriggerBattleStateChanged(true);
        }

        // p_owner.Stats.DecreaseDamageReduction(_dodgeTagSO.DamageReduction);
    }

    protected override void ClearAnimator()
    {
        base.ClearAnimator();

        p_animator.SetInteger("DodgeType", -1);
    }
    #endregion

    #region EventHandle
    public void OnDodgeStarted()
    {
        // 전투 상태일 때 구르기만 전투 상태 유지
        if (p_owner.Combat.IsBattleState)
        {
            p_owner.Events.TriggerBattleStateChanged(true);
        }

        Roll();
    }

    /// <summary>
    /// 회피 애니메이션 종료 시 호출됩니다.
    /// </summary>
    public void OnDodgeFinished()
    {
        p_stateMachine.ChangeState<PlayerIdleState>();
    }
    #endregion

    private void Roll()
    {
        // 구르기 시작
        StepData dodgeData = p_owner.Data.DodgeConfig;
        float currentDistance = 0f;

        DOTween.To(
            () => currentDistance,
            x =>
            {
                // 카메라 방향 기준 벡터 반환
                Vector3 moveInput = p_owner.InputHandler.MoveInput;
                Vector3 dodgeDirection = p_owner.Movement.GetRelativeVectorToCamera(moveInput);

                float deltaDistance = x - currentDistance;
                Vector3 displacement = p_owner.Movement.transform.forward * deltaDistance;

                p_owner.Movement.Rotate(dodgeDirection, dodgeData.StepRotateSpeed, Time.fixedDeltaTime);

                // 캐릭터 컨트롤러 이동
                p_owner.Movement.CharacterControllerMove(displacement, 1);

                currentDistance = x;
            },
            dodgeData.StepDistance,
            dodgeData.StepDuration)
            .SetEase(dodgeData.StepCurve)
            .SetId(this)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(p_owner.Events.TriggerDodgeFinished);
    }
}