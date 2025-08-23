# AssetManager

Unity의 Addressable Asset System을 기반으로, DI(Dependency Injection) 컨테이너를 통해 관리되는 에셋/씬 관리자입니다.

리소스의 종류와 사용 맥락에 따라 최적의 메모리 관리 방식을 적용하는 하이브리드 시스템을 채택하여, 개발 편의성과 안정성을 극대화합니다.

## 주요 특징

- **DI 컨테이너 기반 싱글톤**: `[Register]` 어트리뷰트를 통해 DI 컨테이너가 생명주기를 관리합니다.
- **하이브리드 메모리 관리**: 
  - **Owner 방식 (에셋)**: 에셋의 생명주기를 특정 게임오브젝트에 연결하여 관리합니다.
  - **Handle 방식 (프리팹/씬)**: 독립적인 핸들 오브젝트를 통해 리소스의 생명주기를 관리합니다.
- **Unity Addressable Asset System 기반**: 비동기 로딩을 지원하며 효율적인 메모리 사용을 보장합니다.
- **에셋/씬 통합 관리**: 에셋뿐만 아니라 씬의 로딩 및 자동 해제까지 일관되게 관리합니다.
- **참조 카운팅**: 공유되는 에셋을 중복 로드하지 않고, 참조가 모두 해제될 때만 메모리에서 언로드합니다.

## API 사용법

### 인스턴스 접근

`AssetManager`는 DI 컨테이너를 통해 주입받아 사용합니다.

```csharp
// 필드에 [Inject] 어트리뷰트를 사용하거나
[Inject] private AssetManager _assetManager;

// 생성자를 통해 주입받을 수 있습니다.
public class MyCoolClass
{
    private readonly AssetManager _assetManager;
    public MyCoolClass(AssetManager assetManager)
    {
        _assetManager = assetManager;
    }
}

// 또는 수동으로 Resolve 할 수 있습니다.
AssetManager assetManager = DIContainer.Resolve<AssetManager>();
```

### 1. 에셋 로딩 (Owner 방식)

`LoadAssetAsync`는 에셋의 생명주기를 `owner`로 지정된 게임오브젝트에 연결합니다. `owner`가 파괴되면 에셋의 참조 카운트가 자동으로 감소합니다.

```csharp
public class PlayerView : MonoBehaviour
{
    [Inject] private AssetManager _assetManager;

    public async void Start()
    {
        // PlayerView(this.gameObject)가 파괴될 때 "PlayerSprite"의 참조 카운트가 자동으로 1 감소합니다.
        var sprite = await _assetManager.LoadAssetAsync<Sprite>("PlayerSprite", this.gameObject);
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
}
```

### 2. 프리팹 인스턴스화 (Handle 방식)

`InstantiateAsync`는 프리팹을 인스턴스화하고, **생성된 인스턴스 자체가 핸들 역할**을 합니다. 인스턴스가 파괴되면 원본 프리팹 에셋이 자동으로 해제됩니다.

```csharp
public class EnemySpawner : MonoBehaviour
{
    [Inject] private AssetManager _assetManager;

    public async void SpawnEnemy()
    {
        // "EnemyPrefab"을 인스턴스화합니다.
        GameObject enemyInstance = await _assetManager.InstantiateAsync("EnemyPrefab", transform);

        // enemyInstance가 파괴되면 "EnemyPrefab" 에셋이 자동으로 해제됩니다.
    }
}
```

### 3. 씬 로딩 (Handle 방식)

`LoadSceneAsHandleAsync`는 씬을 로드하고, 씬의 생명주기를 제어하는 **별도의 '핸들' 게임오브젝트**를 반환합니다. 이 핸들 오브젝트를 파괴하면 씬이 자동으로 언로드됩니다.

```csharp
public class UIManager : MonoBehaviour
{
    [Inject] private AssetManager _assetManager;
    private GameObject _popupSceneHandle;

    public async void OpenPopup()
    {
        // 팝업 씬을 Additive 모드로 로드하고 핸들을 받아옵니다.
        _popupSceneHandle = await _assetManager.LoadSceneAsHandleAsync("UI_Popup_Scene", LoadSceneMode.Additive);
    }

    public void ClosePopup()
    {
        // 핸들 오브젝트를 파괴하여 팝업 씬을 언로드합니다.
        if (_popupSceneHandle != null)
        {
            Destroy(_popupSceneHandle);
        }
    }
}
```

## 관리 방식 상세

- **Owner 방식 (에셋)**: 텍스처, 오디오 클립 등 특정 컴포넌트의 '일부'로 사용되는 리소스에 적합합니다. 리소스와 소유자의 생명주기가 자연스럽게 동기화됩니다.
- **Handle 방식 (프리팹/씬)**: 독립적으로 존재하며, 로드한 주체와 생명주기가 다를 수 있는 리소스에 적합합니다. 핸들을 통해 언제든지 원하는 시점에 리소스를 해제할 수 있는 유연성을 제공합니다.

## 구성 요소

- **AssetManager**: 메인 에셋/씬 관리 클래스. DI 컨테이너에 의해 싱글톤으로 관리됩니다.
- **AutoReleaseComponent**: 게임오브젝트에 첨부되어, 해당 오브젝트가 파괴될 때 연결된 리소스(에셋 키, 씬/인스턴스 핸들)의 해제를 `AssetManager`에 요청하는 역할을 합니다.

## 의존성

- Unity Addressable Asset System
- BH_Lib.DI (DI Container)
- BH_Lib.Log (로깅)
- .NET Task 기반 비동기 처리
