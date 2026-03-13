using UnityEngine;
using Pathfinding;
[RequireComponent(typeof(AIPath), typeof(AiController), typeof(Enemy_AnimationEventHandler))]
[RequireComponent(typeof(ParrySystem), typeof(EnemyHealth), typeof(EnemySpecialAbility))]
[RequireComponent(typeof(Mon_Stiffness), typeof(EnemyStateController), typeof(EnemyAnimationBridge))]
[RequireComponent(typeof(EnemyInitializer), typeof(EnemyMovement))]
#if UNITY_EDITOR
[RequireComponent(typeof(EnemyGizmoDrawer))]
#endif
public class Enemy : MonoBehaviour
{
    [field: SerializeField] public EnemyStat enemyStat{ get; set; }
    public EnemyData Data{get; set;}
    public EnemyStateController.EnemyState CurrentState => _stateController?.CurrentState ?? EnemyStateController.EnemyState.Idle;
    public EnemyMovement Movement { get; set; }
    public AIPath aIPath => _initializer?.GetCachedComponent<AIPath>();
    public Animator animator => _animationBridge?.Animator;
    public Enemy_AnimationEventHandler animHandler => _initializer?.GetCachedComponent<Enemy_AnimationEventHandler>();
    public Mon_Stiffness StiffnessSystem =>  _initializer?.GetCachedComponent<Mon_Stiffness>();
    public ParrySystem ParrySystem => _initializer?.GetCachedComponent<ParrySystem>();
    public EnemyHealth EnemyHealth => _initializer?.GetCachedComponent<EnemyHealth>();
    public EnemySpecialAbility specialAbility => _initializer?.GetCachedComponent<EnemySpecialAbility>();
    public AiController _aiController => _initializer?.GetCachedComponent<AiController>();
    public EnemyShield Shield => _initializer?.GetCachedComponent<EnemyShield>();
    public BillboardUI BillboardUI => GetComponentInChildren<BillboardUI>();
    public Transform LaunchPoint { get; set; }
    internal EnemyStateController _stateController;
    internal EnemyAnimationBridge _animationBridge;
    public EnemyInitializer _initializer{get; private set;}
    public PlayerController player =>  Data?.Player;
    public int CurrentStiffness
    {
        get =>Data?.CurrentStiffness ?? 4;
        private set => Data.CurrentStiffness = value;
    }
    public GroupAi groupAi
    {
        get => Data?.GroupAi;
        set => Data.GroupAi = value;
    }
    public Enemy_Type EnemyType { get; private set; }    
    public Vector3 StartPos => Data?.StartPosition ?? transform.position;
    BlackBoard blackboard => _aiController._aiBrain.blackboard;
    public enum Enemy_Type
    {
        Brave,
        Cowardly,
        Cunning
    }
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
    public void Init()
    {
        _initializer?.Reinitialize();
    }

    public void SetStiffness(int amount)
    {
        CurrentStiffness = amount;
    }
     public void SetCurrentAttackData(EnemyAttackData data)
    {
#if UNITY_EDITOR
        var gizmoDrawer = GetComponent<EnemyGizmoDrawer>();
        gizmoDrawer?.SetRuntimeAttackData(data);
#endif
    }
    public void SetState(EnemyStateController.EnemyState newState)
    {
        _stateController.SetState(newState);
    }
     public void AnimationEvent(string eventName)
    {
        if (_stateController != null && _stateController.IsStateLocked && eventName != "Die")
        {
            return;
        }

        Debug.Log($"[Enemy Animation Event] {gameObject.name}에서 이벤트 '{eventName}' 발생.");
        _animationBridge?.TriggerEvent(eventName);
    }
    public void AnimationBool(string boolName, bool value)
    {
        _animationBridge?.SetBool(boolName, value);
    }

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
    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if (hit.collider.CompareTag("Player"))
        {
            // 플레이어와 충돌했을 때의 처리
            Debug.Log($"[Enemy] {gameObject.name} collided with Player.");
            // 예: 플레이어에게 피해를 주거나, 스턴을 적용하는 등의 로직을 여기에 추가
            if(hit.point.y < transform.position.y)
            {
                
            }
        }
    }
}
