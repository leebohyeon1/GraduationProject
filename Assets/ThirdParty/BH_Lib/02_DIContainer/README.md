# BH_Lib DIContainer

Unity 프로젝트를 위한 가볍고 강력한 의존성 주입(DI) 시스템입니다. SOLID 원칙을 따르는 고품질 코드 작성과 깔끔한 아키텍처 설계를 지원합니다.

## 주요 기능

- **타입 기반 의존성 주입**: 인터페이스-구현 관계의 분리와 테스트 용이성 제공
- **다양한 수명 주기(LifetimeScope) 관리**: Singleton, Scene, Transient 범위 지원
- **씬별 제약 조건**: 특정 씬에서만 객체가 생성되도록 제어
- **자동 등록**: 어트리뷰트를 통한 간편한 DI 컨테이너 등록
- **MonoBehaviour 통합**: Unity 컴포넌트에 의존성 주입 지원
- **코드 기반 설정**: 명시적인 코드로 의존성 관계 설정 가능
- **자동 메모리 관리**: 씬 전환 시 자동 객체 정리 및 자원 해제

## 시작하기

### 1. 기본 사용법

```csharp
// 서비스 인터페이스
public interface IDataService 
{
    string GetData();
}

// 서비스 구현
[Register(LifetimeScope.Singleton)]
public class DataService : IDataService 
{
    public string GetData() 
    {
        return "Some data";
    }
}

// 서비스 사용
public class GameManager : MonoBehaviour 
{
    [Inject] private IDataService _dataService;
    
    private void Start() 
    {
        Debug.Log(_dataService.GetData());
    }
}
```

### 2. 수동 등록

```csharp
// 시작 시점에 수동으로 서비스 등록
public class GameInitializer : MonoBehaviour 
{
    private void Awake() 
    {
        var container = DIContainer.Instance;
        
        // 타입 등록
        container.Register<IDataService, DataService>(LifetimeScope.Singleton);
        
        // 인스턴스 직접 등록
        container.RegisterInstance<ISettingsProvider>(SettingsSO.Instance);
    }
}
```

### 3. 씬 제약 조건 설정

```csharp
// "MainMenu" 씬에서만 생성되는 매니저
[Register(LifetimeScope.Scene)]
[SceneConstraint("MainMenu")]
public class MainMenuManager : MonoBehaviour
{
    [Inject] private IUIService _uiService;
    
    private void Start()
    {
        Debug.Log("MainMenuManager initialized");
    }
}

// 빌드 인덱스 1, 2, 3번 씬에서만 생성
[Register(LifetimeScope.Scene)]
[SceneConstraint(1, 2, 3)]
public class LevelManager : MonoBehaviour
{
    [Inject] private ILevelService _levelService;
}
```

## 수명 주기(LifetimeScope)

DIContainer는 세 가지 주요 수명 주기를 지원합니다:

### 1. Singleton
- 애플리케이션 전체에서 단일 인스턴스 유지
- 씬 전환 시에도 유지됨(MonoBehaviour인 경우 자동으로 DontDestroyOnLoad 적용)
- 전역 서비스나 매니저에 적합

### 2. Scene
- 현재 씬에서만 존재하는 인스턴스
- 씬이 언로드될 때 자동으로 정리됨
- 씬에 종속된 매니저나 컨트롤러에 적합
- SceneConstraint 어트리뷰트와 함께 사용하여 특정 씬에서만 생성되도록 제한 가능

### 3. Transient
- 요청할 때마다 새 인스턴스 생성
- 상태를 유지하지 않는 서비스나 유틸리티에 적합
- SceneConstraint 어트리뷰트와 함께 사용 가능

## 어트리뷰트

### Register 어트리뷰트
클래스를 DI 컨테이너에 자동으로 등록합니다.

