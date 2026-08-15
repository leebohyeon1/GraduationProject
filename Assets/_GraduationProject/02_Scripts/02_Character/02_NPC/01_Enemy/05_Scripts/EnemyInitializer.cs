using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

/// <summary>
/// 몬스터의 다양한 시스템을 초기화하는 컴포넌트입니다. 안전한 컬링 설정을 포함합니다.
/// </summary>
public class EnemyInitializer : MonoBehaviour
{
    private const int FinalInitializationPhase = 6;

    [Header("Debug")]
    [SerializeField] private bool _enableLogging = true;
    
    [Header("Group Settings")]
    [SerializeField] private GroupAi _targetGroupAi; 

    private Enemy _enemy;
    private PlayerController _player;
    private GroupAi _groupAi;
    private GroupAiZone _deferredZone;
    private AiController _deferredAiController;
    private AIPath _deferredAiPath;
    private EnemyStateController _deferredStateController;
    private Animator _deferredAnimator;
    private bool _restoreAiControllerEnabled;
    private bool _restoreAiPathEnabled;
    private bool _restoreStateControllerEnabled;
    private bool _restoreAnimatorEnabled;
    private bool _isDeferredInitialization;
    private bool _initializationFailed;
    private int _nextDeferredPhase;

    private static PlayerController _cachedPlayer;
    
    private Dictionary<Type, Component> _componentCache = new Dictionary<Type, Component>();
    private HashSet<string> _initializedSystems = new HashSet<string>();

    public void Initialize()
    {
        if (IsInitialized("Final") || _isDeferredInitialization) return;

        GroupAiZone zone = GetComponentInParent<GroupAiZone>(true);
        if (zone != null && zone.enabled)
        {
            BeginDeferredInitialization(zone);
            return;
        }

        InitializeImmediately();
    }

