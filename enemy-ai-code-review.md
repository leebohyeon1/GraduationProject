# Enemy AI 코드 리뷰 (엄격판)

작성일: 2026-04-28  
대상 프로젝트: `E:\unity\GraduationProject`

---

## 요약

이 enemy 시스템은 **기술 선택은 괜찮지만, 구현 일관성과 신뢰성은 부족하다**.

더 직설적으로 말하면:

- **설계는 중급 이상**이다.
- 하지만 **구현 검증 수준은 그 설계를 못 따라간다**.
- 일부 핵심 로직은 실제로 **버그 상태**다.
- 현재 코드는 “탄탄하게 설계된 적 시스템”이라기보다, **좋은 구조 위에 불안정한 규칙이 얹힌 상태**에 가깝다.

즉, 이 코드는 “조금만 다듬으면 완성” 수준이 아니라, **핵심 계약과 의미 체계를 다시 정리해야 하는 상태**다.

---

## 1. 현재 enemy 구현에 실제로 사용된 기술

### 1.1 커스텀 Behavior Tree
사용 기술:
- `ActionTree`
- `Node`
- `BlackBoard`
- `ServiceNode`
- `WeightedRandomSelector`
- 전용 BT 에디터

근거:
- `Assets/_GraduationProject/04_BT/01_BT_Node/Node/ActionTree.cs`
- `Assets/_GraduationProject/04_BT/01_BT_Node/Node/Node.cs`
- `Assets/_GraduationProject/04_BT/01_BT_Node/Node/BlackBoard.cs`
- `Assets/_GraduationProject/04_BT/01_BT_Node/Node/WeightedRandomSelector.cs`
- `Assets/_GraduationProject/04_BT/Editor/BehaviorTreeEditorWindow.cs`

특징:
- 런타임 시 `ActionTree.Clone()`으로 개체별 트리 상태 분리
- ScriptableObject 기반 행동 조립 구조

### 1.2 A* Pathfinding Project + RVO
사용 기술:
- `AIPath`
- `IAstarAI`
- `RVOController`

근거:
- `Assets/_GraduationProject/02_Scripts/02_Character/02_NPC/01_Enemy/05_Scripts/Enemy.cs:7`
- `.../EnemyMovement.cs:2-4, 30-48`
- `Packages/packages-lock.json:3-4`

### 1.3 AI 컨트롤 분리 구조
역할 분리:
- `EnemyStateController`: 상태 / 상태 락
- `AiController`: 프레임 업데이트 / LOD / BT 평가
- `AiBrain`: 블랙보드 / 쿨다운 / 감지 / 기억

근거:
- `EnemyStateController.cs`
- `AiController.cs`
- `AiBrain.cs`

### 1.4 ScriptableObject 기반 데이터/전략
사용 기술:
- `EnemyStat`
- `EnemyAttackData`
- `EnemyUseAnything`
- 전략 SO (`PlayerChaseStrategy`, `HomingMissileStrategy`, `TeleportAttackStrategy` 등)

근거:
- `EnemyStat.cs:3-11`
- `EnemyAttackData.cs:8-23`
- `EnemyUseAnything.cs:4-24`
- `Assets/_GraduationProject/04_BT/03_BT_SO/Script/*`

### 1.5 그룹 AI
사용 기술:
- 공격 토큰 분배
- 슬롯 배정
- 거리/시야 기반 AI 활성화 예산

근거:
- `GroupAi.cs:60-66`
- `GroupAi.cs:95-103`
- `GroupAi.cs:124-179`
- `GroupAi.cs:200-223`

### 1.6 Addressables + ObjectPool
사용 기술:
- `UnityEngine.AddressableAssets`
- `UnityEngine.Pool.ObjectPool<Enemy>`

근거:
- `MonsterPoolManager.cs:44-60`
- `MonsterPoolManager.cs:62-99`
- `Packages/manifest.json:5`

### 1.7 애니메이션 이벤트 + 피드백 기반 전투
사용 기술:
- `Enemy_AnimationEventHandler`
- `EnemyAnimationBridge`
- `ParrySystem`
- `Mon_Stiffness`

