using UnityEngine;
using Pathfinding;
using System;

#if UNITY_EDITOR
using UnityEditor; // Handles 클래스를 사용하기 위해 반드시 필요합니다.
#endif
[RequireComponent(typeof(AIPath),typeof(AiController)),RequireComponent(typeof(Enemy_AnimationEventHandler),typeof(ParrySystem))
,RequireComponent(typeof(Monster_HeatSystem),typeof(Mon_Stiffness)),RequireComponent(typeof(EnemyTakeDmg),typeof(EnemySpecialAbility))]
public class Enemy : MonoBehaviour
{
    public AiController _aiController{get;private set;}
    public Animator animator{ get;  private set; }
    
    public AIPath aIPath{get;  private set; }
    public Player player{ get;  private set; }
    public Enemy_AnimationEventHandler animHandler{get;private set;}
    public Mon_Stiffness StiffnessSystem { get; private set; }
    public ParrySystem ParrySystem { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public Monster_HeatSystem heatSystem { get; private set; }
    public EnemyTakeDmg EnemyHealth { get; private set; }
    public EnemySpecialAbility specialAbility { get; private set; }

    public Vector3 StartPos { get; private set; }
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;
    Rigidbody rb;


    public Vector3[] wayPoints;
    public int wayPointIndex = 0;
    [SerializeField]private int _CurrentStiffness = 4;
    public int CurrentStiffness => _CurrentStiffness;
    [SerializeField] public Enemy_Type EnemyType;
    public int MaxHealth;
    public enum Enemy_Type
    {
        Brave,
        Cowardly,
        Cunning
    }
    public GroupAi groupAi{get;private set;}
    [SerializeField] private float _normalSpeed = 2f;
    public float NormalSpeed => _normalSpeed;
    public Transform LaunchPoint;
    public enum MonsterName
    {
        Brave,
        Coward,
        Cunning,
        Fire
    }
    public static Enemy_Type Spawn<Enemy_Type>( Transform parent, MonsterName MonsterPrefabName, Vector3 position = default) where Enemy_Type : Enemy
    {
        GameObject prefabObject = GameObject.Instantiate(Resources.Load("MonsterPrefab/" + MonsterPrefabName)) as GameObject;

        Enemy_Type enemy = prefabObject.GetComponent<Enemy_Type>();
        if (enemy == null)
        {
            enemy = prefabObject.AddComponent<Enemy_Type>();
        }
        if (parent != null)
        {
            prefabObject.transform.SetParent(parent);
        }
        prefabObject.transform.localScale = Vector3.one * enemy.transform.localScale.x;

        prefabObject.transform.position = position;

        return enemy;
    }
    protected void Awake()
    {
        player = GameObject.FindFirstObjectByType<Player>();
        rb = GetComponent<Rigidbody>();

        animator = GetComponent<Animator>();
        _aiController = GetComponent<AiController>();
        _aiController.Initialize(this);
        animHandler = GetComponent<Enemy_AnimationEventHandler>();
        heatSystem = GetComponent<Monster_HeatSystem>();
        heatSystem.Init(ActorType.Monster);
        ParrySystem = GetComponent<ParrySystem>();
        ParrySystem.Initialize(this);
        StiffnessSystem = GetComponent<Mon_Stiffness>();
        StiffnessSystem.Initialize(this);
        EnemyHealth = GetComponent<EnemyTakeDmg>();
        EnemyHealth.InitializeHealth(MaxHealth, this);
        specialAbility = GetComponent<EnemySpecialAbility>();
        specialAbility.Initialize(this);
        Movement = new EnemyMovement(this);
        StartPos = transform.position;
    }
    public void Init()
    {
        gameObject.SetActive(true);
        transform.position = StartPos;
        EnemyHealth.InitializeHealth(MaxHealth, this);
        _aiController.Initialize(this);
        StiffnessSystem.Initialize(this);
        specialAbility.Initialize(this);
        groupAi.GroupAdd(this);
        heatSystem.Init(ActorType.Monster);
        Movement.StopMovement();
        SetState(EnemyState.Idle);

    }
    void Start()
    {
        aIPath = GetComponent<AIPath>();
        if (aIPath == null)
        {
            Debug.LogError("AIPath component not found in the scene.");
        }
        StartPos = transform.position;
        groupAi = FindFirstObjectByType<GroupAi>();
        if (groupAi == null)
        {
            GameObject groupObj = new GameObject("EnemyGroup");
            groupAi = groupObj.AddComponent<GroupAi>();
        }
        groupAi.GroupAdd(this);
    }
    public void SetStiffness(int amount)
    {
        _CurrentStiffness = amount;
    }
    #region Enemy State Management
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Beam,
        Noise,
        Die,
        Stunned, // 스턴 상태 추가
        Rush,
        Hit,
        RunAway
    }
    [SerializeField]public EnemyState CurrentState { get; private set; } = EnemyState.Idle;
    public void SetState(EnemyState state)
    {
        CurrentState = state;
    }
    #endregion
    
    public void AnimationEvent(string eventName)
    {
        if (animator != null)
        {
            animator.SetTrigger(eventName);
            CalculationResult stat = heatSystem.CalculationHeat("Test", ActorType.Monster, heatSystem.GetTier(), 0);
            animator.speed = stat.FinalAnimSpeed;

        }
    }
    public void AnimationBool(string boolName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(boolName, value);
            CalculationResult stat = heatSystem.CalculationHeat("Test", ActorType.Monster, heatSystem.GetTier(), 0);
            animator.speed = stat.FinalAnimSpeed;
        }
    }
    private GameObject LastRushHitObject;
    public GameObject GetLastRushHitObject()
    {
        return LastRushHitObject;
    }
    public void SetLastRushHitObject(GameObject obj)
    {
        LastRushHitObject = obj;
    }
    public GameObject TempRushObject{ get; private set;}
    private void OnCollisionEnter(Collision collision)
    {
        if (CurrentState != EnemyState.Rush) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (GetLastRushHitObject() == collision.gameObject)
            {
                return;
            }
            SetLastRushHitObject(collision.gameObject);
            TempRushObject = collision.gameObject;

        }
    }


    #region gizmo
    [Header("Attack Range")]
    public float _currentAttackRadius;
    public Vector3 _currentAttackOffset;
    public void SetCurrentAttackData(float radius, Vector3 offset)
    {
        _currentAttackRadius = radius;
        _currentAttackOffset = offset;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 attackOrigin = transform.position + transform.TransformDirection(_currentAttackOffset);

        Gizmos.DrawWireSphere(attackOrigin, _currentAttackRadius);
#if UNITY_EDITOR
        // 기즈모 라벨의 스타일을 설정합니다.
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        // 1m부터 30m까지 원과 텍스트를 그립니다.
         for (int i = 1; i <= 30; i++)
        {
            Handles.color = Color.Lerp(Color.green, Color.blue, i / 30f);

            Handles.DrawWireDisc(transform.position, Vector3.up, i);
            
            Vector3 textPosition = transform.position + transform.forward * i;
            Handles.Label(textPosition, $"{i}m", style);
        }
#endif
    }
    #endregion
}
