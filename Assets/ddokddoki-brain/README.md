# ddokddoki-brain

ddokddoki 엔티티를 위한 AI 브레인 모듈입니다.

## 개요

이 모듈은 `ddokddoki` 캐릭터의 AI 의사결정 시스템(`DdokddokiBrain`)과  
Unity `MonoBehaviour` 컨트롤러(`DdokddokiController`)를 제공합니다.

기존 프로젝트의 `AiBrain` / `AiController` 패턴을 기반으로 설계되었습니다.

## 파일 구성

| 파일 | 설명 |
|------|------|
| `Scripts/DdokddokiBlackboardKeys.cs` | AI 상태 공유에 사용되는 BlackBoard 키 열거형 |
| `Scripts/DdokddokiBrain.cs` | 시야 감지, 스킬 쿨타임, 전투 상태를 관리하는 브레인 클래스 |
| `Scripts/DdokddokiController.cs` | `DdokddokiBrain`을 보유하고 매 프레임 Tick을 호출하는 MonoBehaviour 컴포넌트 |

## 사용 방법

1. `DdokddokiController` 컴포넌트를 ddokddoki 엔티티 GameObject에 추가합니다.
2. Inspector에서 **Player Transform** 필드에 플레이어의 Transform을 할당합니다.
3. `DdokddokiController.EnterCombat()` / `SetStunned()` 를 통해 상태를 제어합니다.
4. `DdokddokiController.Brain.blackboard` 를 통해 AI 상태를 읽거나 씁니다.

## BlackBoard 주요 키

| 키 | 타입 | 설명 |
|----|------|------|
| `DistanceBetween` | `float` | 플레이어와의 거리 |
| `LastPlayerPos` | `Vector3` | 마지막으로 확인된 플레이어 위치 |
| `IsPlayerDetected` | `bool` | 전투(감지) 상태 여부 |
| `OnPlayerLooking` | `bool` | 플레이어가 자신을 바라보는지 여부 |
| `HomePosition` | `Vector3` | 시작 위치 |

## 확장 방법

- **새 BlackBoard 키 추가** : `DdokddokiBlackboardKeys.cs`에 열거형 값을 추가합니다.
- **커스텀 브레인 로직 추가** : `DdokddokiBrain.Tick()` 또는 `TickCoroutine()`을 확장합니다.
- **Behavior Tree 연동** : 기존 `BehaviorTree.Node`를 상속하여 `DdokddokiBrain`을 참조하는 노드를 작성합니다.