근거:
- `Enemy_AnimationEventHandler.cs`
- `EnemyAnimationBridge.cs`
- `ParrySystem.cs`
- `Mon_Stiffness.cs`

---

## 2. 총평: 무엇이 맞고, 무엇이 틀렸는가

### 맞게 잡은 부분
1. **커스텀 BT 방향 자체는 맞다.**
   - 개체별 런타임 상태 분리를 위해 `ActionTree.Clone()`을 쓴 점은 타당하다.
2. **Enemy를 허브로 두고 서브 시스템을 분리한 방향은 맞다.**
   - 이동, 상태, 체력, 패링, 애니메이션 브리지를 나누려는 시도는 정상적이다.
3. **그룹 토큰 시스템 발상은 좋다.**
   - 동시 공격 수를 제어하려는 접근은 다수전 AI에서 실제로 유효하다.
4. **적 풀링을 넣은 점은 좋다.**
   - 웨이브형 전투와 잘 맞는다.

### 틀린 부분
문제는 설계가 아니라 **구현 정밀도**다.

이 프로젝트는 다음과 같은 문제가 있다.

- 같은 블랙보드 키가 공격 타입마다 **다른 의미**를 가진다.
- 감지 로직이 **서로 다른 기준으로 중복**되어 있다.
- 초기화/재초기화/풀링 재사용이 **동일 계약으로 관리되지 않는다.**
- 상태 이름과 함수 이름이 실제 의미를 제대로 반영하지 않는다.
- 핫패스에 로그와 임시 디버깅 코드가 남아 있어 품질이 낮다.

결론적으로, **뼈대는 괜찮지만 신뢰할 수 있는 시스템이라고 보긴 어렵다.**

---

## 3. 치명적 문제 (High / Blocking)

### 3.1 지연 애니메이션 트리거가 실제로 실행되지 않음
근거:
- `EnemyAnimationBridge.cs:24-27`
  ```csharp
  public void TriggerEvent(string eventNamm,float delay)
  {
      delayAnimationTrigger(eventNamm, delay);
  }
  ```
- `MonsterWave.cs:124`
  ```csharp
  animationBridge.TriggerEvent(spawnAnimationTrigger, feedbackDelay);
  ```

문제:
- `delayAnimationTrigger()`는 `IEnumerator`지만 `StartCoroutine(...)`를 호출하지 않는다.
- 즉, 지연 실행이라는 이름만 있고 실제로는 코루틴이 시작되지 않는다.

판정:
- 이건 해석의 여지가 없는 **명백한 버그**다.

---

### 3.2 공격 성공 판정의 의미가 공격 타입마다 다름
근거:
- 근접 공격:
  - `BaseAttackNode.cs:345-356`
- 원거리 공격:
  - `Task_AttackRange.cs:53-59`
- 실제 투사체 충돌:
  - `EnemyProjectile.cs:37-43`

문제:
- 근접 공격은 **실제 명중 시** `DidLastAttackHit = true`
- 원거리 공격은 **투사체 발사만 해도** `DidLastAttackHit = true`

즉 `DidLastAttackHit`가 뜻하는 바가 통일되어 있지 않다.

이건 단순 취향 문제가 아니다.

- 근접에서는 “플레이어에게 맞았음”
- 원거리에서는 “탄이 나감”

이 둘은 전혀 다른 사건이다.

판정:
- 같은 키에 서로 다른 의미를 넣는 건 **AI 의사결정 오염**이다.
- 이 상태에서는 후속 판단(`재공격`, `압박`, `후딜 회피`, `콤보 분기`)이 일관될 수 없다.

---

### 3.3 히트 판정 루프가 스스로 인덱스를 증가시킴
근거:
- `BaseAttackNode.cs:341-352`
  ```csharp
  for (int i = 0; i < hitCount; i++)
  {
      ...
      if (col.TryGetComponent<PlayerHealth>(out PlayerHealth Character))
      {
          ...
          i++;
          ...
      }
  }
  ```

문제:
- `for` 루프 증가와 별개로 내부에서 `i++`를 또 하고 있다.
- 그 결과 충돌체 일부를 건너뛸 수 있다.

