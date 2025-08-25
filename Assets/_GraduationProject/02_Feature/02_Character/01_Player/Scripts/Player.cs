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
[RequireComponent(typeof(PlayerHealth), typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovement), typeof(PlayerAttack))]
[RequireComponent(typeof(CapsuleCollider), typeof(CharacterController), typeof(Animator))]
public class Player : CharacterBase
{
    [Header("Player Components")]
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private PlayerAnimationEventHandler _playerAnimationEventHandler;
    [SerializeField] private Animator _animator;

    // 입력 기기 감지기
    [Inject] private IInputDeviceDetector _inputDeviceDetector;

    // 상태 머신
    private StateMachine<Player> _stateMachine;

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
        PlayerMovement.Tick(); // 이동 처리

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
        PlayerController.LateTick(); // 입력 상태 리셋
    }

    private void InitializeComponents()
    {
        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }

        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        if (_playerController == null)
        {
            _playerController = GetComponent<PlayerController>();
        }

        if (_playerAttack == null)
        {
            _playerAttack = GetComponent<PlayerAttack>();
        }

        if (_playerAnimationEventHandler == null)
        {
            _playerAnimationEventHandler = GetComponent<PlayerAnimationEventHandler>();
        }

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _playerHealth.Initialize(this);
        _playerMovement.Initialize(this);
        _playerController.Initialize(this);
        _playerAttack.Initialize(this);
        _playerAnimationEventHandler.Initialize(this);
    }

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine<Player>(this);

        // 상태들 추가
        _stateMachine.AddState(new PlayerIdleState(this, _stateMachine));
        _stateMachine.AddState(new PlayerMoveState(this, _stateMachine));
        _stateMachine.AddState(new PlayerFirstAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerSecondAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerDodgeState(this, _stateMachine));

        // 상태 전환 조건 설정
        SetupStateTransitions();

        // 초기 상태를 Idle로 설정
        _stateMachine.ChangeState<PlayerIdleState>();
    }

    private void SetupStateTransitions()
    {
        // Idle 상태에서의 전환
        _stateMachine.AddTransition<PlayerIdleState, PlayerMoveState>(() => PlayerController.MoveInput != Vector2.zero);
        _stateMachine.AddTransition<PlayerIdleState, PlayerFirstAttackState>(() => PlayerController.AttackInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerDodgeState>(() =>
            PlayerController.DodgeInput && PlayerMovement.CanDodge());

        // Move 상태에서의 전환
        _stateMachine.AddTransition<PlayerMoveState, PlayerIdleState>(() => PlayerController.MoveInput == Vector2.zero);
        _stateMachine.AddTransition<PlayerMoveState, PlayerFirstAttackState>(() => PlayerController.AttackInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerDodgeState>(() =>
            PlayerController.DodgeInput && PlayerMovement.CanDodge());
            
        // Attack 상태에서의 전환은 상태 내부에서 시간 기반으로 처리
        // (공격 지속시간이 끝나면 자동으로 전환)

        // Dodge 상태에서의 전환도 상태 내부에서 시간 기반으로 처리
        // (회피 지속시간이 끝나면 자동으로 전환)
    }

    // 공개 프로퍼티들
    public PlayerStats PlayerStats => _playerStats;
    public PlayerHealth PlayerHealth => _playerHealth;
    public PlayerMovement PlayerMovement => _playerMovement;
    public PlayerController PlayerController => _playerController;
    public PlayerAttack PlayerAttack => _playerAttack;
    public PlayerAnimationEventHandler PlayerAnimationEventHandler => _playerAnimationEventHandler;
    public Animator PlayerAnimator => _animator;

    // 현재 입력 기기 정보
    public IInputDeviceDetector InputDeviceDetector => _inputDeviceDetector;
    
    // 현재 상태 정보 (디버깅용)
    public IState CurrentState => _stateMachine?.CurrentState;
    public Type CurrentStateType => _stateMachine?.CurrentStateType;
    

}
