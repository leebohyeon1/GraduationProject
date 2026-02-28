using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private Camera _camera;                                 // 메인 카메라
    [SerializeField] private Animator _animator;            // 애니메이터

    private PlayerEvents _events;                           // 플레이어 이벤트 클래스
    [SerializeField] private InputReaderSO _inputReader;    // 입력을 읽어오는 클래스
    [SerializeField] private PlayerDataSO _data;            // 캐릭터 데이터

    [SerializeField] private PlayerInputHandler _inputHandler;  // 입력 처리 클래스
    [SerializeField] private PlayerHealth _health;          // 플레이어 체력 클래스
    [SerializeField] private PlayerMovement _movement;      // 플레이어 움직임 구현 클래스
    [SerializeField] private PlayerStamina _stamina;        // 플레이어 스테미나 시스템
    [SerializeField] private PlayerCombat _combat;          // 플레이어 전투 시스템
    [SerializeField] private PlayerAnimationTrigger _animationTrigger; // 플레이어 애니메이션 이벤트 트리거
    [SerializeField] private PlayerLockOn _lockOn;          // 락온 시스템
    [SerializeField] private PlayerAbility _ability;        // 능력 시스템
    [SerializeField] private PlayerPotion _potion;          // 포션 시스템
    [SerializeField] private PlayerMoney _money;            // 돈 
    [SerializeField] private PlayerInteract _interact;      // 상호작용 시스템

    private StateMachine<PlayerController> _stateMachine;   // 상태 머신
    private List<IDisposable> _disposableList = new List<IDisposable>(); // 해제해야 하는 객체 리스트

    [SerializeField] private OnPlayerSpawnedSO playerSpawnedSO; // 플레이어 스폰 이벤트

    [Header("Properties")]
    public Camera Camera => _camera;
    public Animator Animator => _animator;

    public PlayerEvents Events => _events;
    public InputReaderSO InputReader => _inputReader;
    public PlayerDataSO Data => _data;

    public PlayerInputHandler InputHandler => _inputHandler;
    public PlayerHealth Health => _health;
    public PlayerMovement Movement => _movement;
    public PlayerStamina Stamina => _stamina;
    public PlayerCombat Combat => _combat;
    public PlayerAnimationTrigger AnimationTrigger => _animationTrigger;
    public PlayerLockOn LockOn => _lockOn;
    public PlayerAbility Ability => _ability;
    public PlayerPotion Potion => _potion;
    public PlayerMoney Money => _money;
    public PlayerInteract Interact => _interact;

    public StateMachine<PlayerController> FSM => _stateMachine;

    public PlayerData RuntimeData { get; private set; }

    private async void Start()
    {        
        // 참조 초기화
        await InitializeReferences();
        InitializeFSM();

        // 초기화 후 스폰 이벤트 발행
        playerSpawnedSO.Publish(this);
    }

    private void Update()
    {
        // FSM Update
        _stateMachine.Update(); 
    }

    private void FixedUpdate()
    {
        // FSM FixedUpdate
        _stateMachine?.FixedUpdate();
    }

    private void OnDestroy()
    {
        // 객체 해제
        Disapose();
    }

    #region Initialize
    /// <summary>
    /// 참조 초기화
    /// </summary>
    private async Task InitializeReferences()
    {
        // DataManager에서 런타임 데이터 가져오기
        if (DataManager.Instance != null)
        {
            RuntimeData = DataManager.Instance.GetGameData().PlayerData;
            
            transform.position = RuntimeData.LastPosition;
        }
        else
        {
            Debug.LogWarning("DataManager Instance is null. Creating default RuntimeData.");
            RuntimeData = new PlayerData();
            RuntimeData.InitializeFromSO(Data);
        }

        // 카메라 초기화
        _camera = Camera.main;

        // Animator 초기화
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        // PlayerEvents 초기화
        if (_events == null)
        {
            _events = new PlayerEvents();
        }

        // InputReader 초기화
        if (_inputReader == null)
        {
            _inputReader = await Addressables.
                LoadAssetAsync<InputReaderSO>("InputReader").Task;
        }

        // CharacterData 초기화
        if (_data == null)
        {
            _data = await Addressables.
                LoadAssetAsync<PlayerDataSO>("PlayerData").Task;
        }

        // InputHandler 초기화
        if (TryGetComponent<PlayerInputHandler>(out _inputHandler))
        {
            _inputHandler.Initialize(this);
        }

        // PlayerHealth 초기화
        if (TryGetComponent<PlayerHealth>(out _health))
        {
            _health.Initialize(this);
        }

        // PlayerMovement 초기화
        if (TryGetComponent<PlayerMovement>(out _movement))
        {
            _movement.Initialize(this);
        }

        // PlayerStamina 초기화
        if (TryGetComponent<PlayerStamina>(out _stamina))
        {
            _stamina.Initialize(this);
        }

        // PlayerCombat 초기화
        if (TryGetComponent<PlayerCombat>(out _combat))
        {
            _combat.Initialize(this);
        }

        // PlayerAnimationTrigger 초기화
        if (TryGetComponent<PlayerAnimationTrigger>(out _animationTrigger))
        {
            _animationTrigger.Initialize(this);
        }

        if(TryGetComponent<PlayerLockOn>(out _lockOn))
        {
        }

        if(TryGetComponent<PlayerAbility>(out _ability))
        {
            _ability.Initialize(this);
        }

        if(TryGetComponent<PlayerPotion>(out _potion))
        {
            _potion.Initialize(this);
        }

        if(TryGetComponent<PlayerMoney>(out _money))
        {
            _money.Initialize(this);
        }

        if(TryGetComponent<PlayerInteract>(out _interact))
        {
            _interact.Initialize(this);
        }
    }

    /// <summary>
    /// FSM 초기화
    /// </summary>
    private void InitializeFSM()
    {
        // 머신 및 상태 초기화, 등록
        _stateMachine = new StateMachine<PlayerController>(this);
        _stateMachine.AddState(new PlayerIdleState(_stateMachine));
        _stateMachine.AddState(new PlayerMoveState(_stateMachine));
        _stateMachine.AddState(new PlayerDodgeState(_stateMachine));
        _stateMachine.AddState(new PlayerNormalAttackState(_stateMachine));
        _stateMachine.AddState(new PlayerNormalCounterState(_stateMachine));
        _stateMachine.AddState(new PlayerHeavyCounterState(_stateMachine));
        _stateMachine.AddState(new PlayerChargeState(_stateMachine));
        _stateMachine.AddState(new PlayerDamagedState(_stateMachine));
        _stateMachine.AddState(new PlayerKnockdownState(_stateMachine));
        _stateMachine.AddState(new PlayerSpecialAttackState(_stateMachine));
        _stateMachine.AddState(new PlayerDraggedState(_stateMachine));

        // Idle 상태에서 시작
        _stateMachine.ChangeState(typeof(PlayerIdleState));
    }
    #endregion

    #region Dispose
    /// <summary>
    /// 객체 폐기
    /// </summary>
    private void Disapose()
    {
        foreach(IDisposable disaposable in _disposableList)
        {
            disaposable.Dispose();
        }

        _disposableList.Clear();
    }

    /// <summary>
    /// Disposable 구독
    /// </summary>
    /// <param name="disposable">구독할 객체</param>
    public void RegisterDisposable(IDisposable disposable)
    {
        // 이미 구독되어 있으면 리턴
        if(_disposableList.Contains(disposable))
        {
            return;
        }

        _disposableList.Add(disposable);
    }
    #endregion

}