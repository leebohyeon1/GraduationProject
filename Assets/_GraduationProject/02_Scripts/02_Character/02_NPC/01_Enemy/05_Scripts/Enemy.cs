using UnityEngine;
using Pathfinding;

/// <summary>
/// 몬스터의 기본 베이스 클래스입니다. 모든 몬스터는 이 클래스를 상속받거나 포함합니다.
/// </summary>
[RequireComponent(typeof(AIPath), typeof(AiController), typeof(Enemy_AnimationEventHandler))]
[RequireComponent(typeof(ParrySystem), typeof(EnemyHealth), typeof(EnemySpecialAbility))]
[RequireComponent(typeof(Mon_Stiffness), typeof(EnemyStateController), typeof(EnemyAnimationBridge))]
[RequireComponent(typeof(EnemyInitializer), typeof(EnemyMovement))]
#if UNITY_EDITOR
[RequireComponent(typeof(EnemyGizmoDrawer))]
#endif
public class Enemy : MonoBehaviour
{
    /// <summary>
    /// 몬스터의 기본 스탯 데이터입니다.
    /// </summary>
    [field: SerializeField] public EnemyStat enemyStat{ get; set; }

    /// <summary>
    /// 런타임에 관리되는 몬스터 데이터입니다.
    /// </summary>
    public EnemyData Data{get; set;}

    /// <summary>
    /// 현재 몬스터의 상태를 반환합니다.
    /// </summary>
    public EnemyStateController.EnemyState CurrentState => _stateController?.CurrentState ?? EnemyStateController.EnemyState.Idle;

    /// <summary>
    /// 몬스터 이동 제어 컴포넌트입니다.
    /// </summary>
    public EnemyMovement Movement { get; set; }

    /// <summary>
    /// A* Pathfinding의 이동 컴포넌트입니다.
    /// </summary>
    public AIPath aIPath => _initializer?.GetCachedComponent<AIPath>();

    /// <summary>
    /// 애니메이터 컴포넌트입니다.
    /// </summary>
    public Animator animator => _animationBridge?.Animator;

    /// <summary>
    /// 애니메이션 이벤트를 처리하는 핸들러입니다.
    /// </summary>
    public Enemy_AnimationEventHandler animHandler => _initializer?.GetCachedComponent<Enemy_AnimationEventHandler>();

    /// <summary>
    /// 강인도(Stiffness) 시스템입니다.
    /// </summary>
    public Mon_Stiffness StiffnessSystem =>  _initializer?.GetCachedComponent<Mon_Stiffness>();

    /// <summary>
    /// 패링 시스템입니다.
    /// </summary>
    public ParrySystem ParrySystem => _initializer?.GetCachedComponent<ParrySystem>();

    /// <summary>
    /// 체력 및 피해 처리 시스템입니다.
    /// </summary>
    public EnemyHealth EnemyHealth => _initializer?.GetCachedComponent<EnemyHealth>();

    /// <summary>
    /// 몬스터 특수 능력 컴포넌트입니다.
    /// </summary>
    public EnemySpecialAbility specialAbility => _initializer?.GetCachedComponent<EnemySpecialAbility>();

    /// <summary>
    /// AI 제어 컴포넌트(Behavior Tree 등)입니다.
    /// </summary>
    public AiController _aiController => _initializer?.GetCachedComponent<AiController>();

    /// <summary>
    /// 방어/방패 시스템입니다.
    /// </summary>
    public EnemyShield Shield => _initializer?.GetCachedComponent<EnemyShield>();

    /// <summary>
    /// 몬스터 머리 위에 표시되는 빌보드 UI입니다.
    /// </summary>
    public BillboardUI BillboardUI => GetComponentInChildren<BillboardUI>();

    /// <summary>
    /// 투사체 발사 지점입니다.
    /// </summary>
    public Transform LaunchPoint { get; set; }

    /// <summary>
    /// 오브젝트 풀링을 위한 프리팹 어드레스 이름입니다.
    /// </summary>
    public string MonsterPrefabName { get; set; } 

    internal EnemyStateController _stateController;
    internal EnemyAnimationBridge _animationBridge;
    
    /// <summary>
    /// 초기화 담당 컴포넌트입니다.
    /// </summary>
    public EnemyInitializer _initializer{get; private set;}

    /// <summary>
    /// 타겟인 플레이어 컨트롤러를 참조합니다.
    /// </summary>
    public PlayerController player =>  Data?.Player;

    /// <summary>
    /// 현재 강인도 수치입니다.
    /// </summary>
    public int CurrentStiffness
    {
        get =>Data?.CurrentStiffness ?? 4;
        private set => Data.CurrentStiffness = value;
    }