판정:
- 이건 의도가 불명확한 최적화가 아니라, **루프 오염**이다.
- 특히 플레이어가 복수 콜라이더를 가질 경우 결과가 더 예측 불가능해진다.

---

### 3.4 감지 로직이 논리적으로 무너져 있음
근거:
- `AiBrain.cs:68-81`
- 데이터 예시:
  - `Monster_a_EnemyStat.asset:17-19`
    - `SeeRange: 10`
    - `DetectRange: 5`
    - `CircleSeeRange: 4`

문제 코드:
```csharp
blackboard.SetValue(EnemyBlackboardKeys.DetectPlayer, dist <= _owner.enemyStat.DetectRange);

bool hasLos = false;
if(dist > _owner.enemyStat.CircleSeeRange) hasLos = true;
else if(dist <= _owner.enemyStat.SeeRange)
{
    if (Vector3.Angle(...) <= 90 * 0.5f)
    {
        hasLos = true;
    }
}
```

문제 분석:
- 현재 조건은 사실상 **거리 > 4면 LOS=true**다.
- 그럼 `SeeRange=10`은 거의 의미가 없어진다.
- 동시에 `DetectPlayer`는 5 이하에서만 true다.

즉 현재 상태는 이런 모순을 만든다:
- 플레이어를 “감지”하지는 않았는데
- “시야는 확보했다”고 판단함

판정:
- 이건 설계가 아니라 **조건문 파편**이다.
- 센서 모델이 아니다.

---

### 3.5 시야 판정이 두 군데 중복되고 기준도 다름
근거:
- `AiBrain.cs:53-82`
- `Condition_CanSeePlayer.cs:14-25`

문제:
- `AiBrain`은 거리/각도 기반
- `Condition_CanSeePlayer`는 각도만 보고 true
- 장애물 차폐(Raycast LOS) 없음

`Condition_CanSeePlayer.cs:23-24` 주석도 스스로 이를 인정한다.

판정:
- 적의 “본다 / 못 본다”가 하나의 시스템이 아니다.
- **서로 다른 두 기준이 동시에 존재**한다.
- 이건 유연성이 아니라 **불일치**다.

---

## 4. 구조적 문제 (Medium-High)

### 4.1 풀링 재초기화가 참조 수집 단계를 생략함
근거:
- `MonsterPoolManager.cs:57`
  ```csharp
  enemy.Init();
  ```
- `EnemyInitializer.cs:42-48`
  ```csharp
  public void Reinitialize()
  {
      Phase2_InitializeData();
      Phase3_InitializeComponents(skipCache: true); 
      Phase4_InitializeAI();
      Phase5_RegisterGroup();
      Phase6_FinalizeState();
  }
  ```

문제:
- `Reinitialize()`는 `Phase1_CollectReferences()`를 다시 수행하지 않는다.
- 즉 `_player`, `_groupAi` 같은 참조는 처음 잡은 것을 계속 재사용한다.

판정:
- 풀링을 넣었으면 참조 재수집 계약을 맞춰야 한다.
- 지금 구조는 **재사용 수명주기 설계가 미완성**이다.

---

### 4.2 초기화 실패를 잡아먹고 로그만 남김
근거:
- `EnemyInitializer.cs:24-39`

문제:
```csharp
try { ... }
catch (Exception ex)
{
    Debug.LogError(...);
}
```

초기화 실패는 실패다.
그런데 이 코드는 실패를 시스템 차원에서 중단하지 않고, 로그 출력으로만 끝낸다.

판정:
- partially initialized enemy를 남길 수 있다.
- 복구가 아니라 **오염 상태 방치**다.

---

### 4.3 저장한 원래 값을 복구하지 않음
근거:
- `PlayerChaseStrategy.cs:29-32`
  - `_originalAcceleration` 저장
- `PlayerChaseStrategy.cs:101-103`
  ```csharp
  aiPath.maxAcceleration = float.PositiveInfinity;
  aiPath.rotationSpeed = _originalRotationSpeed;
  ```

문제:
- 저장한 값은 `_originalAcceleration`
- 복구는 `PositiveInfinity`

