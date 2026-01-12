using DG.Tweening;
using System;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 메인 클래스입니다.
/// 모든 플레이어 관련 컴포넌트들을 관리하고 상태 머신을 통해 플레이어의 행동을 제어합니다.
/// </summary>
public class Player : MonoBehaviour
{
    #region Private Fields

    [Header("Development")]
    [SerializeField] private bool _saveDuringPlay = false; // 플레이 중 변경 사항 저장 여부
     private PlayerDataSO _runtimePlayerData; // 런타임 플레이어 데이터 (디버그용) 

    [Header("Components")]
    [SerializeField] private Animator _animator; // 애니메이터
    [SerializeField] private CharacterController _characterController; // 캐릭터 컨트롤러

    [SerializeField] private PlayerDataSO _data; // 플레이어 데이터베이스
    private PlayerStats _stats; // 플레이어 스탯
    [SerializeField] private PlayerEvents _events; // 플레이어 이벤트

    [SerializeField] private PlayerInputHandler _input; // 입력 핸들러
    [SerializeField] private PlayerHealth _health; // 체력 컴포넌트
    [SerializeField] private PlayerMovement _movement; // 이동 컴포넌트
    [SerializeField] private PlayerCombat _combat; // 전투 컴포넌트
    [SerializeField] private PlayerInteract _interact; // 상호작용 컴포넌트
    [SerializeField] private PlayerStamina _stamina; // 스테미나 컴포넌트
    [SerializeField] private LockOnSystem _lockOnSystem; // 락온 시스템  
    [SerializeField] private PlayerAbility _ability; // 플레이어 능력 컴포넌트
    [SerializeField] private PlayerPotion _potion;

    private StateMachine<Player> _stateMachine; // 상태 머신

    #endregion

    #region Properties
    public Animator Animator => _animator;

    public PlayerDataSO Data => _data;
    public PlayerStats Stats => _stats;
    public PlayerEvents Events => _events;
    public PlayerInputHandler Input => _input;

    public PlayerHealth Health => _health;
    public PlayerMovement Movement => _movement;
    public PlayerCombat Combat => _combat;
    public PlayerInteract Interact => _interact;
    public PlayerStamina Stamina => _stamina;
    public LockOnSystem LockOnSystem => _lockOnSystem;
    public PlayerAbility Ability => _ability;
    public PlayerPotion Potion => _potion;

    public IInputDeviceDetector DeviceDetector => InputDeviceDetector.Instance;
    
    /// <summary>
    /// 현재 플레이어 상태를 나타냅니다.
    /// </summary>
    public Type CurrentPlayerState => _stateMachine.CurrentState?.GetType();
    #endregion


    #region EventSO
    [Space(10f), Header("Event SO")]
    [SerializeField] private OnCameraInitializeSO _onCameraInitializeSO; // 카메라 초기화 이벤트

    #endregion

    private void Awake()
    {
        InitializeReference();
        InitializeStateMachine();
    }

    private void Start()
    {
        SubscribeToEvents();

        _onCameraInitializeSO.Publish(new CameraTarget(0, transform, 5, 0.6f));
        _onCameraInitializeSO.Publish(new CameraTarget(0, _lockOnSystem.LockOnIndicator.transform, 3, 0.4f));
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
        if(_saveDuringPlay)
        {
            _data.SetData(_stats.Data);
        }

        UnsubscribeToEvents();
    }
    
    /// <summary>
    /// 모든 참조를 초기화합니다.
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
    
        if (_data == null)
        {
            _data = GetComponent<PlayerDataSO>();
        }

        _stats = new PlayerStats(Data, _events, this);
        _runtimePlayerData = _stats.RuntimeData;


        if (_events == null)
        {
            _events = GetComponent<PlayerEvents>();
        }
        _events.Initialize(_stats);

        if (_input == null)
        {
            _input = GetComponent<PlayerInputHandler>();
        }
        _input.Initialize(DeviceDetector);
    
        if (_health == null)
        {
            _health = GetComponent<PlayerHealth>();
        }
        _health.Initialize(Stats, Events);
    
        if (_movement == null)
        {
            _movement = GetComponent<PlayerMovement>();
        }
        _movement.Initialize(_characterController, Events);
    
        if (_combat == null)
        {
            _combat = GetComponent<PlayerCombat>();
        }
        _combat.Initialize(Stats, Events);

        if(_interact == null)
        {
            _interact = GetComponent<PlayerInteract>();
        }

        if(_stamina == null)
        {
            _stamina = GetComponent<PlayerStamina>();
        }
        _stamina.Initialize(Stats, Events);

        if(_lockOnSystem == null)
        {
            _lockOnSystem = GetComponent<LockOnSystem>();
        }

        if(_ability == null)
        {
            _ability = GetComponent<PlayerAbility>();
        }
        _ability.Initialize(Stats);

