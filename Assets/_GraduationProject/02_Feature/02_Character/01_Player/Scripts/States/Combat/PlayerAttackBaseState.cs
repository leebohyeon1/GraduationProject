
using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;


public abstract class PlayerAttackBaseState : BaseState<Player>
{
    /// <summary>
    /// 다음 상태를 저장할 변수
    /// </summary>
    protected Type p_nextState;

    /// <summary>
    /// 애니메이션 트리거 이름 (하위 클래스에서 구현)
    /// </summary>    
    protected abstract string p_animationTrigger { get; }
    /// <summary>
    /// 다음 공격 상태 타입 (하위 클래스에서 구현)
    /// </summary>
    protected abstract Type p_nextAttackState { get; }
    /// <summary>
    /// 플레이어 공격 데이터
    /// </summary>
    protected abstract PlayerAttackData p_AttackData { get; }

    /// <summary>
    /// 플레이어 공격 기본 상태 생성자
    /// </summary>
    public PlayerAttackBaseState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;

        p_nextState = null; // 다음 상태 초기화

        Log.Print("Player entered Attack state");
        p_context.Animator.SetTrigger(p_animationTrigger);  // 공격 애니메이션 실행
        p_context.Combat.SetupCombatCenter();

        // 공격 실행
        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Controller.MoveInput;
        var mousePosition = p_context.Controller.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);
        p_context.Events.TriggerBattleStateChanged(true);
          

        // 공격 시 전진 이동 실행
        StartAttackMovement();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        HandleInput();
    }

    public override void OnExit()
    {
        p_context.Events.OnAttackFinish -= HandleAttackFinish;
        p_context.Events.OnAttackPerform -= HandleAttackPerform;

        p_context.Animator.ResetTrigger(p_animationTrigger);
        p_context.Events.TriggerBattleStateChanged(true);

        DOTween.Kill(p_animationTrigger);



        p_nextState = null;
        Log.Print("Player exited Attack state");
    }

    /// <summary>
    /// 공격 애니메이션 이벤트 핸들러
    /// 공격이 완료되면 다른 상태로 전환
    /// </summary>
    protected virtual void HandleAttackFinish()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(p_AttackData.AttackDelay);

        // 연속 공격이 아니면 추가 딜레이
        if (!typeof(PlayerAttackBaseState).IsAssignableFrom(p_nextState))
        { 
            sequence.SetDelay(p_context.DataBase.RuntimeData.CombatData.LastAttackDelay);
        }

        sequence.AppendCallback(() =>
        {
            if (p_nextState != null)
            {
                p_stateMachine.ChangeState(p_nextState);
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        });
    }

    /// <summary>
    /// 공격 효과 적용
    /// </summary>
    protected virtual void HandleAttackPerform()
    {
        Log.Print("공격");
        Collider[] colliders = p_context.Combat.ExecuteAttack(p_AttackData);

        foreach (Collider collider in colliders)
        {
            p_context.Events.TriggerAttackAffect(collider);
        }
    }

    /// <summary>
    /// 공격 시 전진 이동 시작
    /// </summary>
    protected virtual void StartAttackMovement()
    {
        float distance = p_AttackData.AttackMoveDistance;

        // 전방에 오브젝트가 있을 경우 전진 거리 조정
        if (Physics.Raycast(p_context.transform.position, p_context.transform.forward, 
            out var hitInfo, p_AttackData.AttackMoveDistance))
        {
            distance = hitInfo.distance - (p_context.GetComponent<Collider>().bounds.size.z / 2);
        }

        Vector3 targetPosition = p_context.transform.position + (p_context.transform.forward * distance);

        p_context.transform.DOMove(targetPosition, p_AttackData.AttackMoveDuration, false)
        .SetEase(p_AttackData.AttackMoveCurve).SetId(p_animationTrigger);
    }

    /// <summary>
    /// 입력 처리
    /// 회피 중 입력을 감지하여 다음 상태를 결정
    /// </summary>
    public void HandleInput()
    {
        if (p_nextAttackState != null && p_context.Controller.AttackInput)
        {
            p_nextState = p_nextAttackState;
        }
        else if (p_context.Controller.DodgeInput &&
            Time.time - p_context.Movement.LastDodgeTime >=
            p_context.DataBase.RuntimeData.CombatData.DodgeCooldown)
        {
            p_nextState = typeof(PlayerDodgeState);
        }
        else if (p_context.Controller.DefendInput)
        {
            p_nextState = typeof(PlayerDefendState);
        }
        else if (p_context.Combat.CanCounterAttack && p_context.Controller.AttackInput)
        {
           // p_nextState = typeof(PlayerCounterAttackState);
        }
        else if (p_context.Controller.AttackHeldInput)
        {
            p_nextState = typeof(PlayerChargeState);
        }
        else if (p_context.Controller.RangedAttackInput)
        {
            p_nextState = typeof(PlayerRangedChargeState);
        }
        else if (p_context.Controller.SkillInput)
        {
           // p_nextState = typeof(PlayerSkillState);
        }

        if (p_nextState != null)
        {
            Log.PrintColor(Color.skyBlue, $"[PlayerAttackBaseState] 다음 상태: {p_nextState}");
        }
    }

}