판정:
- 이건 실수가 아니라 **자기 계약 파괴**다.
- “원래 값 저장 → 원래 값 복구”라는 가장 기본적인 규칙도 안 지켰다.

---

### 4.4 `IsActionable()`라는 이름이 실제 의미와 맞지 않음
근거:
- `AiBrain.cs:137-138`
  ```csharp
  public bool IsActionable() {
      switch (CurrentState) {
          case EnemyStateController.EnemyState.Attack:
          case EnemyStateController.EnemyState.Rush:
          case EnemyStateController.EnemyState.Die:
              return true;
          default: return false;
      }
  }
  ```

문제:
- “Actionable”은 보통 행동 가능 상태를 뜻한다.
- 그런데 `Die`도 true다.

판정:
- 이건 잘못된 네이밍이다.
- 더 나쁜 점은, 이런 이름은 다른 코드에서 오해를 유발한다.
- 실제로 `EnemyHealth.TakeDamage()` 분기에도 영향 준다.

---

## 5. 성능/운영 측면 문제

### 5.1 몬스터는 풀링하면서 투사체는 반복 생성/파괴
근거:
- `Task_AttackRange.cs:53`
- `HomingMissileStrategy.cs:49`
- `EnemyProjectile.cs:25, 43, 49`

문제:
- 적 본체는 `ObjectPool<Enemy>`로 돌리면서
- 투사체는 `Instantiate/Destroy`를 반복한다.

판정:
- 이건 일관된 성능 전략이 아니다.
- **부분 최적화**다.

---

### 5.2 핫패스에 디버그 로그가 남아 있음
근거 예시:
- `Task_AttackRange.cs:47, 62`
- `EnemyHealth.cs:199`
- `ParabolicMineProjectile.cs:230`
- `EnemyProjectile.cs:36, 41`
- `ParrySystem.cs:87, 113, 148`

판정:
- 디버깅 코드가 관리되지 않고 남아 있다.
- 전투 루프에 로그가 섞여 있으면 성능/가독성/품질 모두 떨어진다.

---

### 5.3 `AiBrain.Tick()`은 빈 함수인데 매 프레임 호출됨
근거:
- `AiController.cs:105-106`
- `AiBrain.cs:41`

문제:
- 업데이트 계약은 있는데 실제 내용은 비어 있다.

판정:
- 죽은 추상화다.
- 구조만 복잡하게 만들고 기능은 없다.

---

## 6. 유지보수성 문제

### 6.1 블랙보드 키 체계가 통일되지 않음
근거:
- enum 기반 키 사용
- raw string 혼용
  - `GroupAi.cs:29-32`
  - `GroupAi.cs:100-101`
  - `BaseAttackNode.cs:46`
  - `Service_UpdateCombatVars.cs:6-7`

문제:
- 키 오타에 취약함
- 리팩터링 안정성 낮음
- 검색 추적이 어려움

판정:
- 이건 유연함이 아니라 **약한 계약**이다.

---

### 6.2 런타임 코드에 Editor 네임스페이스 import 존재
근거:
- `ParrySystem.cs:1`
  ```csharp
  using Packages.Rider.Editor.UnitTesting;
  ```

문제:
- 런타임 코드에 editor 계열 import가 남아 있다.
- 실제 사용 흔적도 없다.

판정:
- 코드 청결도 부족.
- 정리 안 된 흔적이다.

---

### 6.3 실험 흔적/죽은 코드가 남아 있음
예:
- `RadiusTraversalProvider.cs`는 존재하지만 실제 사용 흔적을 찾기 어려움
- `com.unity.behavior` 패키지는 설치돼 있지만 enemy AI는 커스텀 BT 사용

판정:
- 기능 실험 흔적을 정리하지 않으면 프로젝트는 점점 설명 불가능해진다.

---

## 7. 기술적으로 무엇을 쓰면 어떤 부분이 발전하는가

### 7.1 센서 시스템 통합
도입 제안:
- 단일 `PerceptionService`
- 거리 / 시야각 / Raycast LOS / last seen memory 통합

개선 효과:
- 감지 규칙의 충돌 제거
- 적 발견 / 추적 / 엄폐 반응이 일관됨
- 스텔스, 유인, 시야 끊기 같은 플레이 설계 가능

