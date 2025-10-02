using BH_Lib.DI;
using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

public class Player : DIMonoBehaviour
{
    #region Private Fields
    [Inject] private IInputDeviceDetector _inputDeviceDetector;

    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;

    [SerializeField] private PlayerDataBase _dataBase;
    [SerializeField] private PlayerStats _stats;

    [SerializeField] private PlayerInputHandler _input;
    [SerializeField] private PlayerHealth _health;
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerCombat _combat;
    [SerializeField] private PlayerHeat _heat;
    [SerializeField] private PlayerEvents _events;

    private StateMachine<Player> _stateMachine;
    private PlayerHeatManager _heatManager;
    private PlayerCombatManager _combatManager;
    #endregion

    #region Properties
    public Animator Animator => _animator;
    public PlayerDataBase DataBase => _dataBase;
    public PlayerInputHandler Input => _input;
    public PlayerHealth Health => _health;
    public PlayerMovement Movement => _movement;
    public PlayerCombat Combat => _combat;
    public PlayerHeat Heat => _heat;
    public PlayerEvents Events => _events;
    public BasePlayerDatasSO BaseData => DataBase.BaseData;
    public PlayerStats Stats => _stats;

    public IInputDeviceDetector InputDeviceDetector => _inputDeviceDetector;

    /// <summary>
    /// 현재 플레이어 상태
    /// </summary>
    public Type CurrentPlayerState => _stateMachine.CurrentStateType;
    #endregion

    private void Start()
    {
        InitializeReference();
        InitializeStateMachine();

        SubscribeToEvents();
    }

    private void Update()
    {
        OnUpdate();
        _stateMachine?.Update();
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();
        // 상태 머신 고정 업데이트
        _stateMachine?.FixedUpdate();
    }

    private void LateUpdate()
    {
        _input.LateTick();
    }

    private void OnDestroy()
    {
        UnsubscribeToEvents();
    }