    private void InitializeImmediately()
    {
        try
        {
            Phase0_Validation();
            Phase1_CollectReferences();
            Phase2_InitializeData();
            Phase3_InitializeComponents();
            Phase4_InitializeAI();
            Phase5_RegisterGroup();
            Phase6_FinalizeState();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EnemyInitializer] Initialization failed for {gameObject.name}: {ex.Message}");
        }
    }

    private void BeginDeferredInitialization(GroupAiZone zone)
    {
        try
        {
            Phase0_Validation();
            _deferredZone = zone;
            _nextDeferredPhase = 1;
            _isDeferredInitialization = true;
            SuspendRuntimeAI();
            zone.EnqueueInitialization(this);
        }
        catch (Exception ex)
        {
            FailInitialization(ex);
        }
    }

    internal bool AdvanceDeferredInitialization()
    {
        if (!_isDeferredInitialization || _initializationFailed || IsInitialized("Final")) return true;

        try
        {
            switch (_nextDeferredPhase)
            {
                case 1:
                    Phase1_CollectReferences();
                    break;
                case 2:
                    Phase2_InitializeData();
                    break;
                case 3:
                    Phase3_InitializeComponents();
                    break;
                case 4:
                    Phase4_InitializeAI();
                    break;
                case 5:
                    Phase5_RegisterGroup();
                    break;
                case FinalInitializationPhase:
                    Phase6_FinalizeState();
                    CompleteDeferredInitialization();
                    return true;
                default:
                    return true;
            }

            _nextDeferredPhase++;
            return false;
        }
        catch (Exception ex)
        {
            FailInitialization(ex);
            return true;
        }
    }

    public void Reinitialize()
    {
        if (_isDeferredInitialization)
        {
            InitializeImmediately();
            CompleteDeferredInitialization();
            return;
        }

        Phase2_InitializeData();
        Phase3_InitializeComponents(skipCache: true); 
        Phase4_InitializeAI();
        Phase5_RegisterGroup();
        Phase6_FinalizeState();
    }

    public T GetCachedComponent<T>() where T : Component
    {
        if (_componentCache.TryGetValue(typeof(T), out var component)) return component as T;
        var cached = GetComponent<T>();
        if (cached != null) _componentCache[typeof(T)] = cached;
        return cached;
    }

    private void Phase0_Validation()
    {
        _enemy = GetComponent<Enemy>();
        if (_enemy == null) throw new InvalidOperationException("Enemy component is missing.");
        _initializationFailed = false;
        _initializedSystems.Clear();
    }

    private void Phase1_CollectReferences()
    {
        if (_cachedPlayer == null) _cachedPlayer = GameObject.FindFirstObjectByType<PlayerController>();
        _player = _cachedPlayer;
        FindOrCreateGroupAi();
    }

    private void FindOrCreateGroupAi()
    {
        if (_targetGroupAi != null) { _groupAi = _targetGroupAi; return; }
        if (_deferredZone != null && _deferredZone.targetGroupAi != null)
        {
            _groupAi = _deferredZone.targetGroupAi;
            return;
        }
        _groupAi = FindFirstObjectByType<GroupAi>();
        if (_groupAi == null)
        {
            GameObject groupObj = new GameObject("EnemyGroup");
            _groupAi = groupObj.AddComponent<GroupAi>();
        }
    }

    private void Phase2_InitializeData()
    {
        if (_enemy.Data == null)
        {
            _enemy.Data = new EnemyData { Player = _player, StartPosition = _enemy.transform.position, GroupAi = _groupAi, CurrentStiffness = 4 };
        }
        else
        {
            _enemy.Data.StartPosition = _enemy.transform.position;
            _enemy.Data.GroupAi = _groupAi;
            _enemy.Data.Player = _player;
        }
        MarkInitialized("Data");
    }

    private void Phase3_InitializeComponents(bool skipCache = false)
    {
        InitializeStateController();
        InitializeAnimationSystem();
        InitializeParrySystem(skipCache);
        InitializeHealthSystem(skipCache);
        InitializeStiffnessSystem(skipCache);
        MarkInitialized("Components");
    }

    private void InitializeStateController()
    {
        var sc = GetComponent<EnemyStateController>();
        if (sc != null) { sc.Initialize(_enemy); _enemy._stateController = sc; }
    }

    private void InitializeAnimationSystem()
    {
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            // [Fix] 렌더러가 있는 경우에만 안전하게 컬링 모드 설정하여 MissingComponentException 방지
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null) anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            else anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        var handler = GetComponent<Enemy_AnimationEventHandler>();
        if (handler != null) handler.Initialize();

        var bridge = GetComponent<EnemyAnimationBridge>();
        if (bridge != null) { bridge.Initialize(_enemy, anim); _enemy._animationBridge = bridge; }
    }

    private void InitializeHealthSystem(bool skipCache)
    {
        var health = GetOrGetComponent<EnemyHealth>(skipCache);
        if (health != null) health.InitializeHealth(_enemy);
    }


    private void InitializeParrySystem(bool skipCache)
    {
        var ps = GetOrGetComponent<ParrySystem>(skipCache);
        if (ps != null) ps.Initialize(_enemy);
    }

    private void InitializeStiffnessSystem(bool skipCache)
    {
        var ss = GetOrGetComponent<Mon_Stiffness>(skipCache);
        if (ss != null) ss.Initialize(_enemy);
    }

    private T GetOrGetComponent<T>(bool skipCache) where T : Component
    {
        if (!skipCache && _componentCache.TryGetValue(typeof(T), out var cached)) return cached as T;
        var component = GetComponent<T>();
        if (component != null) _componentCache[typeof(T)] = component;
        return component;
    }

    private void Phase4_InitializeAI()
    {
        var aiPath = GetComponent<AIPath>();
        var aiController = GetComponent<AiController>();
        if (aiController != null) aiController.Initialize(_enemy);

        var movement = GetComponent<EnemyMovement>();
        if (movement != null) { movement.Initialize(_enemy); _enemy.Movement = movement; movement.StopMovement(); }
        
        MarkInitialized("AI");
    }

    private void Phase5_RegisterGroup()
    {
        if (_groupAi != null) _groupAi.GroupAdd(_enemy);
        MarkInitialized("GroupAI");
    }

    private void Phase6_FinalizeState()
    {
        _enemy.SetState(EnemyStateController.EnemyState.Idle);
        _enemy.gameObject.SetActive(true);
        MarkInitialized("Final");
    }

    private void SuspendRuntimeAI()
    {
        _deferredStateController = GetComponent<EnemyStateController>();
        if (_deferredStateController != null)
        {
            _restoreStateControllerEnabled = _deferredStateController.enabled;
            _deferredStateController.enabled = false;
        }

        _deferredAnimator = GetComponent<Animator>();
        if (_deferredAnimator != null)
        {
            _restoreAnimatorEnabled = _deferredAnimator.enabled;
            _deferredAnimator.enabled = false;
        }

        _deferredAiController = GetComponent<AiController>();
        if (_deferredAiController != null)
        {
            _restoreAiControllerEnabled = _deferredAiController.enabled;
            _deferredAiController.enabled = false;
        }

        _deferredAiPath = GetComponent<AIPath>();
        if (_deferredAiPath != null)
        {
            _restoreAiPathEnabled = _deferredAiPath.enabled;
            _deferredAiPath.enabled = false;
        }
    }

    private void CompleteDeferredInitialization()
    {
        _isDeferredInitialization = false;
        _deferredZone = null;

        if (_deferredAiPath != null) _deferredAiPath.enabled = _restoreAiPathEnabled;
        if (_deferredAnimator != null) _deferredAnimator.enabled = _restoreAnimatorEnabled;
        if (_deferredStateController != null) _deferredStateController.enabled = _restoreStateControllerEnabled;
        if (_deferredAiController != null) _deferredAiController.enabled = _restoreAiControllerEnabled;

        _deferredAiPath = null;
        _deferredAnimator = null;
        _deferredStateController = null;
        _deferredAiController = null;
    }

    private void FailInitialization(Exception ex)
    {
        _initializationFailed = true;
        Debug.LogError($"[EnemyInitializer] Initialization failed for {gameObject.name}: {ex.Message}", this);
        CompleteDeferredInitialization();
    }

    private void MarkInitialized(string name) => _initializedSystems.Add(name);
    public bool IsInitialized(string name) => _initializedSystems.Contains(name);
    private void OnDestroy()
    {
        _componentCache.Clear();
        _initializedSystems.Clear();
        _deferredZone = null;
    }
    private void Log(string msg) { if (_enableLogging) Debug.Log($"[EnemyInitializer] {msg}"); }
}
