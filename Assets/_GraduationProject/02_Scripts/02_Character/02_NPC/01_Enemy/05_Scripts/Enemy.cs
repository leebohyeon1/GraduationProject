using UnityEngine;
using Pathfinding;
using System;

#if UNITY_EDITOR
using UnityEditor; // Handles 클래스를 사용하기 위해 반드시 필요합니다.
#endif
[RequireComponent(typeof(AIPath),typeof(AiController)),RequireComponent(typeof(Enemy_AnimationEventHandler),typeof(ParrySystem))
,RequireComponent(typeof(EnemyHealth),typeof(EnemySpecialAbility),typeof(Mon_Stiffness))]
public class Enemy : MonoBehaviour
{
    [field: SerializeField] public EnemyStat enemyStat{ get; private set; }
    public AiController _aiController{get;private set;}
    public Animator animator{ get;  private set; }
    
    public AIPath aIPath{get;  private set; }
    public Player player{ get;  private set; }
    public Enemy_AnimationEventHandler animHandler{get;private set;}
    public Mon_Stiffness StiffnessSystem { get; private set; }
    public ParrySystem ParrySystem { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public EnemyHealth EnemyHealth { get; private set; }
    public EnemySpecialAbility specialAbility { get; private set; }

    public Vector3 StartPos { get; private set; }
    public BillboardUI BillboardUI{ get;  private set; }

    [SerializeField]private int _CurrentStiffness = 4;
    public int CurrentStiffness => _CurrentStiffness;
    public Enemy_Type EnemyType { get; private set; }
    BlackBoard blackboard => _aiController._aiBrain.blackboard;
    public enum Enemy_Type
    {
        Brave,
        Cowardly,
        Cunning
    }
    public GroupAi groupAi;
    public float NormalSpeed => enemyStat.MoveSpeed;
    public Transform LaunchPoint;
    public enum MonsterName
    {
        Brave,
        Coward,
        Cunning,
        Fire
    }
    protected void Awake()
    {
        player = GameObject.FindFirstObjectByType<Player>();
        BillboardUI = GetComponentInChildren<BillboardUI>();
        BillboardUI?.Initialize();
        animator = GetComponent<Animator>();
        animHandler = GetComponent<Enemy_AnimationEventHandler>();
        animHandler.Initalize();
        ParrySystem = GetComponent<ParrySystem>();
        ParrySystem.Initialize(this);
        StiffnessSystem = GetComponent<Mon_Stiffness>();
        StiffnessSystem.Initialize(this);
        EnemyHealth = GetComponent<EnemyHealth>();
        Debug.Log(EnemyHealth);
        EnemyHealth.InitializeHealth( this);
        specialAbility = GetComponent<EnemySpecialAbility>();
        specialAbility.Initialize(this);
        Movement = new EnemyMovement(this);
        StartPos = transform.position;
        _aiController = GetComponent<AiController>();
        _aiController.Initialize(this);
        SetState(EnemyState.Idle);
    }
    public void Init()
    {
        gameObject.SetActive(true);
        transform.position = StartPos;
        EnemyHealth.InitializeHealth( this);
        _aiController.Initialize(this);
        StiffnessSystem.Initialize(this);
        specialAbility.Initialize(this);
        groupAi.GroupAdd(this);
        Movement.StopMovement();
        BillboardUI?.Initialize();
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
    [SerializeField] public EnemyState CurrentState { get; private set; }
    public void SetState(EnemyState state)
    {
        CurrentState = state;
        blackboard.SetValue("CurrentStatus", CurrentState);
    }
    #endregion
    
    public void AnimationEvent(string eventName)
    {
        if (animator != null)
        {
            animator.SetTrigger(eventName);
        }
    }
    public void AnimationBool(string boolName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(boolName, value);
        }
    }


    #region gizmo
    [Header("Debug Info")]
    private float _currentRadius;
    [Header("Debug/Preview Attack Range")]
    [Tooltip("런타임 아닐 때,SO넣어서 미리보기")]
    public EnemyAttackData editorPreviewData;
    private EnemyAttackData _runtimeAttackData;
    public void SetCurrentAttackData(EnemyAttackData data)
    {
        _runtimeAttackData = data;
    }
    private void OnDrawGizmosSelected()
    {

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
        EnemyAttackData dataToDraw = null;

        if (Application.isPlaying)
        {
            dataToDraw = _runtimeAttackData;
        }
        else
        {
            // 게임 실행 중이 아닐 때는 인스펙터에 넣어둔 데이터를 그립니다.
            dataToDraw = editorPreviewData;
        }
        if (dataToDraw == null) return;
        if (dataToDraw.shape == AttackShape.Sphere && dataToDraw.damageRadius <= 0) return;
        if (dataToDraw.shape == AttackShape.Box && dataToDraw.boxSize == Vector3.zero) return;
        Gizmos.color = Color.red;
        Vector3 attackOrigin = transform.position + transform.TransformDirection(dataToDraw.attackOffset);

        switch (dataToDraw.shape)
        {
            case AttackShape.Sphere:
                Gizmos.DrawWireSphere(attackOrigin, dataToDraw.damageRadius);
                break;

            case AttackShape.Box:
                // 박스는 회전이 필요하므로 Matrix 조작
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(attackOrigin, transform.rotation, Vector3.one);
                Gizmos.matrix = rotationMatrix;
                // OverlapBox는 HalfExtents를 쓰지만 DrawWireCube는 전체 Size를 씁니다.
                Gizmos.DrawWireCube(Vector3.zero, dataToDraw.boxSize);
                Gizmos.matrix = Matrix4x4.identity; // Matrix 복구
                break;

            case AttackShape.Fan:
                // 부채꼴 (Handles는 Editor에서만 작동)
                Handles.color = new Color(1f, 0f, 0f, 0.2f); // 반투명 빨강
                
                // 시작 각도 계산 (몬스터의 정면 기준)
                // 부채꼴의 왼쪽 끝 방향 벡터
                Vector3 startDir = Quaternion.Euler(0, -dataToDraw.fanAngle * 0.5f, 0) * transform.forward;

                // 부채꼴 그리기 (위치, 축, 시작방향, 각도, 반지름)
                Handles.DrawSolidArc(attackOrigin, Vector3.up, startDir, dataToDraw.fanAngle, dataToDraw.damageRadius);
                
                // 외곽선 진하게
                Handles.color = Color.red;
                Handles.DrawWireArc(attackOrigin, Vector3.up, startDir, dataToDraw.fanAngle, dataToDraw.damageRadius);

                break;
        }
    }
    #endregion
}
