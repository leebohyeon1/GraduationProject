using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BH_Lib.DI
{
    using Log;
    /// <summary>
    /// DI 컨테이너의 핵심 클래스입니다.
    /// 의존성 등록 및 해결을 담당합니다.
    /// </summary>
    public class DIContainer : IDIContainer
    {
        #region Singleton
        private static DIContainer _instance;
        private static readonly object _lock = new object();
        
        public static DIContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DIContainer();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Private Variables
        // Thread-safe 컬렉션으로 변경
        private readonly ConcurrentDictionary<Type, ServiceRegistration> _registrations = new ConcurrentDictionary<Type, ServiceRegistration>();
        private readonly ConcurrentDictionary<string, ServiceRegistration> _namedRegistrations = new ConcurrentDictionary<string, ServiceRegistration>();
        private readonly ConcurrentDictionary<Type, object> _singletonInstances = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, object>> _sceneInstances = new ConcurrentDictionary<string, ConcurrentDictionary<Type, object>>();
        
        // 현재 활성 씬 이름 (thread-safe 접근을 위한 lock)
        private string _currentSceneName;
        private readonly ReaderWriterLockSlim _sceneNameLock = new ReaderWriterLockSlim();
        
        // 순환 의존성 감지를 위한 ThreadLocal 스택
        private readonly ThreadLocal<HashSet<Type>> _resolvingTypes = new ThreadLocal<HashSet<Type>>(() => new HashSet<Type>());
        
        // 리플렉션 캐시
        private readonly ReflectionCache _reflectionCache = new ReflectionCache();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, object>> _sceneMonoBehaviourCache = new ConcurrentDictionary<string, ConcurrentDictionary<Type, object>>();
        #endregion

        #region Public Methods

        /// <summary>
        /// 모든 등록된 서비스를 초기화합니다.
        /// </summary>
        public void ResetContainer()
        {
            // Thread-safe clear operations
            _registrations.Clear();
            _namedRegistrations.Clear();
            _singletonInstances.Clear();
            
            // 씬 관련 인스턴스 정리 (thread-safe)
            var sceneKeys = _sceneInstances.Keys.ToList();
            foreach (var sceneKey in sceneKeys)
            {
                if (_sceneInstances.TryRemove(sceneKey, out var sceneDict))
                {
                    // IDisposable 인스턴스 정리
                    var instances = sceneDict.Values.ToList();
                    foreach (var instance in instances)
                    {
                        if (instance is IDisposable disposable)
                        {
                            try
                            {
                                disposable.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Log.PrintErr($"Error disposing instance: {ex.Message}");
                            }
                        }
                    }
                    sceneDict.Clear();
                }
            }
            
            // 씬 이름 초기화
            _sceneNameLock.EnterWriteLock();
            try
            {
                _currentSceneName = null;
            }
            finally
            {
                _sceneNameLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 서비스를 등록합니다.
        /// </summary>
        /// <typeparam name="TService">서비스 타입</typeparam>
        /// <typeparam name="TImplementation">구현 타입</typeparam>
        /// <param name="lifetime">생명주기</param>
        /// <param name="id">선택적 ID</param>
        public void Register<TService, TImplementation>(LifetimeScope lifetime = LifetimeScope.Singleton, string id = null)
            where TImplementation : TService
        {
            Register(typeof(TService), typeof(TImplementation), lifetime, id);
        }

        /// <summary>
        /// 서비스를 등록합니다.
        /// </summary>
        /// <param name="serviceType">서비스 타입</param>
        /// <param name="implementationType">구현 타입</param>
        /// <param name="lifetime">생명주기</param>
        /// <param name="id">선택적 ID</param>
        public void Register(Type serviceType, Type implementationType, LifetimeScope lifetime = LifetimeScope.Singleton, string id = null)
        {
            // 입력 유효성 검사
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType), "Service type cannot be null");
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType), "Implementation type cannot be null");
            if (!serviceType.IsAssignableFrom(implementationType))
                throw new ArgumentException($"Implementation type '{implementationType.FullName}' does not implement service type '{serviceType.FullName}'", nameof(implementationType));
            if (implementationType.IsAbstract || implementationType.IsInterface)
                throw new ArgumentException($"Implementation type '{implementationType.FullName}' cannot be abstract or interface", nameof(implementationType));

            var registration = new ServiceRegistration
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = lifetime
            };

            _registrations[serviceType] = registration;

            if (!string.IsNullOrEmpty(id))
            {
                _namedRegistrations[id] = registration;
            }
        }

        /// <summary>
        /// 인스턴스를 직접 등록합니다.
        /// </summary>
        /// <typeparam name="TService">서비스 타입</typeparam>
        /// <param name="instance">등록할 인스턴스</param>
        /// <param name="id">선택적 ID</param>
        public void RegisterInstance<TService>(TService instance, string id = null)
        {
            RegisterInstance(typeof(TService), instance, id);
        }

        /// <summary>
        /// 인스턴스를 직접 등록합니다.
        /// </summary>
        /// <param name="serviceType">서비스 타입</param>
        /// <param name="instance">등록할 인스턴스</param>
        /// <param name="id">선택적 ID</param>
        public void RegisterInstance(Type serviceType, object instance, string id = null)
        {
            var registration = new ServiceRegistration
            {
                ServiceType = serviceType,
                ImplementationType = instance.GetType(),
                Lifetime = LifetimeScope.Singleton,
                SingletonInstance = instance
            };

            _registrations[serviceType] = registration;
            _singletonInstances[serviceType] = instance;

            if (!string.IsNullOrEmpty(id))
            {
                _namedRegistrations[id] = registration;
            }
        }

        /// <summary>
        /// 특정 타입의 서비스를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">서비스 타입</typeparam>
        /// <returns>서비스 인스턴스</returns>
        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// 특정 ID로 등록된 서비스를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">서비스 타입</typeparam>
        /// <param name="id">서비스 ID</param>
        /// <returns>서비스 인스턴스</returns>
        public T ResolveById<T>(string id)
        {
            return (T)ResolveById(id);
        }

        /// <summary>
        /// 특정 타입의 서비스를 가져옵니다.
        /// </summary>
        /// <param name="serviceType">서비스 타입</param>
        /// <returns>서비스 인스턴스</returns>
        public object Resolve(Type serviceType)
        {
            // 순환 의존성 검사
            if (_resolvingTypes.Value.Contains(serviceType))
            {
                var circularPath = string.Join(" -> ", _resolvingTypes.Value.Select(t => t.Name));
                throw new InvalidOperationException($"Circular dependency detected: {circularPath} -> {serviceType.Name}");
            }

            if (!_registrations.TryGetValue(serviceType, out var registration))
            {
                throw new InvalidOperationException($"No registration found for type '{serviceType.FullName}'. Please ensure the type is registered in the DI container.");
            }

            // 의존성 해결 시작
            _resolvingTypes.Value.Add(serviceType);
            try
            {
                var instance = CreateInstance(registration);
                if (instance == null && registration.Lifetime == LifetimeScope.Scene)
                {
                    Log.Print($"Service '{serviceType.Name}' was not created due to scene constraints");
                }
                return instance;
            }
            finally
            {
                // 해결 완료 후 스택에서 제거
                _resolvingTypes.Value.Remove(serviceType);
            }
        }

        /// <summary>
        /// 특정 ID로 등록된 서비스를 가져옵니다.
        /// </summary>
        /// <param name="id">서비스 ID</param>
        /// <returns>서비스 인스턴스</returns>
        public object ResolveById(string id)
        {
            if (!_namedRegistrations.TryGetValue(id, out var registration))
            {
                throw new InvalidOperationException($"No registration found for ID '{id}'. Please ensure the service is registered with this ID.");
            }

            // ID 기반 해결도 동일한 순환 의존성 검사 적용
            return Resolve(registration.ServiceType);
        }

        /// <summary>
        /// 특정 객체에 의존성을 주입합니다.
        /// </summary>
        /// <param name="instance">의존성을 주입할 객체</param>
        public void InjectInto(object instance)
        {
            if (instance == null)
                return;

            var type = instance.GetType();

            // 캐시된 필드 정보로 필드 주입
            var fields = _reflectionCache.GetFields(type);
            
            foreach (var field in fields)
            {
                var injectAttribute = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null)
                {
                    try
                    {
                        object dependency;
                        if (string.IsNullOrEmpty(injectAttribute.Id))
                        {
                            dependency = Resolve(field.FieldType);
                        }
                        else
                        {
                            dependency = ResolveById(injectAttribute.Id);
                        }
                        field.SetValue(instance, dependency);
                    }
                    catch (Exception ex)
                    {
                        Log.PrintErr($"Failed to inject field '{field.Name}' in type '{type.Name}': {ex.Message}");
                    }
                }
            }

            // 캐시된 프로퍼티 정보로 프로퍼티 주입
            var properties = _reflectionCache.GetProperties(type);
            
            foreach (var property in properties)
            {
                var injectAttribute = property.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null && property.CanWrite)
                {
                    try
                    {
                        object dependency;
                        if (string.IsNullOrEmpty(injectAttribute.Id))
                        {
                            dependency = Resolve(property.PropertyType);
                        }
                        else
                        {
                            dependency = ResolveById(injectAttribute.Id);
                        }
                        property.SetValue(instance, dependency);
                    }
                    catch (Exception ex)
                    {
                        Log.PrintErr($"Failed to inject property '{property.Name}' in type '{type.Name}': {ex.Message}");
                    }
                }
            }

            // 캐시된 메소드 정보로 메소드 주입
            var methods = _reflectionCache.GetMethods(type);
            
            foreach (var method in methods)
            {
                var injectAttribute = method.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null)
                {
                    try
                    {
                        var parameters = method.GetParameters();
                        var args = new object[parameters.Length];

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var paramInfo = parameters[i];
                            var paramInjectAttr = paramInfo.GetCustomAttribute<InjectAttribute>();
                            
                            if (paramInjectAttr != null && !string.IsNullOrEmpty(paramInjectAttr.Id))
                            {
                                args[i] = ResolveById(paramInjectAttr.Id);
                            }
                            else
                            {
                                args[i] = Resolve(paramInfo.ParameterType);
                            }
                        }

                        method.Invoke(instance, args);
                    }
                    catch (Exception ex)
                    {
                        Log.PrintErr($"Failed to inject method '{method.Name}' in type '{type.Name}': {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Assembly에서 RegisterAttribute가 지정된 모든 타입을 자동으로 등록합니다.
        /// </summary>
        public void RegisterAssemblyTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    RegisterAssemblyTypes(assembly);
                }
                catch (Exception ex)
                {
                    Log.PrintErr($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 특정 Assembly에서 RegisterAttribute가 지정된 모든 타입을 자동으로 등록합니다.
        /// </summary>
        /// <param name="assembly">스캔할 Assembly</param>
        public void RegisterAssemblyTypes(Assembly assembly)
        {
            if (assembly == null)
                return;

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract)
                        continue;

                    var registerAttribute = type.GetCustomAttribute<RegisterAttribute>();
                    if (registerAttribute != null)
                    {
                        if (registerAttribute.AsTypes != null && registerAttribute.AsTypes.Length > 0)
                        {
                            // 지정된 인터페이스로 등록
                            foreach (var serviceType in registerAttribute.AsTypes)
                            {
                                Register(serviceType, type, registerAttribute.Lifetime);
                            }
                        }
                        else
                        {
                            // 자기 자신의 타입으로 등록
                            Register(type, type, registerAttribute.Lifetime);

                            // 구현하는 모든 인터페이스로도 등록
                            foreach (var interfaceType in type.GetInterfaces())
                            {
                                // 기본 인터페이스 제외 (IDisposable 등)
                                if (interfaceType.Namespace != null && 
                                    !interfaceType.Namespace.StartsWith("System."))
                                {
                                    Register(interfaceType, type, registerAttribute.Lifetime);
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Log.PrintErr($"Error scanning types in assembly {assembly.FullName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 씬 컨테이너를 초기화합니다.
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        public void InitializeSceneContainer(string sceneName)
        {
            _currentSceneName = sceneName;
            if (!_sceneInstances.ContainsKey(sceneName))
            {
                _sceneInstances[sceneName] = new ConcurrentDictionary<Type, object>();
            }
        }

        /// <summary>
        /// 씬 컨테이너를 정리합니다.
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        public void CleanupSceneContainer(string sceneName)
        {
            if (_sceneInstances.TryRemove(sceneName, out var instances))
            {
                // IDisposable 인스턴스 정리 (thread-safe)
                var instanceValues = instances.Values.ToList();
                foreach (var instance in instanceValues)
                {
                    DisposeInstance(instance);
                }
                
                instances.Clear();
                
                // 현재 씬 캐싱 초기화 (thread-safe)
                _sceneNameLock.EnterWriteLock();
                try
                {
                    if (_currentSceneName == sceneName)
                    {
                        _currentSceneName = null;
                    }
                }
                finally
                {
                    _sceneNameLock.ExitWriteLock();
                }
            }
        }
        
        /// <summary>
        /// 인스턴스를 안전하게 해제합니다.
        /// </summary>
        /// <param name="instance">해제할 인스턴스</param>
        private void DisposeInstance(object instance)
        {
            if (instance == null) return;
            
            try
            {
                // MonoBehaviour는 Unity에서 관리하므로 Destroy 호출
                if (instance is MonoBehaviour monoBehaviour && monoBehaviour != null)
                {
                    UnityEngine.Object.Destroy(monoBehaviour.gameObject);
                }
                // 일반 IDisposable 객체
                else if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.PrintErr($"Error disposing instance of type {instance.GetType().Name}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 현재 씬의 이름을 가져옵니다.
        /// </summary>
        /// <returns>현재 씬 이름</returns>
        private string GetCurrentSceneName()
        {
            _sceneNameLock.EnterReadLock();
            try
            {
                if (string.IsNullOrEmpty(_currentSceneName))
                {
                    _sceneNameLock.ExitReadLock();
                    _sceneNameLock.EnterWriteLock();
                    try
                    {
                        // Double-checked locking
                        if (string.IsNullOrEmpty(_currentSceneName))
                        {
                            _currentSceneName = SceneManager.GetActiveScene().name;
                        }
                        return _currentSceneName;
                    }
                    finally
                    {
                        _sceneNameLock.ExitWriteLock();
                    }
                }
                return _currentSceneName;
            }
            finally
            {
                if (_sceneNameLock.IsReadLockHeld)
                {
                    _sceneNameLock.ExitReadLock();
                }
            }
        }
        
        /// <summary>
        /// 등록된 모든 서비스 정보를 가져옵니다.
        /// </summary>
        /// <returns>등록된 서비스들의 딕셔너리</returns>
        public Dictionary<Type, ServiceRegistration> GetRegistrations()
        {
            return new Dictionary<Type, ServiceRegistration>(_registrations);
        }
        #endregion

        #region Private Methods
        private object CreateInstance(ServiceRegistration registration)
        {
            object instance = null;
            
            switch (registration.Lifetime)
            {
                case LifetimeScope.Singleton:
                    instance = GetOrCreateSingletonInstance(registration);
                    break;
                case LifetimeScope.Scene:
                    instance = GetOrCreateSceneInstance(registration);
                    break;
                case LifetimeScope.Transient:
                    // Transient 인스턴스도 씬 제약 조건 확인
                    var sceneConstraint = registration.ImplementationType.GetCustomAttribute<SceneConstraintAttribute>();
                    if (sceneConstraint != null)
                    {
                        string sceneName = GetCurrentSceneName();
                        int sceneBuildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                        
                        bool isAllowed = false;
                        
                        // 씬 이름 체크
                        if (sceneConstraint.SceneNames != null && sceneConstraint.SceneNames.Length > 0)
                        {
                            isAllowed = Array.Exists(sceneConstraint.SceneNames, s => s == sceneName);
                        }
                        
                        // 씬 인덱스 체크
                        if (!isAllowed && sceneConstraint.SceneIndices != null && sceneConstraint.SceneIndices.Length > 0)
                        {
                            isAllowed = Array.Exists(sceneConstraint.SceneIndices, i => i == sceneBuildIndex);
                        }
                        
                        // 허용되지 않은 씬이면 null 반환
                        if (!isAllowed)
                        {
                            Debug.Log($"Skipping creation of {registration.ImplementationType.Name} due to scene constraint. Current scene: {sceneName} (Build Index: {sceneBuildIndex})");
                            return null;
                        }
                    }
                    
                    instance = CreateAndInjectInstance(registration.ImplementationType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return instance;
        }

        private object GetOrCreateSingletonInstance(ServiceRegistration registration)
        {
            // 이미 생성된 인스턴스가 있으면 반환
            if (registration.SingletonInstance != null)
                return registration.SingletonInstance;

            if (_singletonInstances.TryGetValue(registration.ServiceType, out var instance))
                return instance;

            // 새 인스턴스 생성
            instance = CreateAndInjectInstance(registration.ImplementationType);
            _singletonInstances[registration.ServiceType] = instance;
            registration.SingletonInstance = instance;

            // MonoBehaviour 타입이면 DontDestroyOnLoad 적용
            if (instance is MonoBehaviour monoBehaviour)
            {
                UnityEngine.Object.DontDestroyOnLoad(monoBehaviour.gameObject);
            }

            return instance;
        }
        
        private object GetOrCreateSceneInstance(ServiceRegistration registration)
        {
            // 현재 씬 이름 가져오기
            string sceneName = GetCurrentSceneName();
            int sceneBuildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            
            // 씬 제약 조건 확인
            var sceneConstraint = registration.ImplementationType.GetCustomAttribute<SceneConstraintAttribute>();
            if (sceneConstraint != null)
            {
                bool isAllowed = false;
                
                // 씬 이름 체크
                if (sceneConstraint.SceneNames != null && sceneConstraint.SceneNames.Length > 0)
                {
                    isAllowed = Array.Exists(sceneConstraint.SceneNames, s => s == sceneName);
                }
                
                // 씬 인덱스 체크
                if (!isAllowed && sceneConstraint.SceneIndices != null && sceneConstraint.SceneIndices.Length > 0)
                {
                    isAllowed = Array.Exists(sceneConstraint.SceneIndices, i => i == sceneBuildIndex);
                }
                
                // 허용되지 않은 씬이면 null 반환
                if (!isAllowed)
                {
                    Debug.Log($"Skipping creation of {registration.ImplementationType.Name} due to scene constraint. Current scene: {sceneName} (Build Index: {sceneBuildIndex})");
                    return null;
                }
            }
            
            // 씬 딕셔너리 확인 또는 생성 (thread-safe)
            var sceneDict = _sceneInstances.GetOrAdd(sceneName, _ => new ConcurrentDictionary<Type, object>());
            
            // 기존 인스턴스 있으면 반환
            if (sceneDict.TryGetValue(registration.ServiceType, out var instance))
            {
                return instance;
            }
            
            // 새 인스턴스 생성 (thread-safe)
            instance = CreateAndInjectInstance(registration.ImplementationType);
            if (instance != null)
            {
                sceneDict.TryAdd(registration.ServiceType, instance);
            }
            
            return instance;
        }

        private object CreateAndInjectInstance(Type type)
        {
            // MonoBehaviour 타입 체크
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                return CreateMonoBehaviourInstance(type);
            }

            return CreateRegularInstance(type);
        }
        
        /// <summary>
        /// MonoBehaviour 인스턴스를 생성합니다 (캐싱 적용).
        /// </summary>
        private object CreateMonoBehaviourInstance(Type type)
        {
            string sceneName = GetCurrentSceneName();
            var sceneCache = _sceneMonoBehaviourCache.GetOrAdd(sceneName, _ => new ConcurrentDictionary<Type, object>());
            
            // 캐시에서 확인
            if (sceneCache.TryGetValue(type, out var cachedInstance))
            {
                var cachedMono = cachedInstance as MonoBehaviour;
                if (cachedMono != null && cachedMono.gameObject != null)
                {
                    return cachedInstance;
                }
                else
                {
                    // 캐시된 객체가 파괴되었으면 제거
                    sceneCache.TryRemove(type, out _);
                }
            }
            
            // 씬에서 기존 객체 찾기 (한 번만 수행)
            var existing = GameObject.FindFirstObjectByType(type) as MonoBehaviour;
            if (existing != null)
            {
                sceneCache.TryAdd(type, existing);
                InjectInto(existing);
                return existing;
            }

            // 새 GameObject 생성
            var gameObject = new GameObject(type.Name);
            var component = gameObject.AddComponent(type) as MonoBehaviour;
            sceneCache.TryAdd(type, component);
            InjectInto(component);
            return component;
        }
        
        /// <summary>
        /// 일반 클래스 인스턴스를 생성합니다 (캐시된 리플렉션 사용).
        /// </summary>
        private object CreateRegularInstance(Type type)
        {
            // 캐시된 생성자 정보 사용
            var constructors = _reflectionCache.GetConstructors(type);
            
            ConstructorInfo targetConstructor = null;
            
            // 먼저 [Inject] 어트리뷰트가 있는 생성자 검색
            foreach (var constructor in constructors)
            {
                if (constructor.GetCustomAttribute<InjectAttribute>() != null)
                {
                    targetConstructor = constructor;
                    break;
                }
            }
            
            // 없으면 매개변수가 가장 많은 public 생성자 선택
            if (targetConstructor == null)
            {
                targetConstructor = constructors
                    .Where(c => c.IsPublic)
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();
            }
            
            // 그래도 없으면 기본 생성자 사용
            if (targetConstructor == null)
            {
                targetConstructor = type.GetConstructor(Type.EmptyTypes);
                
                if (targetConstructor == null)
                {
                    throw new Exception($"No suitable constructor found for type {type.Name}");
                }
                
                // 기본 생성자로 인스턴스 생성
                var instance = Activator.CreateInstance(type);
                InjectInto(instance);
                return instance;
            }
            
            // 선택된 생성자의 매개변수에 의존성 주입
            var parameters = targetConstructor.GetParameters();
            var arguments = new object[parameters.Length];
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameterInjectAttr = parameter.GetCustomAttribute<InjectAttribute>();
                
                if (parameterInjectAttr != null && !string.IsNullOrEmpty(parameterInjectAttr.Id))
                {
                    arguments[i] = ResolveById(parameterInjectAttr.Id);
                }
                else
                {
                    arguments[i] = Resolve(parameter.ParameterType);
                }
            }


            // 인스턴스 생성 및 추가 의존성 주입
            var instanceObj = targetConstructor.Invoke(arguments);
            InjectInto(instanceObj);
            
            return instanceObj;
        }
        #endregion

        /// <summary>
        /// 서비스 등록 정보를 저장하는 클래스
        /// </summary>
        public class ServiceRegistration
        {
            public Type ServiceType { get; set; }
            public Type ImplementationType { get; set; }
            public LifetimeScope Lifetime { get; set; }
            public object SingletonInstance { get; set; }
        }
    }
}
