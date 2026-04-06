using UnityEngine;

public class PlayerDraggedState : PlayerBaseState
{
    public PlayerDraggedState(StateMachine<PlayerController> machine) 
        : base(machine) 
    {
        p_owner.Movement.Dragged += OnDragged;
    }

    ~PlayerDraggedState()
    {
        p_owner.Movement.Dragged -= OnDragged;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("드래그");
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("드래그 종료");
    }

    /// <summary>
    /// 드래그 이벤트 처리 함수
    /// </summary>
    /// <param name="isDrag">드래그 했는가</param>
    private void OnDragged(bool isDrag)
    {
        if (isDrag)
        {
            p_stateMachine.ChangeState<PlayerDraggedState>();
            p_owner.Ability.AddTag(p_owner.Movement.DragSuperArmorSO);
        }
        else
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
            p_owner.Ability.RemoveTag(p_owner.Movement.DragSuperArmorSO);
        }
    }
}