    /// <summary>
    /// 레퍼런스 초기화
    /// </summary>
    private void InitializeReference()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
        }

        if (_dataBase == null)
        {
            _dataBase = GetComponent<PlayerDataBase>();
        }
        _stats = new PlayerStats(BaseData);

        if (_input == null)
        {
            _input = GetComponent<PlayerInputHandler>();
        }
        _input.Initialize(_inputDeviceDetector);

        if (_health == null)
        {
            _health = GetComponent<PlayerHealth>();
        }
        _health.Initialize(Stats);

        if (_movement == null)
        {
            _movement = GetComponent<PlayerMovement>();
        }
        _movement.Initialize(_characterController);

        if (_combat == null)
        {
            _combat = GetComponent<PlayerCombat>();
        }
        _combat.Initialize(Stats);

        if (_heat  == null)
        {
            _heat = GetComponent<PlayerHeat>(); 
        }
        _heat.Initialize(DataBase.SourceMapData, DataBase.TierStatData, DataBase.OverHeatData);

        if (_events == null)
        {
            _events = GetComponent<PlayerEvents>();
        }

        _heatManager = new PlayerHeatManager(Heat, Events);
        _combatManager = new PlayerCombatManager(Combat, Events);
    }

    /// <summary>
    /// 상태머신 초기화
    /// </summary>
    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine<Player>(this);

        _stateMachine.AddState(new PlayerIdleState(this, _stateMachine));
        _stateMachine.AddState(new PlayerMoveState(this, _stateMachine));
        _stateMachine.AddState(new PlayerDodgeState(this, _stateMachine));
        _stateMachine.AddState(new PlayerFirstAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerSecondAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerThirdAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerChargeState(this, _stateMachine));
        _stateMachine.AddState(new PlayerChargeAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerRangedChargeState(this, _stateMachine));
        _stateMachine.AddState(new PlayerRangedAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerHitState(this, _stateMachine));
        _stateMachine.AddState(new PlayerDefendState(this, _stateMachine));
        _stateMachine.AddState(new PlayerFirstCounterAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerSecondCounterAttackState(this, _stateMachine));

        SetupStateTransitions();

        // 초기 상태를 Idle로 설정
        _stateMachine.ChangeState<PlayerIdleState>();
    }
    /// <summary>
    /// 상태머신 변경 세팅
    /// </summary>
    private void SetupStateTransitions()
    {
        // Hit 상태로의 전환 (모든 상태에서 가능)
        _stateMachine.AddAnyTransition<PlayerHitState>(() =>
            !Health.IsDead && Stats.IsDamaged);

        // Idle 상태에서의 전환
        _stateMachine.AddTransition<PlayerIdleState, PlayerMoveState>(() 
            => Input.MoveInput != Vector2.zero);
        _stateMachine.AddTransition<PlayerIdleState, PlayerDodgeState>(()
            => Input.DodgeInput && Time.time - Movement.LastDodgeTime >= Stats.CombatData.DodgeCooldown);
        _stateMachine.AddTransition<PlayerIdleState, PlayerFirstAttackState>(()
            => Input.AttackInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerChargeState>(() 
            => Input.AttackHeldInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerRangedChargeState>(()
            => Input.RangedAttackInput);
        _stateMachine.AddTransition<PlayerIdleState, PlayerDefendState>(()
            => Input.DefendInput);

        // Move 상태에서의 전환
        _stateMachine.AddTransition<PlayerMoveState, PlayerIdleState>(()
            => Input.MoveInput == Vector2.zero);
        _stateMachine.AddTransition<PlayerMoveState, PlayerDodgeState>(()
            => Input.DodgeInput && Time.time - Movement.LastDodgeTime >= Stats.CombatData.DodgeCooldown);
        _stateMachine.AddTransition<PlayerMoveState, PlayerFirstAttackState>(()
            => Input.AttackInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerChargeState>(()
            => Input.AttackHeldInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerRangedChargeState>(()
            => Input.RangedAttackInput);
        _stateMachine.AddTransition<PlayerMoveState, PlayerDefendState>(()
            => Input.DefendInput);
    }

    /// <summary>
    /// Update에 호출되는 함수
    /// </summary>
    private void OnUpdate()
    {
        _movement.CheckGrounded(DataBase.BaseData.GroundCheckDistance,
                    DataBase.BaseData.GroundLayerMask);
    
        if (Heat.CanHeatTierEffect())
        {
            Events.TriggerTier(Heat.CurrentTier);
        }
    
        if (Time.time - Combat.LastBattleTime >= Stats.BattleOutTime && Combat.IsBattleState)
        {
            Events.TriggerBattleStateChanged(false);
        }
    }    

    /// <summary>
    /// FixedUpdate에 호출되는 함수
    /// </summary>
    private void OnFixedUpdate()
    {
        _movement.ApplyGravity(DataBase.BaseData.Gravity);

    }

    #region Event
    private void SubscribeToEvents()
    {
        Stats.OnAnimationSpeedChanged += HandleAnimationSpeedChanged;
        Events.OnOverHeat += HandleOverHeat;

        Events.OnAttackStart += Combat.SetupCombatCenter;
        Events.OnParryPerform += Combat.SetupCombatCenter;
    }

    private void UnsubscribeToEvents()
    {
        _heatManager?.Dispose();
        _combatManager?.Dispose();

        Stats.OnAnimationSpeedChanged -= HandleAnimationSpeedChanged;
        Events.OnOverHeat -= HandleOverHeat;

        Events.OnAttackStart -= Combat.SetupCombatCenter;
        Events.OnParryPerform -= Combat.SetupCombatCenter;
    }

    private void HandleAnimationSpeedChanged(float speed)
    {
        Animator.speed = speed;
    }

    private void HandleOverHeat()
    {
        if (Heat.IsOverHeat)
        {
            Health.TakeDamage(DataBase.OverHeatData.DamagePerTick);
        }
    }
    #endregion
}
