using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private PlayerEvents _events;                           // 플레이어 이벤트 클래스
    [SerializeField] private InputReaderSO _inputReader;    // 입력을 읽어오는 클래스
    [SerializeField] private Animator _animator;            // 애니메이터
    [SerializeField] private PlayerDataSO _data;            // 캐릭터 데이터

    [SerializeField] private PlayerInputHandler _inputHandler;  // 입력 처리 클래스
    [SerializeField] private PlayerHealth _health;          // 플레이어 체력 클래스
    [SerializeField] private PlayerMovement _movement;      // 플레이어 움직임 구현 클래스
    [SerializeField] private PlayerStamina _stamina;        // 플레이어 스테미나 시스템
    [SerializeField] private PlayerCombat _combat;          // 플레이어 전투 시스템
    [SerializeField] private PlayerAnimationTrigger _animationTrigger; // 플레이어 애니메이션 이벤트 트리거
    [SerializeField] private LockOnSystem _lockOn;          // 락온 시스템

    private StateMachine<PlayerController> _stateMachine;        // 상태 머신

    [Header("Properties")]
    public PlayerEvents Events => _events;
    public InputReaderSO InputReader => _inputReader;
    public Animator Animator => _animator;
    public PlayerDataSO Data => _data;

    public PlayerInputHandler InputHandler => _inputHandler;
    public PlayerHealth Health => _health;
    public PlayerMovement Movement => _movement;
    public PlayerStamina Stamina => _stamina;
    public PlayerCombat Combat => _combat;
    public PlayerAnimationTrigger AnimationTrigger => _animationTrigger;
    public LockOnSystem LockOn => _lockOn;

    public StateMachine<PlayerController> FSM => _stateMachine;

    private async void Start()
    {
        // 참조 초기화
        await InitializeReferences();
        InitializeFSM();
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    private void FixedUpdate()
    {
        _stateMachine?.FixedUpdate();
    }

    private void OnDestroy()
    {
        _inputHandler?.Dispose();
        _stamina?.Dispose();
    }

    #region Initialize
    /// <summary>
    /// 참조 초기화
    /// </summary>
    private async Task InitializeReferences()
    {
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

        // Animator 초기화
        if(_animator == null)
        {
            _animator = GetComponent<Animator>();
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
            _inputHandler.Initialize(InputReader, _events);
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

        if(TryGetComponent<LockOnSystem>(out _lockOn))
        {
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
        _stateMachine.AddState(new PlayerNormalAttackState(_stateMachine));
        _stateMachine.AddState(new PlayerDodgeState(_stateMachine));
        _stateMachine.AddState(new PlayerNormalCounterState(_stateMachine));
        _stateMachine.AddState(new PlayerHeavyCounterState(_stateMachine));
        _stateMachine.AddState(new PlayerChargeState(_stateMachine));

        // Idle 상태에서 시작
        _stateMachine.ChangeState(typeof(PlayerIdleState));
    }
    #endregion
}