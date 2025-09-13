using System;
using System.Collections.Generic;
using BH_Lib.DI;
using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어 캐릭터의 기본 클래스
/// 역할: 각 기능 모듈을 연결하고, 외부에서 접근할 수 있는 진입점 제공.
/// </summary>

[Register(LifetimeScope.Transient)]
[RequireComponent(typeof(CapsuleCollider), typeof(CharacterController), typeof(Animator))]
public class Player : CharacterBase
{
    [Header("Player Components")]
    [SerializeField] private PlayerStatsSO _playerStats;
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerMeleeAttack _playerMeleeAttack;
    [SerializeField] private PlayerRangedAttack _playerRangedAttack;
    [SerializeField] private PlayerCombat _playerCombat;
    [SerializeField] private PlayerHeat _playerHeat;
    [SerializeField] private PlayerAnimationEventHandler _playerAnimationEventHandler;
    [SerializeField] private Animator _animator;

    // 입력 기기 감지기
    [Inject] private IInputDeviceDetector _inputDeviceDetector;
    [Inject] private EventManager _event;

    public PlayerContext Context { get; private set; }
    // 상태 머신
    private StateMachine<PlayerContext> _stateMachine;

    protected override void Awake()
    {
        base.Awake();
        
        InitializeComponents();
    }

    private void Start()
    {

        InitializeStateMachine();
    }

    private void Update()
    {
        Context.Movement.Tick(); // 이동 처리

        // 상태 머신 업데이트
        _stateMachine?.Update();
    }

    private void FixedUpdate()
    {
        // 상태 머신 고정 업데이트
        _stateMachine?.FixedUpdate();
    }

    private void LateUpdate()
    {
        Context.Controller.LateTick(); // 입력 상태 리셋
    }

    private void InitializeComponents()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_playerController == null)
        {
            _playerController = GetComponent<PlayerController>();
        }

        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }

        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        if (_playerMeleeAttack == null)
        {
            _playerMeleeAttack = GetComponent<PlayerMeleeAttack>();
        }

        if (_playerRangedAttack == null)
        {
            _playerRangedAttack = GetComponent<PlayerRangedAttack>();
        }

        if(_playerCombat == null)
        {
            _playerCombat = GetComponent<PlayerCombat>();
        }

        if (_playerHeat == null)
        {
            _playerHeat = GetComponent<PlayerHeat>();
        }

        if (_playerAnimationEventHandler == null)
        {
            _playerAnimationEventHandler = GetComponent<PlayerAnimationEventHandler>();
        }

        Context = new PlayerContext(this, _playerMovement, _playerMeleeAttack, _playerRangedAttack,
        _playerCombat, _playerHealth, _playerController, _playerHeat,
        _playerStats, _animator, _inputDeviceDetector, Event);

        _playerHealth.Initialize(Context);
        _playerMovement.Initialize(Context);
        _playerController.Initialize(Context.InputDeviceDetector);
        _playerMeleeAttack.Initialize(Context);
        _playerRangedAttack.Initialize(Context);
        _playerCombat.Initialize(Context);
        _playerHeat.Initialize(Context);
        _playerAnimationEventHandler.Initialize(Context.Event);

        Context.Event.Player.OnFootstep += () => { PlayFeedbackSound("FootStep"); };
        Context.Event.Player.Dodge.OnFinished += () => { PlayFeedbackSound("DodgeEnd"); };
        Context.Event.Player.MeleeAttack.OnPerform += () => { PlayFeedbackSound("MeleeAttack"); };
        Context.Event.Player.RangedAttack.OnPerform += () => { PlayFeedbackSound("RangedAttack"); };
        Context.Event.Player.ChargeMeleeAttack.OnCharge += () => { PlayFeedbackSound("Charge"); };
        Context.Event.Player.ChargeMeleeAttack.OnStart += () => { PlayFeedbackSound("ChargeAttack"); };
        Context.Event.Player.Parry.OnAffect += (collider) => { PlayFeedbackSound("Parry"); };

    }

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine<PlayerContext>(Context);

        // 상태들 추가
        _stateMachine.AddState(new PlayerIdleState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerMoveState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerFirstMeleeAttackState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerSecondMeleeAttackState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerRangedAttackChargeState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerRangedAttackFireState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerDodgeState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerDefendState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerHitState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerMeleeAttackChargeState(Context, _stateMachine));
        _stateMachine.AddState(new PlayerChargeMeleeAttackState(Context, _stateMachine));


        // 상태 전환 조건 설정
        SetupStateTransitions();

        // 초기 상태를 Idle로 설정
        _stateMachine.ChangeState<PlayerIdleState>();
    }

    private void SetupStateTransitions()
    {
        // Hit 상태로의 전환 (모든 상태에서 가능)
        _stateMachine.AddAnyTransition<PlayerHitState>(() =>
            Context.Health.IsAlive && Context.Health.IsHit);

        // Idle 상태에서의 전환
        _stateMachine.AddTransition<PlayerIdleState, PlayerMoveState>(() => Context.Controller.MoveInput != Vector2.zero);
        _stateMachine.AddTransition<PlayerIdleState, PlayerFirstMeleeAttackState>(() => Context.Controller.AttackInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerMeleeAttackChargeState>(() => Context.Controller.AttackHeldInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerDodgeState>(() =>
            Context.Controller.DodgeInput && Context.Movement.CanDodge());
        _stateMachine.AddTransition<PlayerIdleState, PlayerDefendState>(() => Context.Controller.DefendInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerRangedAttackChargeState>(() => Context.Controller.RangedAttackInput);

        // Move 상태에서의 전환
        _stateMachine.AddTransition<PlayerMoveState, PlayerIdleState>(() => Context.Controller.MoveInput == Vector2.zero);
        _stateMachine.AddTransition<PlayerMoveState, PlayerFirstMeleeAttackState>(() => Context.Controller.AttackInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerMeleeAttackChargeState>(() => Context.Controller.AttackHeldInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerDodgeState>(() =>
            Context.Controller.DodgeInput && Context.Movement.CanDodge());
        _stateMachine.AddTransition<PlayerMoveState, PlayerDefendState>(() => Context.Controller.DefendInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerRangedAttackChargeState>(() => Context.Controller.RangedAttackInput);


        // Attack, Dodge 상태에서의 전환은 각 상태 클래스 내부에서 처리됩니다.
    }

    // 현재 상태 정보 (디버깅용)
    public IState CurrentState => _stateMachine?.CurrentState;
    public Type CurrentStateType => _stateMachine?.CurrentStateType;
    public EventManager Event => _event;

    private void OnDestroy()
    {
        Event.Player.Dispose();
    }
}
