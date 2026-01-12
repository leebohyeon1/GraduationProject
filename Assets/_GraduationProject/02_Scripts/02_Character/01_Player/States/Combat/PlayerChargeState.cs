using System.Collections;
using Unity.InferenceEngine;
using UnityEngine;

/// <summary>
/// 플레이어의 차지 상태입니다.
/// </summary>
public class PlayerChargeState : State<Player>
{
    private int _chargeLevel => p_context.Stats.ChargeLevel;
    private float _chargeTimer = 0f;

    public PlayerChargeState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        Debug.Log("Enter Charge State");
        p_context.Animator.SetBool("IsCharging", true);
        p_context.Stats.IsChage = true;

        p_context.Stats.ChargeLevel = 0;
        _chargeTimer = 0f;   
    }

    public override void OnUpdate()
    {
        _chargeTimer += Time.deltaTime;
        if(_chargeLevel < p_context.Stats.CurrentAttackData.ChargeConfigs.Count && 
            _chargeTimer >= p_context.Stats.CurrentAttackData.ChargeConfigs[_chargeLevel].ChargeTime)
        {
            p_context.Stats.ChargeLevel++;
            p_context.Events.TriggerChargeLevelFeedback(_chargeLevel);
        }
        
        if(_chargeTimer >= p_context.Stats.RuntimeData.CombatData.MaxChargeTime)
        {
            p_stateMachine.ChangeState<PlayerChargeAttackState>();
        }

        p_context.Stamina.UseStamina(p_context.Stats.RuntimeData.CombatData.ChargeStamina * Time.deltaTime);

        // 회전 처리
        if (p_context.Stats.IsLockOn)
        {
            Vector3 targetPosition = new Vector3(p_context.LockOnSystem.CurrentTarget.position.x, 0, p_context.LockOnSystem.CurrentTarget.position.z);
            Vector3 directionToTarget = (targetPosition - new Vector3(p_context.transform.position.x, 0, p_context.transform.position.z)).normalized;

            p_context.Movement.SetRotation(Quaternion.LookRotation(directionToTarget), p_context.Stats.RuntimeData.RotateSpeed);
        }
        else
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Input.MoveInput;
            var mousePosition = p_context.Input.MousePosition;
            p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition, p_context.Stats.RuntimeData.CombatData.ChargeRotateSpeed);
        }

        // 이동 처리
        if (p_context.Movement != null && p_context.Input.MoveInput != Vector2.zero)
        {
            Vector3 moveDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y).normalized;
            p_context.Movement.Move(moveDirection, p_context.Stats.RuntimeData.CombatData.ChargeMoveSpeed);
            
            Vector3 localMove = p_context.transform.InverseTransformDirection(moveDirection);
            p_context.Animator.SetFloat("X", localMove.x);
            p_context.Animator.SetFloat("Y", localMove.z);
        }
        else
        {
            p_context.Animator.SetFloat("X", 0);
            p_context.Animator.SetFloat("Y", 0);
        }


        // 입력에 따른 상태 전환
        if (!p_context.Input.AttackHeldInput)
        {
            if (_chargeLevel > 0)
            {
                p_stateMachine.ChangeState<PlayerChargeAttackState>();
            }
            else
            {
                p_context.Stats.ChargeLevel = 0;
                p_context.Stats.IsChage = false;
                p_stateMachine.ChangeState<PlayerParryState>();
            }
        }
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsCharging", false);

        p_context.Events.TriggerBattleStateChanged(true);
        p_context.Events.TriggerChargeCanceled();
    }

}