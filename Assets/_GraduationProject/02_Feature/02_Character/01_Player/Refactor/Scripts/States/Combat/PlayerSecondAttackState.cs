using BH_Lib.FSM;
using player.Refactor;
using System;

namespace player.Refactor
{
    public class PlayerSecondAttackState : PlayerAttackBaseState
    {
        public PlayerSecondAttackState(Player context, StateMachine<Player> stateMachine)
            : base(context, stateMachine) { }

        protected override string p_animationTrigger => "SecondAttack";

        protected override Type p_nextAttackState => typeof(PlayerThirdAttackState);

        protected override PlayerAttackData p_AttackData => p_context.DataBase.RuntimeData.CombatData.AttackDatas[1];



    }
}