```csharp
// 기본 등록 - 자신의 타입과 구현하는 모든 인터페이스로 등록
[Register(LifetimeScope.Singleton)]
public class AudioService : IAudioService { }

// 특정 인터페이스로만 등록
[Register(typeof(IDataService), LifetimeScope.Scene)]
public class DataService : IDataService, IInitializable { }

// 여러 인터페이스로 등록
[Register(new Type[] { typeof(IDataService), typeof(IInitializable) }, LifetimeScope.Transient)]
public class DataService : IDataService, IInitializable { }
```

### Inject 어트리뷰트
필드, 프로퍼티, 생성자 매개변수에 의존성을 주입합니다.

```csharp
public class GameManager : MonoBehaviour
{
    // 필드 주입
    [Inject] private IDataService _dataService;
    
    // ID로 주입
    [Inject("GlobalConfig")] private IConfigService _configService;
    
    // 프로퍼티 주입
    [Inject]
    public IUIService UIService { get; private set; }
    
    // 생성자 주입 (MonoBehaviour가 아닌 클래스)
    public class GameController
    {
        private readonly ILevelService _levelService;
        
        [Inject]
        public GameController(ILevelService levelService)
        {
            _levelService = levelService;
        }
    }
}
```

### SceneConstraint 어트리뷰트
객체가 특정 씬에서만 생성되도록 제약합니다.

```csharp
// 씬 이름으로 제약
[Register(LifetimeScope.Scene)]
[SceneConstraint("MainMenu", "Options")]
public class MenuManager : MonoBehaviour { }

// 빌드 인덱스로 제약
[Register(LifetimeScope.Scene)]
[SceneConstraint(1, 2, 3)]
public class LevelManager : MonoBehaviour { }
```

## 생명 주기 이벤트 처리

DI 컨테이너는 씬 전환 시 자동으로 Scene 범위 객체를 정리합니다. IDisposable 인터페이스를 구현한 객체는 자동으로 Dispose() 메서드가 호출됩니다.

```csharp
[Register(LifetimeScope.Scene)]
public class ResourceManager : IDisposable
{
    private bool _disposed = false;
    
    public void Dispose()
    {
        if (!_disposed)
        {
            // 자원 정리 코드
            _disposed = true;
        }
    }
}
```

## 고급 기능

### 1. 어셈블리 자동 스캔

```csharp
// 모든 어셈블리에서 [Register] 어트리뷰트가 있는 타입 검색 및 등록
DIContainer.Instance.RegisterAssemblyTypes();

// 특정 어셈블리만 스캔
DIContainer.Instance.RegisterAssemblyTypes(typeof(MyClass).Assembly);
```

### 2. 수동 객체 주입

```csharp
// 기존 인스턴스에 수동으로 의존성 주입
var myObject = new MyClass();
DIContainer.Instance.InjectInto(myObject);
```

### 3. 컨테이너 초기화

```csharp
// 모든 등록 정보 초기화
DIContainer.Instance.ResetContainer();
```

## 모범 사례

1. **인터페이스 기반 설계**: 구현보다 인터페이스에 의존하도록 설계하여 유연성과 테스트 용이성 확보
2. **생명 주기 신중한 선택**: 각 서비스의 특성에 맞는 적절한 생명 주기(LifetimeScope) 선택
3. **MonoBehaviour 분리**: 비즈니스 로직은 일반 클래스로 분리하고, MonoBehaviour는 Unity 통합에만 집중
4. **싱글톤 패턴 대체**: 직접 싱글톤 구현 대신 DI 컨테이너의 Singleton 생명 주기 활용
5. **씬별 관리**: Scene 범위와 SceneConstraint를 활용하여 씬별 독립적인 컴포넌트 구조 설계

## 성능 고려사항

- 많은 의존성을 가진 복잡한 객체 그래프는 초기화 시간에 영향을 줄 수 있음
- 매우 자주 생성/삭제되는 객체는 Transient 대신 객체 풀링을 고려
- 대량의 등록이 필요한 경우 어셈블리 스캔보다 명시적 등록이 성능상 더 효율적
