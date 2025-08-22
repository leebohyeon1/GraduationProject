# BH_Lib FSM (Finite State Machine)

Unity용 유한 상태 머신 라이브러리입니다. 게임 캐릭터, AI, UI 등의 상태 관리에 활용할 수 있습니다.

## 📁 구조

```
04_FSM/
├── IState.cs              # 상태 인터페이스
├── BaseState.cs           # 기본 상태 추상 클래스
├── StateMachine.cs        # 상태 머신 핵심 클래스
├── StateTransition.cs     # 상태 전환 규칙 클래스
└── README.md             # 이 파일
```

## 🎯 주요 기능

- **상태 관리**: 상태 추가, 전환, 제거
- **자동 전환**: 조건부 상태 자동 전환 시스템
- **상태 히스토리**: 이전 상태로 복귀 기능 (최대 10개)
- **타입 안전성**: 제네릭을 활용한 컴파일 타임 타입 검사
- **Unity 통합**: Update/FixedUpdate 라이프사이클 지원
- **이벤트 시스템**: 상태 변경 알림

## 🚀 사용 방법

### 1. 기본 설정

```csharp
using BH_Lib.FSM;

// 컨텍스트 클래스 (상태가 작동할 대상)
public class Player : MonoBehaviour
{
    private StateMachine<Player> stateMachine;
    
    void Start()
    {
        // 상태 머신 생성
        stateMachine = new StateMachine<Player>(this);
        
        // 상태들 추가
        stateMachine.AddState(new IdleState(this, stateMachine));
        stateMachine.AddState(new MoveState(this, stateMachine));
        stateMachine.AddState(new AttackState(this, stateMachine));
        
        // 초기 상태 설정
        stateMachine.ChangeState<IdleState>();
    }
    
    void Update()
    {
        stateMachine.Update(); // 매 프레임 업데이트
    }
    
    void FixedUpdate()
    {
        stateMachine.FixedUpdate(); // 물리 업데이트
    }
}
```

### 2. 상태 구현

#### 방법 1: BaseState 상속 (권장)
```csharp
public class IdleState : BaseState<Player>
{
    public IdleState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }
    
    public override void OnEnter()
    {
        Debug.Log("Idle 상태 진입");
        // 애니메이션 설정 등
    }
    
    public override void OnUpdate()
    {
        // 입력 처리
        if (Input.GetAxis("Horizontal") != 0)
        {
            stateMachine.ChangeState<MoveState>();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            stateMachine.ChangeState<AttackState>();
        }
    }
    
    public override void OnExit()
    {
        Debug.Log("Idle 상태 종료");
    }
}
```

#### 방법 2: IState 직접 구현
```csharp
public class MoveState : IState
{
    private Player player;
    private StateMachine<Player> stateMachine;
    
    public MoveState(Player player, StateMachine<Player> stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }
    
    public void OnEnter()
    {
        Debug.Log("Move 상태 진입");
    }
    
    public void OnUpdate()
    {
        float input = Input.GetAxis("Horizontal");
        if (input == 0)
        {
            stateMachine.ChangeState<IdleState>();
            return;
        }
        
        // 이동 처리
        player.transform.Translate(Vector3.right * input * Time.deltaTime * 5f);
    }
    
    public void OnFixedUpdate() { }
    public void OnExit() { }
}
```

### 3. 자동 상태 전환

```csharp
void Start()
{
    stateMachine = new StateMachine<Player>(this);
    
    // 상태 추가
    stateMachine.AddState(new IdleState(this, stateMachine));
    stateMachine.AddState(new MoveState(this, stateMachine));
    stateMachine.AddState(new DeadState(this, stateMachine));
    
    // 조건부 자동 전환 설정
    // Idle에서 Move로: 입력이 있을 때
    stateMachine.AddTransition<IdleState, MoveState>(() => Input.GetAxis("Horizontal") != 0);
    
    // Move에서 Idle로: 입력이 없을 때
    stateMachine.AddTransition<MoveState, IdleState>(() => Input.GetAxis("Horizontal") == 0);
    
    // 모든 상태에서 Dead로: HP가 0 이하일 때
    stateMachine.AddAnyTransition<DeadState>(() => hp <= 0);
    
    stateMachine.ChangeState<IdleState>();
}
```

### 4. 이벤트 처리

```csharp
void Start()
{
    stateMachine = new StateMachine<Player>(this);
    
    // 상태 변경 이벤트 구독
    stateMachine.OnStateChanged += OnStateChanged;
}

private void OnStateChanged(Type from, Type to)
{
    Debug.Log($"상태 변경: {from.Name} → {to.Name}");
    
    // UI 업데이트, 사운드 재생 등
    if (to == typeof(AttackState))
    {
        PlayAttackSound();
    }
}
```

### 5. 상태 히스토리 활용

```csharp
public class PauseState : BaseState<GameManager>
{
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 이전 상태로 복귀
            stateMachine.RevertToPreviousState();
        }
    }
}
```

## 📝 활용 예시

### 캐릭터 AI
```csharp
// AI 상태들
public class PatrolState : BaseState<Enemy> { ... }
public class ChaseState : BaseState<Enemy> { ... }
public class AttackState : BaseState<Enemy> { ... }

// 자동 전환 설정
stateMachine.AddTransition<PatrolState, ChaseState>(() => 
    Vector3.Distance(context.transform.position, player.position) < detectionRange);
    
stateMachine.AddTransition<ChaseState, AttackState>(() => 
    Vector3.Distance(context.transform.position, player.position) < attackRange);
```

### UI 상태 관리
```csharp
public class MainMenuState : BaseState<UIManager> { ... }
public class InGameState : BaseState<UIManager> { ... }
public class PauseState : BaseState<UIManager> { ... }
public class GameOverState : BaseState<UIManager> { ... }
```

### 게임 시스템
```csharp
public class LoadingState : BaseState<GameManager> { ... }
public class PlayingState : BaseState<GameManager> { ... }
public class PausedState : BaseState<GameManager> { ... }
public class EndState : BaseState<GameManager> { ... }
```

## 🎛️ 고급 기능

### 상태 확인
```csharp
// 현재 상태가 특정 타입인지 확인
if (stateMachine.IsInState<AttackState>())
{
    Debug.Log("현재 공격 중");
}

// 현재 상태 타입 가져오기
Type currentStateType = stateMachine.CurrentStateType;
```

### 컨텍스트 접근
```csharp
// 상태에서 컨텍스트 접근
Player player = stateMachine.GetContext();
```

## ⚠️ 주의사항

1. **상태 등록**: `ChangeState()` 호출 전에 반드시 `AddState()`로 상태를 등록해야 합니다.
2. **Update 호출**: MonoBehaviour에서 `stateMachine.Update()`와 `stateMachine.FixedUpdate()`를 호출해야 합니다.
3. **메모리 관리**: 상태 객체들은 한 번만 생성하여 재사용됩니다.
4. **전환 조건**: 자동 전환 조건은 매 프레임 확인되므로 성능을 고려해 작성하세요.

## 🔧 의존성

- Unity Engine
- BH_Lib.Log (로그 출력용)

## 📄 라이선스

BH_Lib 라이브러리의 일부입니다.