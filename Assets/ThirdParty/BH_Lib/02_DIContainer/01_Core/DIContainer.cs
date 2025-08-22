using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static DIContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DIContainer();
                }
                return _instance;
            }
        }
        #endregion

        #region Private Variables
        // 타입별 등록 정보를 저장하는 딕셔너리
        private readonly Dictionary<Type, ServiceRegistration> _registrations = new Dictionary<Type, ServiceRegistration>();
        
        // 특정 ID를 가진 등록 정보를 저장하는 딕셔너리
        private readonly Dictionary<string, ServiceRegistration> _namedRegistrations = new Dictionary<string, ServiceRegistration>();
        
        // 싱글톤 인스턴스를 저장하는 딕셔너리
        private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();
        
        // 씬별 인스턴스를 저장하는 딕셔너리
        private readonly Dictionary<string, Dictionary<Type, object>> _sceneInstances = new Dictionary<string, Dictionary<Type, object>>();
        
        // 현재 활성 씬 이름
        private string _currentSceneName;
        #endregion

        #region Public Methods

        /// <summary>
        /// 모든 등록된 서비스를 초기화합니다.
        /// </summary>
        public void ResetContainer()
        {
            _registrations.Clear();
            _namedRegistrations.Clear();
            _singletonInstances.Clear();
            
            // 씬 관련 인스턴스 정리
            foreach (var sceneDict in _sceneInstances.Values)
            {
                // IDisposable 인스턴스 정리
                foreach (var instance in sceneDict.Values)
                {
                    if (instance is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                sceneDict.Clear();
            }
            _sceneInstances.Clear();
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
            if (!_registrations.TryGetValue(serviceType, out var registration))
            {
                throw new Exception($"No registration found for {serviceType.Name}");
            }

            var instance = CreateInstance(registration);
            if (instance == null && registration.Lifetime == LifetimeScope.Scene)
            {
                // 씬 제약 조건에 의해 생성되지 않은 경우에 대한 처리
                Debug.Log($"Service {serviceType.Name} was not created due to scene constraints");
            }
            return instance;
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
                throw new Exception($"No registration found for ID: {id}");
            }

            return CreateInstance(registration);
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

            // 필드 주입
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var injectAttribute = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null)
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
            }

            // 프로퍼티 주입
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var injectAttribute = property.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null && property.CanWrite)
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
            }

            // 메소드 주입
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var injectAttribute = method.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null)
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
                _sceneInstances[sceneName] = new Dictionary<Type, object>();
            }
        }

        /// <summary>
        /// 씬 컨테이너를 정리합니다.
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        public void CleanupSceneContainer(string sceneName)
        {
            if (_sceneInstances.TryGetValue(sceneName, out var instances))
            {
                // IDisposable 인스턴스 정리
                foreach (var instance in instances.Values)
                {
                    if (instance is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                
                instances.Clear();
                _sceneInstances.Remove(sceneName);
                
                // 만약 현재 씬이 삭제된 씬이라면 현재 씬 캐싱 초기화
                if (_currentSceneName == sceneName)
                {
                    _currentSceneName = null;
                }
            }
        }
        
        /// <summary>
        /// 현재 씬의 이름을 가져옵니다.
        /// </summary>
        /// <returns>현재 씬 이름</returns>
        private string GetCurrentSceneName()
        {
            if (string.IsNullOrEmpty(_currentSceneName))
            {
                _currentSceneName = SceneManager.GetActiveScene().name;
            }
            return _currentSceneName;
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
            
            // 씬 딕셔너리 확인 또는 생성
            if (!_sceneInstances.TryGetValue(sceneName, out var sceneDict))
            {
                sceneDict = new Dictionary<Type, object>();
                _sceneInstances[sceneName] = sceneDict;
            }
            
            // 기존 인스턴스 있으면 반환
            if (sceneDict.TryGetValue(registration.ServiceType, out var instance))
            {
                return instance;
            }
            
            // 새 인스턴스 생성
            instance = CreateAndInjectInstance(registration.ImplementationType);
            if (instance != null)
            {
                sceneDict[registration.ServiceType] = instance;
            }
            
            return instance;
        }

        private object CreateAndInjectInstance(Type type)
        {
            // MonoBehaviour 타입 체크
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                // 이미 씬에 존재하는지 확인
                var existing = GameObject.FindFirstObjectByType(type) as MonoBehaviour;
                if (existing != null)
                {
                    InjectInto(existing);
                    return existing;
                }

                // 없으면 새 GameObject 생성하고 컴포넌트 추가
                var gameObject = new GameObject(type.Name);
                var component = gameObject.AddComponent(type) as MonoBehaviour;
                InjectInto(component);
                return component;
            }

            // 생성자 선택 - [Inject] 어트리뷰트가 있는 생성자 또는 매개변수가 가장 많은 생성자
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