        if (_potion == null)
        {
            _potion = GetComponent<PlayerPotion>();
        }
        _potion.Initialize(Stats);
    }
    
    /// <summary>
    /// 상태 머신을 초기화하고 모든 상태를 추가합니다.
    /// </summary>
    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine<Player>(this);
    
        _stateMachine.AddState(new PlayerIdleState(this, _stateMachine));
        _stateMachine.AddState(new PlayerMoveState(this, _stateMachine));
        _stateMachine.AddState(new PlayerDodgeState(this, _stateMachine));
        _stateMachine.AddState(new PlayerAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerChargeState(this, _stateMachine));
        _stateMachine.AddState(new PlayerChargeAttackState(this, _stateMachine));
        _stateMachine.AddState(new PlayerHitState(this, _stateMachine));
        _stateMachine.AddState(new PlayerParryState(this, _stateMachine));

        // 초기 상태를 Idle로 설정
        _stateMachine.ChangeState<PlayerIdleState>();
    }

    /// <summary>
    /// 매 프레임 호출되는 업데이트 함수입니다.
    /// </summary>
    private void OnUpdate()
    {

        if (Time.time - Combat.LastBattleTime >= Stats.RuntimeData.BattleOutTime && Combat.IsBattleState)
        {
            Events.TriggerBattleStateChanged(false);
        }

        if(Input.InteractInput)
        {
            Interact.Interact();
        }

        if(Input.PotionInput && Potion.CurrentPotion > 0)
        {
            Health.Heal(Stats.RuntimeData.PotionHealAmount);
            Potion.UsePotion();
        }

        if(Input.ToggleLockOnInput)
        {
            if(!_stats.IsLockOn)
            {
                var deviceType = DeviceDetector.CurrentInputDevice;
                var moveInput = Input.LockOnTargetChangeVector2Input;
                var mousePosition = Input.MousePosition;

                _stats.IsLockOn = _lockOnSystem.LockOn(deviceType, moveInput, mousePosition);
            }
            else
            {
                _stats.IsLockOn = false;
                _lockOnSystem.LockOff();
            }
        }
        else if(Input.LockOnTargetChangeInput && !_stats.IsLockOn)
        {
            var deviceType = DeviceDetector.CurrentInputDevice;
            var moveInput = Input.LockOnTargetChangeVector2Input;
            var mousePosition = Input.MousePosition;

            _stats.IsLockOn = _lockOnSystem.LockOn(deviceType, moveInput, mousePosition);
        }
        else if(_stats.IsLockOn)
        {
            // 입력 강도가 일정 이상인지 확인 (0.5f ~ 0.8f 추천)
            bool hasInput = Input.LockOnTargetChangeInput || Input.LockOnTargetChangeVector2Input.sqrMagnitude > 0.5f;

            // 쿨타임이 지났는지 확인
            bool isCooldownReady = Time.time >= Stats.LastTargetChangeTime + PlayerStats.TARGET_CHANGE_COOLDOWN;

            if (hasInput && isCooldownReady)
            {
                var deviceType = DeviceDetector.CurrentInputDevice;
                var moveInput = Input.LockOnTargetChangeVector2Input;
                var mousePosition = Input.MousePosition;

                // 타겟 변경 시도
                _lockOnSystem.ChangeLockOnTarget(deviceType, moveInput, mousePosition);

                // 변경 시도했으므로 시간 갱신
                Stats.LastTargetChangeTime = Time.time;
            }
            else if (!hasInput)
            {
                // 스틱을 중립으로 놓으면 쿨타임을 조금 더 빨리 초기화
                Stats.LastTargetChangeTime = 0f; 
            }
        }
    }
    
    /// <summary>
    /// 고정된 시간 간격으로 호출되는 업데이트 함수입니다.
    /// </summary>
    private void OnFixedUpdate()
    {
        _movement.ApplyGravity();
    }
    
    #region Event
    /// <summary>
    /// 이벤트 구독을 설정합니다.
    /// </summary>
    private void SubscribeToEvents()
    {

    }
    
    /// <summary>
    /// 이벤트 구독을 해제합니다.
    /// </summary>
    private void UnsubscribeToEvents()
    {
        Stats.Dispose();
        Movement.Dispose();
        Health.Dispose();
        Combat.Dispose();
        Stamina.Dispose();
        Ability.Dispose();
    }
    #endregion


#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        // 공격 범위 기즈모
        DrawActionGizmo(_data.CombatData.AttackDatas[0].AttackConfig.AttackRadius, Color.mediumVioletRed);
        DrawActionGizmo(_data.CombatData.AttackDatas[1].AttackConfig.AttackRadius, Color.orangeRed);
        DrawActionGizmo(_data.CombatData.AttackDatas[2].AttackConfig.AttackRadius, Color.darkRed);
        //DrawActionGizmo(_stats.CurrentChargeAttackData.AttackConfig.AttackRadius, Color.red);

    }

    private void DrawActionGizmo(Vector3 radius, Color color)
    {
        Vector3 attackCenter = transform.position + transform.forward * (radius.z / 2);
        Gizmos.color = color;
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, radius);
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif

}