    /// <summary>
    /// 그룹 AI 시스템 참조입니다.
    /// </summary>
    public GroupAi groupAi
    {
        get => Data?.GroupAi;
        set => Data.GroupAi = value;
    }

    /// <summary>
    /// 몬스터의 행동 타입입니다.
    /// </summary>
    public Enemy_Type EnemyType { get; private set; }    

    /// <summary>
    /// 초기 스폰 위치를 반환합니다.
    /// </summary>
    public Vector3 StartPos => Data?.StartPosition ?? transform.position;

    BlackBoard blackboard => _aiController._aiBrain.blackboard;

    /// <summary>
    /// 몬스터 행동 성향 정의
    /// </summary>
    public enum Enemy_Type
    {
        Brave,
        Cowardly,
        Cunning
    }

    /// <summary>
    /// 몬스터 종류 정의
    /// </summary>
    public enum MonsterName
    {
        Brave,
        Coward,
        Cunning,
        Fire
    }

    protected void Awake()
    {
        _initializer = GetComponent<EnemyInitializer>();
        _initializer.Initialize();
    }

    /// <summary>
    /// 오브젝트 풀에서 꺼낼 때 상태를 재설정합니다.
    /// </summary>
    public void Init()
    {
        _initializer?.Reinitialize();
    }

    /// <summary>
    /// 강인도 수치를 설정합니다.
    /// </summary>
    /// <param name="amount">설정할 양</param>
    public void SetStiffness(int amount)
    {
        CurrentStiffness = amount;
    }

    /// <summary>
    /// 현재 공격 데이터를 설정합니다 (디버깅용 기즈모 포함).
    /// </summary>
    /// <param name="data">공격 데이터</param>
     public void SetCurrentAttackData(EnemyAttackData data)
    {
#if UNITY_EDITOR
        var gizmoDrawer = GetComponent<EnemyGizmoDrawer>();
        gizmoDrawer?.SetRuntimeAttackData(data);
#endif
    }

    /// <summary>
    /// 몬스터의 상태를 변경합니다.
    /// </summary>
    /// <param name="newState">변경할 대상 상태</param>
    public void SetState(EnemyStateController.EnemyState newState)
    {
        _stateController.SetState(newState);
    }

    /// <summary>
    /// 애니메이션 이벤트를 처리합니다.
    /// </summary>
    /// <param name="eventName">발생한 이벤트 이름</param>
     public void AnimationEvent(string eventName)
    {
        if (_stateController != null && _stateController.IsStateLocked && eventName != "Die")
        {
            return;
        }

        Debug.Log($"[Enemy Animation Event] {gameObject.name}에서 이벤트 '{eventName}' 발생.");
        _animationBridge?.TriggerEvent(eventName);
    }

    /// <summary>
    /// 애니메이션 불(bool) 파라미터를 설정합니다.
    /// </summary>
    /// <param name="boolName">파라미터 이름</param>
    /// <param name="value">설정할 값</param>
    public void AnimationBool(string boolName, bool value)
    {
        _animationBridge?.SetBool(boolName, value);
    }

    /// <summary>
    /// 현재 몬스터의 상태를 디버그 로그로 출력합니다.
    /// </summary>
    [ContextMenu("Debug/Log Enemy Status")]
    public void LogEnemyStatus()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"<color=yellow>=== Enemy Status Debug: {gameObject.name} ===</color>");
        
        sb.AppendLine($"[State] Current: {CurrentState}, Locked: {(_stateController != null ? _stateController.IsStateLocked.ToString() : "N/A")}");
        sb.AppendLine($"[Anim] IsAttacking (Bridge): {(_animationBridge != null ? _animationBridge.IsAttacking.ToString() : "N/A")}");
        
        if (aIPath != null)
        {
            sb.AppendLine($"[AIPath] isStopped: {aIPath.isStopped}, canMove: {aIPath.canMove}, maxSpeed: {aIPath.maxSpeed}");
            sb.AppendLine($"[AIPath] destination: {aIPath.destination}, hasPath: {aIPath.hasPath}, pathPending: {aIPath.pathPending}");
        }
        else
        {
            sb.AppendLine("[AIPath] Component Missing");
        }

        if (Movement != null)
        {
            sb.AppendLine($"[Movement] NormalSpeed: {Movement._normalSpeed}");
        }

        if (blackboard != null)
        {
            sb.AppendLine("[Blackboard] --- Important Keys ---");
            string[] keysToLog = { "IsAttacking", "OnTakeHit", "IsPlayerDetected", "CurrentStatus" };
            foreach (var key in keysToLog)
            {
                if (blackboard.HasKey(key))
                {
                    sb.AppendLine($" - {key}: {blackboard.GetValue<object>(key)}");
                }
            }
        }

        Debug.Log(sb.ToString());
    }
}
