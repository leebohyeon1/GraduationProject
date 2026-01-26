using System.Collections;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어의 차지 상태입니다.
/// </summary>
public class PlayerChargeState : PlayerBaseState
{
    private int _chargeLevel => p_owner.Combat.ChargeLevel;
    private float _chargeTimer = 0f;

    public PlayerChargeState(StateMachine<PlayerController> stateMachine)
        : base(stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();

        Debug.Log("Enter Charge State");
        //p_owner.Combat.IsCharge = true;

        //p_owner.Stats.ChargeLevel = 0;
        _chargeTimer = 0f;   
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        _chargeTimer += Time.deltaTime;
        if(_chargeLevel < p_owner.Data.HeavyCounterAttackConfigList.Count && 
            _chargeTimer >= p_owner.Data.HeavyCounterAttackConfigList[_chargeLevel].ChargeTime)
        {
            // p_owner.Stats.ChargeLevel++;
            // p_context.Events.TriggerChargeLevelFeedback(_chargeLevel);
        }

        p_owner.Stamina.UseStamina(p_owner.Data.ChargeStamina * Time.deltaTime);

        //if (!p_context.Health.IsDead && p_context.Stats.IsDamaged)
        //{
        //    p_stateMachine.ChangeState<PlayerHitState>();
        //}
        //else if (_chargeTimer >= p_context.Stats.RuntimeData.CombatData.MaxChargeTime)
        //{
        //    p_stateMachine.ChangeState<PlayerChargeAttackState>();
        //}
        //else if (!p_context.Input.AttackHeldInput)  // 입력에 따른 상태 전환
        //{
        //    if (_chargeLevel > 0)
        //    {
        //        p_stateMachine.ChangeState<PlayerChargeAttackState>();
        //    }
        //    else
        //    {
        //        p_context.Stats.ChargeLevel = 0;
        //        p_context.Stats.IsCharge = false;
        //        p_stateMachine.ChangeState<PlayerNormalCounterState>();
        //    }
        //}
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        // 이동 방향 계산
        Vector3 moveDirection = p_owner.Movement.GetRelativeVectorToCamera(p_owner.InputHandler.MoveInput);

        // 이동 처리
        p_owner.Movement.Move(moveDirection, p_owner.Data.ChargeMoveSpeed, Time.fixedDeltaTime);
        Vector3 localMove = p_owner.transform.InverseTransformDirection(moveDirection);
        p_animator.SetFloat("X", localMove.x);
        p_animator.SetFloat("Y", localMove.z);

        // 회전 처리
        if (p_owner.LockOn.IsLockOn)
        {
            Vector3 targetPosition = new Vector3(p_owner.LockOn.CurrentTarget.position.x, 0, p_owner.LockOn.CurrentTarget.position.z);
            Vector3 directionToTarget = (targetPosition - new Vector3(p_owner.transform.position.x, 0, p_owner.transform.position.z)).normalized;

            p_owner.Movement.Rotate(directionToTarget, Time.fixedDeltaTime);
        }
        else
        {
            if (p_owner.InputHandler.CurrentInputDevice == InputDeviceType.Gamepad)
            {
                // 게임패드: 이동 방향으로 회전
                if (p_owner.InputHandler.MoveInput.sqrMagnitude > 0.01f)
                {
                    p_owner.Movement.Rotate(moveDirection, p_owner.Data.ChargeRotationSpeed, Time.fixedDeltaTime);
                }
            }
            else
            {
                // 키보드/마우스: 마우스 방향으로 회전
                Vector3 mouseDirection = p_owner.Movement.GetDirectionToMouse(p_owner.InputHandler.MousePosition);
                p_owner.Movement.Rotate(mouseDirection, p_owner.Data.ChargeRotationSpeed, Time.fixedDeltaTime);
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        p_owner.Events.TriggerBattleStateChanged(true);
        p_owner.Events.TriggerChargeFinshed();
    }

    #region Setup Function
    protected override void SetupStats()
    {
        base.SetupStats();
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Charge);
    }
    #endregion
}