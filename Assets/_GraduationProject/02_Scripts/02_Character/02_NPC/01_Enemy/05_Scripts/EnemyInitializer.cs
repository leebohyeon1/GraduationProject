using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class EnemyInitializer : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _enableLogging = true;
    private Enemy _enemy;
    private PlayerController _player;
    private GroupAi _groupAi;
    // 컴포넌트 캐시 등록
    private Dictionary<Type, Component> _componentCache = new Dictionary<Type, Component>();

    // 초기화 상태 추적 string형태그 사용
    private HashSet<string> _initializedSystems = new HashSet<string>();
    public void Initialize()
    {
        Log($" starting Initialization Enemy: {gameObject.name}");
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
    private void Log(string message)
    {

        if (_enableLogging)
        {
            Debug.Log($"[EnemyInitializer] {message}");
        }
    }
    public void Reinitialize()
    {
        Log($"starting Reinitialization Enemy: {gameObject.name}");

        Phase2_InitializeData();
        Phase3_InitializeComponents(skipCache: true); // 캐시 사용
        Phase4_InitializeAI();
        Phase5_RegisterGroup();
        Phase6_FinalizeState();
    }
    //컴포넌트 캐시에서 가져오기
    public T GetCachedComponent<T>() where T : Component
    {
        if (_componentCache.TryGetValue(typeof(T), out var component))
        {
            return component as T;
        }

        // 캐시에 없으면 가져와서 저장
        var cached = GetComponent<T>();
        if (cached != null)
        {
            _componentCache[typeof(T)] = cached;
        }
        return cached;
    }
    #region Phase 0 : 초기화 검증
    private void Phase0_Validation()
    {
        _enemy = GetComponent<Enemy>();
        if (_enemy == null)
        {
            throw new InvalidOperationException("Enemy component is missing.");
        }
        _initializedSystems.Clear();
    }
    #endregion
    #region Phase 1: 기본 참조 수집 
    private void Phase1_CollectReferences()
    {
        Log("Phase 1: Collecting References started.");
        _player = GameObject.FindFirstObjectByType<PlayerController>();

        FindOrCreateGroupAi();

        Log("Phase 1: Collecting References completed.");
    }
    private void FindOrCreateGroupAi()
    {
        _groupAi = FindFirstObjectByType<GroupAi>();
        if (_groupAi == null)
        {
            GameObject groupObj = new GameObject("EnemyGroup");
            _groupAi = groupObj.AddComponent<GroupAi>();
            Log("Created new GroupAi instance.");
        }
    }
    #endregion

    #region Phase 2: 시스템 초기화
    private void Phase2_InitializeData()
    {
        Log("Phase 2 : Initializing data");

        if (_enemy.Data == null)
        {
            _enemy.Data = new EnemyData
            {
                Player = _player,
                StartPosition = _enemy.transform.position,
                GroupAi = _groupAi,
                CurrentStiffness = 4
            };
        }
        else
        {
            _enemy.Data.StartPosition = _enemy.transform.position;
        }
        MarkInitialized("Data");
        Log("Phase 2 : Complete");
    }
    #endregion

    #region Phasse 3 : 컴포넌트 초기화
    private void Phase3_InitializeComponents(bool skipCache = false)
    {
        Log("Phase 3 : Initializing Components");

        InitializeStateController();
        InitializeAnimationSystem();
        InitializeBillboardUI();
        InitializeParrySystem(skipCache);
        InitializeHealthSystem(skipCache);
        InitializeStiffnessSystem(skipCache);
        InitializeSpecialAbility(skipCache);
        MarkInitialized("Components");
        Log("Phase 3 complete");
    }

    private void InitializeBillboardUI()
    {
        var billboardUI = GetComponentInChildren<BillboardUI>();
        if (billboardUI == null)
        {
            Debug.LogError("BillboardUI component is missing.");
            return;
        }
        billboardUI.Initialize();
        Log("Initialized BillboardUI");
    }

    private void InitializeStateController()
    {
        var stateController = GetComponent<EnemyStateController>();
        if (stateController == null)
        {
            Debug.LogError("EnemyStateController component is missing.");
            return;
        }
        stateController.Initialize(_enemy);
        _enemy._stateController = stateController;
        Log("Initialized EnemyStateController");
    }
    private void InitializeAnimationSystem()
    {
        var animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing.");
            return;
        }
        var animHandler = GetComponent<Enemy_AnimationEventHandler>();
        if (animHandler == null)
        {
            Debug.LogError("[EnemyInitializer] Enemy_AnimationEventHandler not found!");
            return;
        }

        animHandler.Initalize();

        var animBridge = GetComponent<EnemyAnimationBridge>();
        if (animBridge == null)
        {
            Debug.LogError("[EnemyInitializer] EnemyAnimationBridge not found!");
            return;
        }
        animBridge.Initialize(_enemy, animator);
        _enemy._animationBridge = animBridge;
        Log("Initialized Animation System");
    }
    private void InitializeHealthSystem(bool skipCache)
    {
        var health = GetOrGetComponent<EnemyHealth>(skipCache);
        if (health == null)
        {
            Debug.LogError("EnemyHealth component is missing.");
            return;
        }
        health.InitializeHealth(_enemy);
        Log("Initialized Health System");
    }
    private void InitializeSpecialAbility(bool skipCache)
    {
        var specialAbility = GetOrGetComponent<EnemySpecialAbility>(skipCache);
        if (specialAbility == null)
        {
            Debug.LogError("[EnemyInitializer] EnemySpecialAbility not found!");
            return;
        }

        specialAbility.Initialize(_enemy);
        Log("  - Special ability initialized");
    }
    private void InitializeParrySystem(bool skipCache)
    {
        var parrySystem = GetOrGetComponent<ParrySystem>(skipCache);
        if (parrySystem == null)
        {
            Debug.LogError("[EnemyInitializer] ParrySystem not found!");
            return;
        }

        parrySystem.Initialize(_enemy);
        Log("  - Parry system initialized");
    }

    private void InitializeStiffnessSystem(bool skipCache)
    {
        var stiffnessSystem = GetOrGetComponent<Mon_Stiffness>(skipCache);
        if (stiffnessSystem == null)
        {
            Debug.LogError("[EnemyInitializer] Mon_Stiffness not found!");
            return;
        }

        stiffnessSystem.Initialize(_enemy);
        Log("  - Stiffness system initialized");
    }


    // getorgetcomponent : 캐시 사용 옵션 포함
    private T GetOrGetComponent<T>(bool skipCache) where T : Component
    {
        if (skipCache && _componentCache.TryGetValue(typeof(T), out var cached))
        {
            return cached as T;
        }

        var component = GetComponent<T>();
        if (component != null)
        {
            _componentCache[typeof(T)] = component;
        }
        return component;
    }
    #endregion
    #region Phase 4 : AI 및 이동 초기화
    private void Phase4_InitializeAI()
    {
        Log("Phase 4 : Initializing AI and Movement");
        var aiPath = GetComponent<AIPath>();
        if (aiPath == null)
        {
            Debug.LogError("AIPath component is missing.");
            return;
        }
        var aiController = GetComponent<AiController>();
        if (aiController == null)
        {
            Debug.LogError("AiController component is missing.");
            return;
        }
        aiController.Initialize(_enemy);

        var movement = GetComponent<EnemyMovement>();
        if (movement == null)
        {
            Debug.LogError("EnemyMovement component is missing.");
            return;
        }
        movement.Initialize(_enemy);
        _enemy.Movement = movement;

        movement.StopMovement();

        MarkInitialized("AI");
        Log("Phase 4 complete");
    }
    #endregion
    #region Phase 5 : 그룹 AI 등록
    private void Phase5_RegisterGroup()
    {
        Log("Phase 5 : Registering to Group AI");
        if (_groupAi != null)
        {
            _groupAi.GroupAdd(_enemy);
            Log(" Registered to Group AI");
        }
        else
        {
            Debug.LogWarning("GroupAi instance not found. Skipping registration.");
        }
        MarkInitialized("GroupAI");
        Log("Phase 5 complete");
    }
    #endregion
    #region Phase 6 : 최종 상태 설정
    private void Phase6_FinalizeState()
    {
        Log("Phase 6 : Finalizing State");
        _enemy.SetState(EnemyStateController.EnemyState.Idle);



        // 액티브 상태로 설정
        _enemy.gameObject.SetActive(true);

        MarkInitialized("Final");
        Log("Phase 6 complete");
        Log("All systems initialized!");
    }
    #endregion

    #region 유틸리티 메서드
    private void MarkInitialized(string systemName)
    {
        _initializedSystems.Add(systemName);
        Log($"Marked {systemName} as initialized.");
    }
    public bool IsInitialized(string systemName)
    {
        return _initializedSystems.Contains(systemName);
    }

    private void OnDestroy()
    {
        _componentCache.Clear();
        _initializedSystems.Clear();
    }
    #endregion
    #region 디버깅 및 에디터
#if UNITY_EDITOR
    private void OnValidate()
    {

    }
#endif
    #endregion
}