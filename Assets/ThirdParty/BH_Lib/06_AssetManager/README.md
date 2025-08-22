# AssetManager

Unity의 Addressable Asset System을 사용하는 에셋 관리 싱글톤 클래스입니다.

## 기능

### 주요 특징
- 싱글톤 패턴으로 구현된 전역 에셋 관리자
- Unity Addressable Asset System 기반
- 비동기 에셋 로딩 및 인스턴스화
- **참조 카운팅 기반 자동 메모리 관리**
- **컴포넌트 기반 자동 해제 시스템**
- 중복 로딩 방지 및 에셋 재사용
- 에러 핸들링 및 로깅

### 지원 기능
- 일반 에셋 비동기 로딩
- 프리팹 비동기 인스턴스화
- 참조 카운팅 기반 스마트 해제
- 게임오브젝트 파괴 시 자동 해제
- 에셋 중복 로딩 방지
- 인스턴스 파괴 및 해제

## 사용법

### 기본 사용

```csharp
// 인스턴스 접근
AssetManager assetManager = AssetManager.Instance;
```

### 에셋 로딩

```csharp
// 일반 에셋 로딩 (수동 해제)
Sprite sprite = await AssetManager.Instance.LoadAssetAsync<Sprite>("sprite_key");
AudioClip clip = await AssetManager.Instance.LoadAssetAsync<AudioClip>("audio_key");

// 게임오브젝트와 연결된 에셋 로딩 (자동 해제)
Sprite playerSprite = await AssetManager.Instance.LoadAssetAsync<Sprite>("player_sprite", gameObject);
// 이 gameObject가 파괴될 때 자동으로 "player_sprite" 해제
```

### 프리팹 인스턴스화

```csharp
// 프리팹 인스턴스화 (자동 해제)
GameObject enemy = await AssetManager.Instance.InstantiateAsync("enemy_prefab");
// 생성된 enemy 오브젝트가 파괴될 때 자동으로 "enemy_prefab" 해제

// 부모 Transform 지정
GameObject childInstance = await AssetManager.Instance.InstantiateAsync("prefab_key", parentTransform);
```

### 에셋 해제

```csharp
// 수동 에셋 해제
AssetManager.Instance.ReleaseAsset("asset_key");

// 인스턴스 해제
AssetManager.Instance.ReleaseInstance(gameObjectInstance);

// 특정 컴포넌트의 에셋 해제
var autoRelease = gameObject.GetComponent<AutoReleaseComponent>();
autoRelease.ReleaseAsset("specific_asset");

// 게임오브젝트 파괴 시 자동 해제 (권장)
Destroy(gameObject); // 연결된 모든 에셋 자동 해제
```

## 내부 동작

### 참조 카운팅 시스템
- Dictionary를 사용하여 각 에셋의 참조 횟수 추적
- 참조 카운트가 0이 될 때만 실제 메모리에서 해제
- 중복 로딩 방지 및 메모리 효율성 보장

### 컴포넌트 기반 자동 해제
- `AutoReleaseComponent`가 자동으로 게임오브젝트에 추가
- 게임오브젝트 파괴 시 `OnDestroy`에서 자동 해제
- 컴포넌트별 관리 에셋 추적 및 일괄 해제

### 에셋 핸들 관리
- Dictionary를 사용하여 로드된 에셋의 핸들을 관리
- 중복 로딩 방지를 위한 캐싱 메커니즘
- 애플리케이션 종료 시 자동 정리

### 에러 처리
- 에셋 로딩 실패 시 null 반환 및 에러 로깅
- 잘못된 키나 존재하지 않는 에셋에 대한 경고
- 메모리 누수 방지를 위한 실패 시 핸들 제거

### 메모리 관리
- 참조 카운팅 + 컴포넌트 기반 이중 안전망
- OnDestroy에서 모든 핸들 자동 해제
- 개별 에셋별 수동 해제 지원
- Addressable과 일반 인스턴스 구분 처리

## 사용 시나리오

### 자동 해제 시나리오
```csharp
// 1. 플레이어 스프라이트 로딩
var playerSprite = await AssetManager.Instance.LoadAssetAsync<Sprite>("PlayerSprite", gameObject);
// 참조 카운트: 1

// 2. UI에서 같은 스프라이트 사용
var uiSprite = await AssetManager.Instance.LoadAssetAsync<Sprite>("PlayerSprite", uiGameObject);
// 참조 카운트: 2 (실제 로드는 하지 않고 기존 것 재사용)

// 3. 플레이어 오브젝트 파괴
Destroy(gameObject);
// 참조 카운트: 1 (아직 해제되지 않음)

// 4. UI 오브젝트 파괴
Destroy(uiGameObject);
// 참조 카운트: 0 (실제 메모리에서 해제)
```

## 주의사항

1. **싱글톤 패턴**: 전역에서 하나의 인스턴스만 존재
2. **비동기 처리**: 모든 로딩 작업은 async/await 사용
3. **자동 해제**: 게임오브젝트와 연결하면 자동 해제 (권장)
4. **Addressable 설정**: 에셋이 Addressable로 등록되어 있어야 함
5. **참조 카운팅**: 같은 에셋을 여러 곳에서 사용해도 한 번만 로드

## 구성 요소

### AssetManager
- 메인 에셋 관리 싱글톤 클래스
- 참조 카운팅 및 에셋 로딩/해제 담당

### AutoReleaseComponent
- 게임오브젝트에 자동 추가되는 컴포넌트
- 오브젝트 파괴 시 연결된 에셋 자동 해제
- 개별 에셋 관리 및 해제 기능

## 의존성

- Unity Addressable Asset System
- .NET Task 기반 비동기 처리
- MonoBehaviour 기반 싱글톤
- Component 기반 자동 해제 시스템