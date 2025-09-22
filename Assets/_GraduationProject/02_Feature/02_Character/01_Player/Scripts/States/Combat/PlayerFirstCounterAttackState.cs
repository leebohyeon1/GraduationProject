using BH_Lib.FSM;
using UnityEngine;

public class PlayerFirstCounterAttackState : BaseState<Player>
{
    public PlayerFirstCounterAttackState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

}
