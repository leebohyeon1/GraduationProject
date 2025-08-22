using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Reflection;

namespace BH_Lib.DI
{
    using Log;
    /// <summary>
    /// DI 컨테이너를 초기화하는 MonoBehaviour 컴포넌트입니다.
    /// 게임 초기화 단계에서 가장 먼저 실행되도록 설정되어 있습니다.
    /// 중복 초기화를 방지하고 기존 MonoBehaviour의 DontDestroyOnLoad 문제를 해결합니다.
    /// </summary>
    [DefaultExecutionOrder(-9999)]
    public class DIContainerInitializer : MonoBehaviour
    {
        [SerializeField, Tooltip("게임 전체에서 DI 컨테이너를 유지할지 여부")]
        private bool _dontDestroyOnLoad = true;
        
        [SerializeField, Tooltip("초기화 로그를 출력할지 여부")]
        private bool _enableLogs = true;
        
        [SerializeField, Tooltip("씬에 수동 배치된 Singleton MonoBehaviour 자동 처리 여부")]
        private bool _processManualSingletons = true;

        private IDIContainer _container;
        
        // 중복 초기화 방지를 위한 정적 변수
        private static bool _isInitialized = false;
        private static readonly object _initLock = new object();
        
        private void Awake()
        {
            // 중복 초기화 방지 (Thread-safe)
            lock (_initLock)
            {
                if (_isInitialized)
                {
                    if (_enableLogs)
                    {
                        Log.Print("DI Container already initialized. Destroying duplicate initializer.");
                    }
                    Destroy(gameObject);
                    return;
                }
                
                _isInitialized = true;
            }

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            // DI 컨테이너 인스턴스 참조
            _container = DIContainer.Instance;

            // [Register] 어트리뷰트를 가진 클래스 검색 및 등록
            _container.RegisterAssemblyTypes();
            
            // 현재 씬 컨테이너 초기화
            InitializeCurrentSceneContainer();
            
            // 씬에 수동 배치된 Singleton 컴포넌트들 처리 (개선된 로직)
            if (_processManualSingletons)
            {
                ProcessManualSingletons();
            }
            
            // 등록된 Singleton MonoBehaviour들 자동 생성
            ResolveRegisteredSingletons();
            
            // 씬 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            if (_enableLogs)
            {
                Log.Print("DI Container initialized successfully");
            }
        }
        
        private void OnDestroy()
        {
            // 씬 이벤트 구독 해제
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
        
        private void InitializeCurrentSceneContainer()
        {
            var currentScene = SceneManager.GetActiveScene();
            
            if (_container is DIContainer diContainer)
            {
                diContainer.InitializeSceneContainer(currentScene.name);
                
                if (_enableLogs)
                {
                    Log.Print($"Initialized scene container for: {currentScene.name}");
                }
            }
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_container is DIContainer diContainer)
            {
                diContainer.InitializeSceneContainer(scene.name);
                
                if (_enableLogs)
                {
                    Log.Print($"Scene loaded: {scene.name}, initialized scene container");
                }
            }
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            if (_container is DIContainer diContainer)
            {
                diContainer.CleanupSceneContainer(scene.name);
                
                if (_enableLogs)
                {
                    Log.Print($"Scene unloaded: {scene.name}, cleaned up scene container");
                }
            }
        }
        
        /// <summary>
        /// 씬에 수동으로 배치된 Singleton MonoBehaviour들을 찾아서 DIContainer에 등록하고 DontDestroyOnLoad 적용
        /// </summary>
        private void ProcessManualSingletons()
        {
            if (!(_container is DIContainer diContainer))
                return;

            // 현재 씬의 모든 MonoBehaviour 중에서 [Register(LifetimeScope.Singleton)] 어트리뷰트를 가진 것들 찾기
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var monoBehaviour in allMonoBehaviours)
            {
                var type = monoBehaviour.GetType();
                var registerAttribute = type.GetCustomAttribute<RegisterAttribute>();
                
                if (registerAttribute != null && registerAttribute.Lifetime == LifetimeScope.Singleton)
                {
                    try
                    {
                        // DIContainer에 기존 인스턴스로 등록
                        if (registerAttribute.AsTypes != null && registerAttribute.AsTypes.Length > 0)
                        {
                            // 지정된 인터페이스들로 등록
                            foreach (var serviceType in registerAttribute.AsTypes)
                            {
                                _container.RegisterInstance(serviceType, monoBehaviour);
                            }
                        }
                        else
                        {
                            // 자기 자신의 타입으로 등록
                            _container.RegisterInstance(type, monoBehaviour);
                            
                            // 구현하는 모든 인터페이스로도 등록
                            foreach (var interfaceType in type.GetInterfaces())
                            {
                                if (interfaceType.Namespace != null && 
                                    !interfaceType.Namespace.StartsWith("System."))
                                {
                                    _container.RegisterInstance(interfaceType, monoBehaviour);
                                }
                            }
                        }
                        
                        // DontDestroyOnLoad 적용
                        DontDestroyOnLoad(monoBehaviour.gameObject);
                        
                        if (_enableLogs)
                        {
                            Log.Print($"Processed manual singleton: {type.Name} -> DontDestroyOnLoad applied");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (_enableLogs)
                        {
                            Log.PrintErr($"Failed to process manual singleton {type.Name}: {ex.Message}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 등록된 Singleton MonoBehaviour들을 자동으로 생성 (씬에 수동 배치된 것들 제외)
        /// </summary>
        private void ResolveRegisteredSingletons()
        {
            if (!(_container is DIContainer diContainer))
                return;
                
            // 등록된 모든 타입 중에서 Singleton이면서 MonoBehaviour인 것들을 찾기
            var registrations = diContainer.GetRegistrations();
            
            foreach (var registration in registrations)
            {
                if (registration.Value.Lifetime == LifetimeScope.Singleton && 
                    typeof(MonoBehaviour).IsAssignableFrom(registration.Value.ImplementationType))
                {
                    // 이미 등록된 인스턴스가 있는지 확인 (수동 배치된 것들)
                    if (registration.Value.SingletonInstance != null)
                    {
                        if (_enableLogs)
                        {
                            Log.Print($"Skipping auto-resolve for manual singleton: {registration.Value.ImplementationType.Name}");
                        }
                        continue;
                    }
                    
                    try
                    {
                        // Resolve 호출하여 새 인스턴스 생성 및 DontDestroyOnLoad 적용
                        _container.Resolve(registration.Key);
                        
                        if (_enableLogs)
                        {
                            Log.Print($"Auto-resolved registered singleton: {registration.Value.ImplementationType.Name}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (_enableLogs)
                        {
                            Log.PrintErr($"Failed to auto-resolve singleton {registration.Value.ImplementationType.Name}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}