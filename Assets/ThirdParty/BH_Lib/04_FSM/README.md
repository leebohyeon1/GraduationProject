# FSM (Finite State Machine)

Unity 프로젝트를 위한 유연하고 강력한 유한 상태 머신(FSM) 라이브러리입니다. 제네릭 기반으로 설계되어 타입 안정성을 보장하며, 게임 캐릭터 AI, UI 상태, 게임 시스템 로직 등 다양한 곳에 적용할 수 있습니다.

## 주요 기능

- **타입-안전(Type-Safe) 상태 관리**: 제네릭을 사용하여 컴파일 타임에 상태 타입을 검증합니다.
- **자동 상태 전환**: 조건(Predicate)을 기반으로 한 유연한 자동 상태 전환 시스템을 제공합니다.
- **상태 히스토리**: `RevertToPreviousState()` 메서드로 이전 상태로 쉽게 돌아갈 수 있습니다.
- **Unity 라이프사이클 통합**: `Update` 및 `FixedUpdate`를 지원하여 MonoBehaviour와 자연스럽게 연동됩니다.
- **이벤트 기반 알림**: `OnStateChanged` 이벤트를 통해 상태 변경 시점에 원하는 로직을 실행할 수 있습니다.

## 구조

```
04_FSM/
├── IState.cs           // 모든 상태가 구현해야 하는 기본 인터페이스
├── BaseState.cs        // 상태 구현을 돕는 편리한 추상 클래스
├── StateMachine.cs     // 상태를 관리하고 전환을 처리하는 핵심 클래스
├── StateTransition.cs  // 상태 전환의 규칙과 조건을 정의하는 클래스
└── README.md          // 현재 문서
```

## 사용 방법

### 1. 상태 머신 설정

상태를 관리할 주체(Context)가 되는 클래스(예: `Player`, `UIManager`)에서 `StateMachine`을 생성하고 초기화합니다.

```csharp
using BH_Lib.FSM;

public class Player : MonoBehaviour
{
    public StateMachine<Player> StateMachine { get; private set; }
    
    void Awake()
    {
        // 1. 상태 머신 생성 (컨텍스트는 Player 자신)
        StateMachine = new StateMachine<Player>(this);
        
        // 2. 상태들 추가
        StateMachine.AddState(new IdleState(this, StateMachine));
        StateMachine.AddState(new MoveState(this, StateMachine));
        StateMachine.AddState(new AttackState(this, StateMachine));
        
        // 3. 초기 상태 설정
        StateMachine.ChangeState<IdleState>();
    }
    
    void Update()
    {
        // 4. 매 프레임 상태 머신 업데이트
        StateMachine.Update();
    }
}
```

### 2. 상태 클래스 구현

`BaseState<T>`를 상속받아 각 상태의 구체적인 로직을 구현합니다.

```csharp
public class IdleState : BaseState<Player>
{
    // 생성자에서 context와 stateMachine을 부모 클래스로 전달
    public IdleState(Player context, StateMachine<Player> stateMachine) : base(context, stateMachine) { }
    
    public override void OnEnter()
    {
        // p_context를 통해 Player 클래스의 멤버에 접근 가능
        Debug.Log($"{p_context.name} Idle 상태 진입");
    }
    
    public override void OnUpdate()
    {
        // 특정 조건 만족 시 p_stateMachine을 통해 상태 전환
        if (Input.GetAxis("Horizontal") != 0)
        {
            p_stateMachine.ChangeState<MoveState>();
        }
    }
}
```

### 3. 자동 상태 전환 설정

`Update` 내에서 직접 상태를 전환하는 대신, 조건부 자동 전환을 설정하여 코드를 더 깔끔하게 관리할 수 있습니다.

```csharp
void Awake()
{
    StateMachine = new StateMachine<Player>(this);
    
    StateMachine.AddState(new IdleState(this, StateMachine));
    StateMachine.AddState(new MoveState(this, StateMachine));
    StateMachine.AddState(new DeadState(this, StateMachine));
    
    // Idle -> Move 전환 조건: 입력이 있을 때
    StateMachine.AddTransition<IdleState, MoveState>(() => Input.GetAxis("Horizontal") != 0);
    
    // Move -> Idle 전환 조건: 입력이 없을 때
    StateMachine.AddTransition<MoveState, IdleState>(() => Input.GetAxis("Horizontal") == 0);
    
    // 모든 상태에서 Dead로 전환 조건: HP가 0 이하일 때
    StateMachine.AddAnyTransition<DeadState>(() => p_context.HP <= 0);
    
    StateMachine.ChangeState<IdleState>();
}
```

## 의존성

- **BH_Lib.Log**: 로그 출력을 위해 사용됩니다. (선택적)