이유:
- 현재는 `AiBrain`과 `Condition_CanSeePlayer`가 서로 다른 기준으로 플레이어를 본다.

---

### 7.2 공격 결과 규약 분리
도입 제안:
- `AttackStarted`
- `ProjectileSpawned`
- `AttackConnected`
- `AttackBlocked`
- `AttackMissed`

개선 효과:
- 근접/원거리/범위 공격을 동일한 의미 체계로 처리 가능
- 후속 AI 분기 정확도 상승

이유:
- 현재 `DidLastAttackHit`는 공격 타입에 따라 뜻이 다르다.

---

### 7.3 투사체 풀링
도입 제안:
- `ObjectPool<EnemyProjectile>`
- `ObjectPool<HomingProjectile>`
- `ObjectPool<ParabolicMineProjectile>`

개선 효과:
- GC 감소
- 웨이브전 안정성 향상
- 보스 패턴 다중 사용 시 프레임 안정화

이유:
- 지금은 몬스터만 풀링하고 투사체는 매번 생성/파괴한다.

---

### 7.4 Utility AI를 BT 상위에 혼합
도입 제안:
- 기존 BT 폐기 금지
- 상위 의사결정만 Utility 점수화

개선 효과:
- 체력, 거리, 플레이어 상태, 쿨다운 기반으로 더 자연스러운 행동 선택
- 단순 가중 랜덤보다 설득력 있는 패턴

이유:
- 현재 `WeightedRandomSelector`는 상황 적응보다 랜덤 선택에 가깝다.

---

### 7.5 테스트 자동화
도입 제안:
- PlayMode 테스트
- 감지 테스트
- 패링/경직 테스트
- 공격 명중 테스트
- 풀링 재초기화 테스트

개선 효과:
- 회귀 버그 감소
- 리팩터링 가능성 증가

이유:
- 현재 구조는 복잡하지만 적 AI 전용 자동 테스트 기반이 거의 보이지 않는다.

---

## 8. 우선순위 제안

### 즉시 수정 (Blocking)
1. `EnemyAnimationBridge.TriggerEvent(string,float)` 실제 코루틴 실행되도록 수정
2. `DidLastAttackHit` 의미 재정의
3. `BaseAttackNode` 히트 루프의 내부 `i++` 제거
4. 감지 로직 (`AiBrain`) 재검토 및 조건 일원화

### 단기 개선
1. `Condition_CanSeePlayer`를 센서 서비스에 통합하거나 제거
2. `Reinitialize()`에서 참조 재수집 포함
3. `PlayerChaseStrategy`의 원래 값 복구 로직 수정
4. 블랙보드 키 enum/상수 체계 통일

### 중기 개선
1. 투사체 풀링
2. Utility AI 상위 계층 도입
3. 로그 레벨 게이팅
4. PlayMode 테스트 도입

---

## 9. 최종 판정

### 판정
**REQUEST CHANGES**

### 이유
다음 사유로 현재 구현은 신뢰 가능한 상태라고 보기 어렵다.

1. 지연 애니메이션 트리거가 실제 동작하지 않음
2. 공격 성공 판정 의미가 공격 타입별로 다름
3. 감지 로직이 논리적으로 충돌함
4. 재초기화 시 참조 수집 계약이 깨짐
5. 상태/이동/복구 로직에 자기모순이 존재함

### 최종 결론
이 enemy 시스템은:
- **기술 선택은 맞았다.**
- **구조의 방향도 나쁘지 않다.**
- 하지만 **구현 일관성, 정밀도, 의미 체계가 부족하다.**

따라서 지금 단계에서 필요한 것은 새 기술을 더 붙이는 일이 아니라,

1. **센서/상태/공격 결과의 의미를 통일하고**
2. **재초기화/풀링/투사체 수명주기 계약을 다시 정리하고**
3. **테스트로 검증 가능한 구조로 바꾸는 것**이다.

그 전까지는 이 코드를 “확장 가능한 적 시스템”이라고 부를 수는 있어도, **신뢰 가능한 적 시스템**이라고 부르기는 어렵다.
