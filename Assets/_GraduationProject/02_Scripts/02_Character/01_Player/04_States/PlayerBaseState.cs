using UnityEngine;

/// <summary>
/// 플레이어의 기본 상태를 정의하는 클래스
/// </summary>
public abstract class PlayerBaseState : IState
{
    /// <summary>
    /// 플레이어 애니메이터 상태 enum
    /// </summary>
    public enum AnimatorState
    {
        Idle = 0,
        Move = 1,
        Dodge = 2,
        NormalAttack = 3,
        NormalCounterAttack = 4,
        Charge = 5,
        ChargeCounterCounterAttack = 6,

        Damaged = -1
    }

    protected StateMachine<PlayerController> p_stateMachine;            // 상태를 관리하는 상태 머신
    protected PlayerController p_owner => p_stateMachine.GetContext();  // 플레이어 기본 상태를 소유한 클래스 변수
    protected Animator p_animator;

    protected const string p_stateParamter = "State";

    public PlayerBaseState(StateMachine<PlayerController> stateMachine)
    { 
        p_stateMachine = stateMachine;

        if(p_stateMachine.GetContext().TryGetComponent<Animator>(out p_animator))
        {
        } 
    }

    // 상태가 시작할 때 호출
    public virtual void OnEnter()
    {
        SetupEvents();
        SetupStats();
        SetupAnimator();
    }

    // 매 프레임마다 호출
    public virtual void OnUpdate()
    {

    }

    // 물리 시간마다 호출
    public virtual void OnFixedUpdate()
    {

    }

    // 상태가 끝날 때 호출
    public virtual void OnExit()
    {
        ClearEvents();
        ClearStats();
        ClearAnimator();
    }

    #region Setup Function
    /// <summary>
    /// 이벤트 설정 함수
    /// </summary>
    protected virtual void SetupEvents()
    {
        Debug.Log(111);
        p_owner.InputReader.MoveEvent += OnMove;
        p_owner.InputReader.MousePositionEvent += OnMousePosition;

        p_owner.InputReader.DodgeEvent += OnDodge;
        p_owner.InputReader.NormalAttackEvent += OnAttack;
        p_owner.InputReader.NormalCounterEvent += OnNormalCounter;
        p_owner.InputReader.ChargeStartEvent += OnChargeStart;
    }
    /// <summary>
    /// 능력치 설정 함수
    /// </summary>
    protected virtual void SetupStats()
    {

    }
    /// <summary>
    /// 애니메이터 설정 함수
    /// </summary>
    protected virtual void SetupAnimator()
    {

    }
    #endregion

    #region Clear Function
    /// <summary>
    /// 이벤트 해제 함수
    /// </summary>
    protected virtual void ClearEvents()
    {
        p_owner.InputReader.MoveEvent -= OnMove;
        p_owner.InputReader.MousePositionEvent -= OnMousePosition;

        p_owner.InputReader.DodgeEvent -= OnDodge;
        p_owner.InputReader.NormalAttackEvent -= OnAttack;
        p_owner.InputReader.NormalCounterEvent -= OnNormalCounter;
        p_owner.InputReader.ChargeStartEvent -= OnChargeStart;
    }
    /// <summary>
    /// 능력치 해제 함수
    /// </summary>
    protected virtual void ClearStats()
    {

    }
    /// <summary>
    /// 애니메이터 해제 함수
    /// </summary>
    protected virtual void ClearAnimator()
    {

    }

    #endregion

    #region InputEventHandle
    /// <summary>
    /// 이동 입력 처리
    /// </summary>
    /// <param name="vector2">이동 방향</param>
    protected virtual void OnMove(Vector2 vector2) { }

    /// <summary>
    /// 마우스 위치 입력 처리
    /// </summary>
    /// <param name="vector2">마우스 위치</param>
    protected virtual void OnMousePosition(Vector2 vector2) { }   

    /// <summary>
    /// 회피 입력 처리
    /// </summary>
    protected virtual void OnDodge() { }

    /// <summary>
    /// 공격 입력 처리
    /// </summary>
    protected virtual void OnAttack() 
    {
        // 일반 공격이 가능하지 않으면 리턴
        if (!p_owner.Combat.CanNormalAttack())
        {
            return;
        }
    }
    
    /// <summary>
    /// 일반 상쇄 입력 처리
    /// </summary>
    protected virtual void OnNormalCounter() { }

    /// <summary>
    /// 차지 시작 입력 처리
    /// </summary>
    protected virtual void OnChargeStart() { }    
    #endregion
}

