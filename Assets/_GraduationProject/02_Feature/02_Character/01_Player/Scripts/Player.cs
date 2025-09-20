using System;
using System.Collections.Generic;
using BH_Lib.DI;
using BH_Lib.FSM;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 플레이어 캐릭터의 기본 클래스
/// 역할: 각 기능 모듈을 연결하고, 외부에서 접근할 수 있는 진입점 제공.
/// </summary>

[Register(LifetimeScope.Transient)]
[RequireComponent(typeof(CharacterController), typeof(Animator))]
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
    // 플레이어 컨텍스트
    public PlayerContext Context { get; private set; }
    // 상태 머신
    private StateMachine<PlayerContext> _stateMachine;


    // 현재 상태 정보 (디버깅용)
    public IState CurrentState => _stateMachine?.CurrentState;
    public Type CurrentStateType => _stateMachine?.CurrentStateType;
    public PlayerEventChannel PlayerEvent => Context.Event;

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

        if (_playerCombat == null)
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
        _playerStats, _animator, _inputDeviceDetector);

        _playerHealth.Initialize(Context);
        _playerMovement.Initialize(Context);
        _playerController.Initialize(Context.InputDeviceDetector);
        _playerMeleeAttack.Initialize(Context);
        _playerRangedAttack.Initialize(Context);
        _playerCombat.Initialize(Context);
        _playerHeat.Initialize(Context);
        _playerAnimationEventHandler.Initialize(Context);

        InitializeFeedbacks();

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
        _stateMachine.AddState(new PlayerCounterAttackState(Context, _stateMachine));


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
        _stateMachine.AddTransition<PlayerIdleState, PlayerFirstMeleeAttackState>(() => !Context.Combat.CanCounterAttack && Context.Controller.AttackInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerMeleeAttackChargeState>(() => Context.Controller.AttackHeldInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerDodgeState>(() =>
            Context.Controller.DodgeInput && Context.Movement.CanDodge());
        _stateMachine.AddTransition<PlayerIdleState, PlayerDefendState>(() => Context.Controller.DefendInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerRangedAttackChargeState>(() => Context.Controller.RangedAttackInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerCounterAttackState>(() =>
            Context.Combat.CanCounterAttack && Context.Controller.AttackInput);

        // Move 상태에서의 전환
        _stateMachine.AddTransition<PlayerMoveState, PlayerIdleState>(() => Context.Controller.MoveInput == Vector2.zero);
        _stateMachine.AddTransition<PlayerMoveState, PlayerFirstMeleeAttackState>(() => !Context.Combat.CanCounterAttack && Context.Controller.AttackInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerMeleeAttackChargeState>(() => Context.Controller.AttackHeldInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerDodgeState>(() =>
            Context.Controller.DodgeInput && Context.Movement.CanDodge());
        _stateMachine.AddTransition<PlayerMoveState, PlayerDefendState>(() => Context.Controller.DefendInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerRangedAttackChargeState>(() => Context.Controller.RangedAttackInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerCounterAttackState>(() =>
            Context.Combat.CanCounterAttack && Context.Controller.AttackInput);

        _stateMachine.AddTransition<PlayerDefendState, PlayerCounterAttackState>(() =>
            Context.Combat.CanCounterAttack && Context.Controller.AttackInput);



        // Attack, Dodge 상태에서의 전환은 각 상태 클래스 내부에서 처리됩니다.
    }

    private void InitializeFeedbacks()
    {
        /// 움직임 관련 피드백
        Context.Event.OnFootstep += HandleFootstep;
        Context.Event.OnMoveStop += HandleMoveStop;
        Context.Event.Dodge.OnStart += HandleDodgeStart;
        Context.Event.Dodge.OnFinished += HandleDodgeFinished;
        Context.Event.OnLand += HandleLand;

        /// 피격 관련 피드백
        Context.Event.OnNomalHit += HandleNomalHit;
        Context.Event.OnStrongHit += HandleStrongHit;
        Context.Event.OnDefendHit += HandleDefendHit;

        /// 근접 공격 관련 피드백
        Context.Event.OnFirstMeleeAttackEffect += HandleFirstMeleeAttackEffect;
        Context.Event.OnSecondMeleeAttackEffect += HandleSecondMeleeAttackEffect;
        Context.Event.OnThirdMeleeAttackEffect += HandleThirdMeleeAttackEffect;
        Context.Event.MeleeAttack.OnAffect += HandleMeleeAttackAffect;

        /// 차징 공격 관련 피드백
        Context.Event.MeleeAttackCharge.OnStart += HandleMeleeAttackChargeStart;
        Context.Event.MeleeAttackCharge.OnCancel += HandleMeleeAttackChargeCancel;
        Context.Event.MeleeAttackCharge.OnFinished += HandleMeleeAttackChargeFinished;
        Context.Event.ChargeMeleeAttack.OnStart += HandleChargeMeleeAttackStart;
        Context.Event.ChargeMeleeAttack.OnFinished += HandleChargeMeleeAttackFinished;
        Context.Event.OnTier1ChargeAttackEffect += HandleTier1ChargeAttackEffect;
        Context.Event.OnTier2ChargeAttackEffect += HandleTier2ChargeAttackEffect;
        Context.Event.OnTier3ChargeAttackEffect += HandleTier3ChargeAttackEffect;

        /// 원거리 공격 관련 피드백
        Context.Event.RangedAttackCharge.OnStart += HandleRangedAttackChargeStart;
        Context.Event.RangedAttackCharge.OnPerform += HandleRangedAttackChargePerform;
        Context.Event.RangedAttackCharge.OnCancel += HandleRangedAttackChargeCancel;
        Context.Event.RangedAttackCharge.OnFinished += HandleRangedAttackChargeFinished;
        Context.Event.RangedAttack.OnStart += HandleRangedAttackStart;
        Context.Event.RangedAttack.OnAffect += HandleRangedAttackAffect;

        /// 패리/카운터 공격 관련 피드백
        Context.Event.Parry.OnStart += HandleParryStart;
        Context.Event.Parry.OnAffect += HandleParryAffect;
        Context.Event.CounterAttack.OnStart += HandleCounterAttackStart;
        Context.Event.OnTier1FirstCounterAttackEffect += HandleTier1FirstCounterAttackEffect;
        Context.Event.OnTier2FirstCounterAttackEffect += HandleTier2FirstCounterAttackEffect;
        Context.Event.OnTier3FirstCounterAttackEffect += HandleTier3FirstCounterAttackEffect;
        Context.Event.OnTier1SecondCounterAttackEffect += HandleTier1SecondCounterAttackEffect;
        Context.Event.OnTier2SecondCounterAttackEffect += HandleTier2SecondCounterAttackEffect;
        Context.Event.OnTier3SecondCounterAttackEffect += HandleTier3SecondCounterAttackEffect;
        Context.Event.CounterAttack.OnFinished += HandleCounterAttackFinished;
    }

    private void OnDisable()
    {
        /// 움직임 관련 피드백
        Context.Event.OnFootstep -= HandleFootstep;
        Context.Event.OnMoveStop -= HandleMoveStop;
        Context.Event.Dodge.OnStart -= HandleDodgeStart;
        Context.Event.Dodge.OnFinished -= HandleDodgeFinished;
        Context.Event.OnLand -= HandleLand;

        /// 피격 관련 피드백
        Context.Event.OnNomalHit -= HandleNomalHit;
        Context.Event.OnStrongHit -= HandleStrongHit;
        Context.Event.OnDefendHit -= HandleDefendHit;

        /// 근접 공격 관련 피드백
        Context.Event.OnFirstMeleeAttackEffect -= HandleFirstMeleeAttackEffect;
        Context.Event.OnSecondMeleeAttackEffect -= HandleSecondMeleeAttackEffect;
        Context.Event.OnThirdMeleeAttackEffect -= HandleThirdMeleeAttackEffect;
        Context.Event.MeleeAttack.OnAffect -= HandleMeleeAttackAffect;

        /// 차징 공격 관련 피드백
        Context.Event.MeleeAttackCharge.OnStart -= HandleMeleeAttackChargeStart;
        Context.Event.MeleeAttackCharge.OnCancel -= HandleMeleeAttackChargeCancel;
        Context.Event.MeleeAttackCharge.OnFinished -= HandleMeleeAttackChargeFinished;
        Context.Event.ChargeMeleeAttack.OnStart -= HandleChargeMeleeAttackStart;
        Context.Event.ChargeMeleeAttack.OnFinished -= HandleChargeMeleeAttackFinished;
        Context.Event.OnTier1ChargeAttackEffect -= HandleTier1ChargeAttackEffect;
        Context.Event.OnTier2ChargeAttackEffect -= HandleTier2ChargeAttackEffect;
        Context.Event.OnTier3ChargeAttackEffect -= HandleTier3ChargeAttackEffect;

        /// 원거리 공격 관련 피드백
        Context.Event.RangedAttackCharge.OnStart -= HandleRangedAttackChargeStart;
        Context.Event.RangedAttackCharge.OnPerform -= HandleRangedAttackChargePerform;
        Context.Event.RangedAttackCharge.OnCancel -= HandleRangedAttackChargeCancel;
        Context.Event.RangedAttackCharge.OnFinished -= HandleRangedAttackChargeFinished;
        Context.Event.RangedAttack.OnStart -= HandleRangedAttackStart;
        Context.Event.RangedAttack.OnAffect -= HandleRangedAttackAffect;

        /// 패리/카운터 공격 관련 피드백
        Context.Event.Parry.OnStart -= HandleParryStart;
        Context.Event.Parry.OnAffect -= HandleParryAffect;
        Context.Event.CounterAttack.OnStart -= HandleCounterAttackStart;
        Context.Event.OnTier1FirstCounterAttackEffect -= HandleTier1FirstCounterAttackEffect;
        Context.Event.OnTier2FirstCounterAttackEffect -= HandleTier2FirstCounterAttackEffect;
        Context.Event.OnTier3FirstCounterAttackEffect -= HandleTier3FirstCounterAttackEffect;
        Context.Event.OnTier1SecondCounterAttackEffect -= HandleTier1SecondCounterAttackEffect;
        Context.Event.OnTier2SecondCounterAttackEffect -= HandleTier2SecondCounterAttackEffect;
        Context.Event.OnTier3SecondCounterAttackEffect -= HandleTier3SecondCounterAttackEffect;
        Context.Event.CounterAttack.OnFinished -= HandleCounterAttackFinished;
    }
    
    #region Feedback Handlers

    private void HandleDodgeStart(Vector3 position)
    {
        PlayFeedback("DodgeStart_FB", position);
    }

    private void HandleMoveStop()
    {
        PlayFeedbackSound("MoveStop_FB");
    }

    private void HandleFootstep()
    {
        PlayFeedbackSound("Move_FB");
    }


    private void HandleDodgeFinished(Vector3 position)
    {
        PlayFeedback("DodgeFinish_FB", position);
    }

    private void HandleLand()
    {
        PlayFeedbackSound("Landing_FB");
    }

    private void HandleNomalHit()
    {
        PlayFeedbackSound("TakeDamage_Nomal_FB");
    }

    private void HandleStrongHit()
    {
        PlayFeedbackSound("TakeDamage_Strong_FB");
    }

    private void HandleDefendHit()
    {
        PlayFeedbackSound("TakeDamage_Defend_FB");
    }

    private void HandleFirstMeleeAttackEffect(Vector3 position)
    {
        PlayFeedback("FirstAttackStart_FB", position);
    }

    private void HandleSecondMeleeAttackEffect(Vector3 position)
    {
        PlayFeedback("SecondAttackStart_FB", position);
    }

    private void HandleThirdMeleeAttackEffect(Vector3 position)
    {
        PlayFeedback("ThirdAttackStart_FB", position);
    }

    private void HandleMeleeAttackAffect(Vector3 position, Collider collider)
    {
        PlayFeedback("MeleeAttackHit_FB", position);
    }

    private void HandleMeleeAttackChargeStart(Vector3 position)
    {
        PlayFeedback("ChargeStart_FB", position);
    }

    private void HandleMeleeAttackChargeCancel(Vector3 position)
    {
        PlayFeedback("ChargeCancel_FB", position);
    }

    private void HandleMeleeAttackChargeFinished(Vector3 position)
    {
        PlayFeedback("ChargeFinish_FB", position);
    }

    private void HandleChargeMeleeAttackStart(Vector3 position)
    {
        PlayFeedback("ChargeAttackStart_FB", position);
    }

    private void HandleChargeMeleeAttackFinished(Vector3 position)
    {
        PlayFeedback("ChargeAttackFinish_FB", position);
    }

    private void HandleTier1ChargeAttackEffect(Vector3 position)
    {
        PlayFeedback("Tier1ChargeAttackHit_FB", position);
    }

    private void HandleTier2ChargeAttackEffect(Vector3 position)
    {
        PlayFeedback("Tier2ChargeAttackHit_FB", position);
    }

    private void HandleTier3ChargeAttackEffect(Vector3 position)
    {
        PlayFeedback("Tier3ChargeAttackHit_FB", position);
    }

    private void HandleRangedAttackChargeStart(Vector3 position)
    {
        PlayFeedback("RangeAttackChargeStart_FB", position);
    }

    private void HandleRangedAttackChargePerform(Vector3 position)
    {
        PlayFeedback("RangeAttackCharging_FB", position);
    }

    private void HandleRangedAttackChargeCancel(Vector3 position)
    {
        PlayFeedback("RangeAttackChargeCancel_FB", position);
    }

    private void HandleRangedAttackChargeFinished(Vector3 position)
    {
        PlayFeedback("RangeAttackChargeFinish_FB", position);
    }

    private void HandleRangedAttackStart(Vector3 position)
    {
        PlayFeedback("RangeAttackStart_FB", position);
    }

    private void HandleRangedAttackAffect(Vector3 position, Collider collider)
    {
        PlayFeedback("RangeAttackHit_FB", position);
    }

    private void HandleParryStart(Vector3 position)
    {
        PlayFeedback("ParryStart_FB", position);
    }

    private void HandleParryAffect(Vector3 position, Collider collider)
    {
        PlayFeedback("ParrySuccess_FB", position);
    }

    private void HandleCounterAttackStart(Vector3 position)
    {
        PlayFeedback("CounterAttackStart_FB", position);
    }

    private void HandleTier1FirstCounterAttackEffect(Vector3 position)
    {
        PlayFeedback("Tier1CounterAttackFirstHit_FB", position);
    }

    private void HandleTier2FirstCounterAttackEffect(Vector3 position)
    {
        PlayFeedback("Tier2CounterAttackFirstHit_FB", position);
    }

    private void HandleTier3FirstCounterAttackEffect(Vector3 position)
    {
        PlayFeedback("Tier3CounterAttackFirstHit_FB", position);
    }

    private void HandleTier1SecondCounterAttackEffect(Vector3 position)
    {
        PlayFeedback("Tier1CounterAttackSecondHit_FB", position);
    }

    private void HandleTier2SecondCounterAttackEffect(Vector3 position)
    {
        PlayFeedback("Tier2CounterAttackSecondHit_FB", position);
    }

    private void HandleTier3SecondCounterAttackEffect(Vector3 position)
    {
        PlayFeedback("Tier3CounterAttackSecondHit_FB", position);
    }

    private void HandleCounterAttackFinished(Vector3 position)
    {
        PlayFeedback("CounterAttackFinish_FB", position);
    }
    #endregion
}
