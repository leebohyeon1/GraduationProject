using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// PlayerController의 컴포넌트 참조 및 초기화 로직을 담당하는 부분
/// </summary>
public partial class PlayerController
{
    [Header("Required References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private InputReaderSO _inputReader;
    [SerializeField] private PlayerDataSO _data;
    [SerializeField] private OnPlayerSpawnedSO playerSpawnedSO;

    [Header("Player Components")]
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private PlayerHealth _health;
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerStamina _stamina;
    [SerializeField] private PlayerCombat _combat;
    [SerializeField] private PlayerAnimationTrigger _animationTrigger;
    [SerializeField] private PlayerLockOn _lockOn;
    [SerializeField] private PlayerAbility _ability;
    [SerializeField] private PlayerPotion _potion;
    [SerializeField] private PlayerMoney _money;
    [SerializeField] private PlayerInteract _interact;

    private PlayerEvents _events;
    private Camera _camera;

    #region Properties for Components
    public Camera Camera => _camera ??= Camera.main;
    public Animator Animator => _animator;
    public PlayerEvents Events => _events ??= new PlayerEvents();
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
    #endregion

    /// <summary>
    /// 참조 초기화
    /// </summary>
    private async Task InitializeReferences()
    {
        // 1. 필요한 에셋 로드 (Addressables)
        await LoadRequiredAssets();

        // 2. 런타임 데이터 초기화
        InitializeRuntimeData();

        // 3. 기본 레퍼런스 및 컴포넌트 초기화
        _camera = Camera.main;
        _animator ??= GetComponent<Animator>();
        _events ??= new PlayerEvents();

        // 4. 각 컴포넌트 자동 탐색 및 초기화
        InitComponent(ref _inputHandler);
        InitComponent(ref _health, c => c.OnDied += Dispose);
        InitComponent(ref _movement);
        InitComponent(ref _stamina);
        InitComponent(ref _combat);
        InitComponent(ref _animationTrigger);
        InitComponent(ref _lockOn);
        InitComponent(ref _ability);
        InitComponent(ref _potion);
        InitComponent(ref _money);
        InitComponent(ref _interact);
    }

    /// <summary>
    /// Addressables를 통한 필수 에셋 로드
    /// </summary>
    private async Task LoadRequiredAssets()
    {
        try
        {
            if (_inputReader == null)
            {
                _inputReader = await Addressables.LoadAssetAsync<InputReaderSO>("InputReader").Task;
            }

            if (_data == null)
            {
                _data = await Addressables.LoadAssetAsync<PlayerDataSO>("PlayerData").Task;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerController] Asset Loading Failed: {e.Message}");
        }
    }

    /// <summary>
    /// 게임 데이터 매니저로부터 런타임 데이터 초기화
    /// </summary>
    private void InitializeRuntimeData()
    {
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
    }

    /// <summary>
    /// 컴포넌트를 가져오고 초기화하는 범용 헬퍼 함수
    /// </summary>
    private void InitComponent<T>(ref T component, Action<T> extraAction = null) where T : MonoBehaviour
    {
        if (component == null)
        {
            TryGetComponent(out component);
        }
        
        if (component != null)
        {
            // 리플렉션을 사용하여 Initialize(PlayerController) 메서드 호출
            var method = typeof(T).GetMethod("Initialize", new[] { typeof(PlayerController) });
            if (method != null)
            {
                method.Invoke(component, new object[] { this });
            }
            
            extraAction?.Invoke(component);
        }
    }
}
