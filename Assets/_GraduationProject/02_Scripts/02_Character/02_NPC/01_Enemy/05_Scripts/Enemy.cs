using UnityEngine;
using Pathfinding;
[RequireComponent(typeof(AIPath), typeof(AiController), typeof(Enemy_AnimationEventHandler))]
[RequireComponent(typeof(ParrySystem), typeof(EnemyHealth), typeof(EnemySpecialAbility))]
[RequireComponent(typeof(Mon_Stiffness), typeof(EnemyStateController), typeof(EnemyAnimationBridge))]
[RequireComponent(typeof(EnemyInitializer))]
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
    public BillboardUI BillboardUI => GetComponentInChildren<BillboardUI>();
    public Transform LaunchPoint { get; set; }
    //내부 컴포넌트
    internal EnemyStateController _stateController;
    internal EnemyAnimationBridge _animationBridge;
    public EnemyInitializer _initializer{get; private set;}
    //데이터
    public Player player =>  Data?.Player;
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
        _animationBridge?.TriggerEvent(eventName);
    }
    public void AnimationBool(string boolName, bool value)
    {
        Debug.Log($"AnimationBool {boolName} set to {value}");
        Debug.Log($"_animationBridge is {_animationBridge}");
        _animationBridge?.SetBool(boolName, value);
    }
    public float NormalSpeed => enemyStat.MoveSpeed;

    public bool AnimationBasedMovement ;

    public void StopMovement()
    {
        blackboard.SetValue(EnemyBlackboardKeys.StopMovement, true);
    }
}
