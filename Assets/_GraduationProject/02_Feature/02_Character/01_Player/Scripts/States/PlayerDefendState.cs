using UnityEngine;
using System.Threading;
using BH_Lib.FSM;
using BH_Lib.Log;


/// <summary>
/// 플레이어 방어 상태
/// 방어 키를 누르고 있을 때의 상태
/// 방어 중에는 이동 및 공격이 불가능하고, 받는 데미지가 70% 감소
/// </summary>
public class PlayerDefendState : BaseState<Player>
{
    public PlayerDefendState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnParryPerform += HandleParryPerform;

        p_context.Animator.SetBool("IsDefending", true);
        Log.Print("Player entered Defend state");


        p_context.Health.SetDefending(true);
        p_context.Combat.SetupCombatCenter();
        p_context.Events.TriggerBattleStateChanged(true);
    }

    public override void OnUpdate()
    {
        // 방어 중에는 이동하지 않음 (중력만 적용)
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);

        // 방어 키를 떼면 Idle 상태로 전환
        if (!p_context.Controller.DefendInput)
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
            p_context.Health.SetDefending(false);
        }
        else if (p_context.Controller.DodgeInput)
        {
            p_stateMachine.ChangeState<PlayerDodgeState>();
            p_context.Health.SetDefending(false);
        }
    }

    public override void OnExit()
    {
        p_context.Events.OnParryPerform -= HandleParryPerform;

        p_context.Animator.SetBool("IsDefending", false);
        p_context.Events.TriggerBattleStateChanged(true);

        Log.Print("Player exited Defend state");

        // 방어 상태 종료 시 체력 시스템에 알림
    }

    private void HandleParryPerform()
    {
        Log.Print("패리");
        Collider[] colliders = p_context.Combat.ExecuteParry(p_context.DataBase.RuntimeData.CombatData.ParryRadius);

        foreach (Collider collider in colliders)
        {
            p_context.Events.TriggerParryAffect(collider);
        }
    }
}

