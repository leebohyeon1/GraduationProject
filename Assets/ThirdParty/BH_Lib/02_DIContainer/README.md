# BH_Lib DIContainer

Unity 프로젝트를 위한 가볍고 강력한 의존성 주입(DI) 시스템입니다. SOLID 원칙을 따르는 고품질 코드 작성과 깔끔한 아키텍처 설계를 지원합니다.

## 주요 기능

- **타입 기반 의존성 주입**: 인터페이스-구현 관계의 분리와 테스트 용이성 제공
- **다양한 수명 주기(LifetimeScope) 관리**: Singleton, Scene, Transient 범위 지원
- **씬별 제약 조건**: 특정 씬에서만 객체가 생성되도록 제어
- **어트리뷰트 기반 자동 등록**: `[Register]` 어트리뷰트로 간편하게 DI 컨테이너에 클래스 등록
- **MonoBehaviour 완벽 통합**: `DIMonoBehaviour` 상속 또는 `[Inject]` 어트리뷰트로 Unity 컴포넌트에 손쉽게 의존성 주입
- **자동 메모리 관리**: 씬 전환 시 Scene Scope 객체 자동 정리 및 `IDisposable` 인터페이스 지원

## 시작하기

### 1. 초기 설정

프로젝트의 시작 씬(보통 가장 먼저 로드되는 씬)에 빈 게임 오브젝트를 만들고 `DIContainerInitializer` 컴포넌트를 추가합니다. 이 컴포넌트가 모든 것을 자동으로 설정합니다.

### 2. 서비스 정의 및 등록

서비스로 사용할 클래스에 `[Register]` 어트리뷰트를 붙여 DI 컨테이너에 자동으로 등록합니다.

```csharp
// 서비스 인터페이스
public interface IDataService 
{
    string GetData();
}

// 서비스 구현체에 [Register] 어트리뷰트 추가
[Register(typeof(IDataService), LifetimeScope.Singleton)]
public class DataService : IDataService 
{
    public string GetData() => "Hello, DI!";
}
```

### 3. 의존성 주입받기

`[Inject]` 어트리뷰트를 사용하여 필드나 프로퍼티에 의존성을 주입받습니다.

```csharp
public class GameManager : MonoBehaviour 
{
    [Inject] private IDataService _dataService;
    
    private void Start() 
    {
        // _dataService는 자동으로 주입되어 사용 가능합니다.
        Debug.Log(_dataService.GetData()); // "Hello, DI!"
    }
}
```

## 수명 주기 (LifetimeScope)

DIContainer는 세 가지 주요 수명 주기를 지원합니다.

- **Singleton**: 애플리케이션 전체에서 단일 인스턴스를 유지합니다. 씬 전환 시에도 파괴되지 않으며, 전역 매니저나 서비스에 적합합니다.
- **Scene**: 씬 내에서 단일 인스턴스를 유지합니다. 씬이 언로드될 때 자동으로 파괴되며, 해당 씬에 종속적인 관리가 필요할 때 사용합니다.
- **Transient**: 요청할 때마다 새로운 인스턴스를 생성합니다. 상태를 유지하지 않는 일회성 객체나 유틸리티에 적합합니다.

## 주요 어트리뷰트

### `[Register]`
클래스를 DI 컨테이너에 자동으로 등록합니다.

```csharp
// 기본 등록 (Singleton, 자신의 타입과 인터페이스로 등록)
[Register]
public class AudioService : IAudioService { ... }

// 특정 인터페이스와 Scene Scope로 등록
[Register(typeof(IDataService), LifetimeScope.Scene)]
public class DataService : IDataService { ... }
```

### `[Inject]`
필드, 프로퍼티, 생성자 매개변수에 의존성을 주입합니다.

```csharp
// 필드 주입
[Inject] private IDataService _dataService;

// 프로퍼티 주입
[Inject] public IUIService UIService { get; private set; }

// ID 기반 주입 (특정 인스턴스를 구분할 때)
[Inject("GlobalConfig")] private IConfigService _configService;
```

### `[SceneConstraint]`
`Scene` 또는 `Transient` 생명주기를 가진 객체가 특정 씬에서만 생성되도록 제약합니다.

```csharp
// 씬 이름으로 제약
[Register(LifetimeScope.Scene)]
[SceneConstraint("MainMenu", "Lobby")]
public class MenuManager : MonoBehaviour { ... }

// 빌드 인덱스로 제약
[Register(LifetimeScope.Scene)]
[SceneConstraint(1, 2)]
public class LevelManager : MonoBehaviour { ... }
```

## 고급 기능

### `DIMonoBehaviour`
이 클래스를 상속받는 MonoBehaviour는 `Awake()` 시점에 자동으로 의존성이 주입됩니다. `base.Awake()`를 호출할 필요가 없어 편리합니다.

```csharp
public class MyComponent : DIMonoBehaviour
{
    [Inject] private IMyService _myService;

    private void Start()
    {
        _myService.DoSomething();
    }
}
```

### `IDisposable` 지원
`Scene` 또는 `Singleton` 생명주기를 가진 객체가 `IDisposable` 인터페이스를 구현하면, 컨테이너가 관리하는 생명주기가 끝날 때 (씬 언로드, 앱 종료 등) 자동으로 `Dispose()` 메서드를 호출하여 안전한 자원 해제를 보장합니다.

## 의존성

- 없음 (독립 모듈)