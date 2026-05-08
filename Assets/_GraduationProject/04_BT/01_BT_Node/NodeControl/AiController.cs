
using BehaviorTree;
using Pathfinding;
using UnityEngine;

/// <summary>
/// 몬스터의 AI 업데이트를 제어합니다. 화면 밖에서도 최소 업데이트를 보장하여 자연스러운 패트롤을 구현합니다.
/// </summary>
public class AiController : MonoBehaviour, IEventListener<string>
{
    [SerializeField] private ActionTree _behaviorTree;
    public AiBrain _aiBrain { get; private set; }
    private Enemy _enemy;
    private AIPath _aiPath;
    private Camera _mainCam;
    
    [SerializeField] private OnSwingMissSO _onSwingMissEvent;
    [SerializeField] private OnHealingSO _onHealingEvent;

    [field: SerializeField] public EnemyAttackData[] enemyAttackDatas { get; private set; }
    [HideInInspector] public EnemyAttackData[] inGameenemyAttackDatas { get; private set; }

    [Header("Optimization Settings")]
    [SerializeField] private float _lodDistance = 25f;      
    [SerializeField] private float _hardCullDistance = 60f; 
    [SerializeField] private int _tickInterval = 5;         
    [SerializeField] private float _viewMargin = 0.5f;      
    [SerializeField] private int _onScreenTickInterval = 1;
    private int _staggerOffset;

    private void OnEnable()
    {
        if (_onSwingMissEvent != null) _onSwingMissEvent.Subscribe(this);
        if (_onHealingEvent != null) _onHealingEvent.Subscribe(this);
    }

    public void Initialize(Enemy owner, EnemyStatMultiplier statMultiplier = default)
    {
        _enemy = owner;
        _aiPath = owner.GetComponent<AIPath>();
        _mainCam = Camera.main;
        _staggerOffset = Random.Range(0, _tickInterval);

        if (_aiBrain == null) _aiBrain = new AiBrain(owner);
        else _aiBrain.ResetBrain();

        if (inGameenemyAttackDatas == null || inGameenemyAttackDatas.Length == 0)
        {
            _behaviorTree = _behaviorTree.Clone();
            _behaviorTree.SetRunner(owner, _aiBrain);
            inGameenemyAttackDatas = new EnemyAttackData[enemyAttackDatas.Length];
            for (int i = 0; i < enemyAttackDatas.Length; i++)
            {
                inGameenemyAttackDatas[i] = Instantiate(enemyAttackDatas[i]);
                float c = statMultiplier != null ? statMultiplier.AttackMultiply : 1;
                inGameenemyAttackDatas[i].damageData.DamageAmount = (int)(inGameenemyAttackDatas[i].damageData.DamageAmount * c);
                _aiBrain.AddEnemyAttackData(inGameenemyAttackDatas[i]);
            }
        }
        _behaviorTree.rootNode?.initNode();
    }

    private void OnDisable()
    {
        if (_onSwingMissEvent != null) _onSwingMissEvent.Unsubscribe(this);
        if (_onHealingEvent != null) _onHealingEvent.Unsubscribe(this);
    }

    private void Update()
    {
        if (_enemy == null || _enemy.EnemyHealth.IsDead) return;

        bool isCombat = _aiBrain != null && _aiBrain._isCombat;
        bool isReturningHome = _aiBrain != null && _aiBrain.blackboard.GetValue<bool>("GoHome");
        bool isEngaged = _aiBrain != null && _aiBrain.blackboard.GetValue<bool>(EnemyBlackboardKeys.Engage);
        bool isDetecting = _aiBrain != null && (
            _aiBrain.blackboard.GetValue<bool>(EnemyBlackboardKeys.DetectPlayer) ||
            _aiBrain.blackboard.GetValue<bool>(EnemyBlackboardKeys.IsHasLOS)
        );
        
        bool isImportantState = isCombat || isReturningHome || isEngaged || isDetecting ||
                                _enemy.CurrentState == EnemyStateController.EnemyState.Stunned || 
                                _enemy.CurrentState == EnemyStateController.EnemyState.Hit;
        bool isVisible = IsInExtendedView();
        float distSq = (_enemy.player != null) ? (transform.position - _enemy.player.transform.position).sqrMagnitude : float.MaxValue;
        if (distSq > _hardCullDistance * _hardCullDistance) return;

        // 3. 상황별 Tick Interval 결정 (숫자가 작을수록 자주 업데이트)
        int effectiveInterval;

        if (isImportantState || isVisible)
        {
            // 화면 안에 있거나 전투 중이면 아주 빠르게 업데이트 (1프레임 혹은 설정값)
            effectiveInterval = _onScreenTickInterval; 
        }
        else if (distSq < _lodDistance * _lodDistance)
        {
            // 화면 밖이지만 플레이어와 가까운 경우 (중간 속도)
            effectiveInterval = _tickInterval;
        }
        else
        {
            // 화면 밖이고 멀리 있는 경우 (매우 느리게)
            effectiveInterval = _tickInterval * 4;
        }

        // 4. Tick 실행 여부 결정
        if ((Time.frameCount + _staggerOffset) % Mathf.Max(1, effectiveInterval) != 0) return;
        // Debug.Log($"{name} - Effective Tick Interval: {effectiveInterval}");
        // 5. 실제 AI 실행
        _aiBrain?.Tick(Time.deltaTime * effectiveInterval); // 델타 타임에 간격을 곱해줘야 물리/이동이 자연스럽습니다.
        _behaviorTree?.rootNode?.Evaluate();
    }

    private bool IsInExtendedView()
    {
        if (_mainCam == null) _mainCam = Camera.main;
        if (_mainCam == null) return true;

        // 1. 오브젝트의 렌더러에서 '전체 크기(Bounds)'를 가져옵니다.
        // Renderer가 없다면 Collider의 bounds를 써도 됩니다.
        Bounds bounds = GetComponent<Collider>().bounds;
        bounds.Expand(_viewMargin); // 마진만큼 판정 영역 확장

        // 2. 카메라의 절두체(Frustum) 평면들을 가져옵니다.
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_mainCam);
        // 3. 이 평면들 안에 오브젝트의 Bounds가 일부라도 포함되는지 검사합니다.
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    public bool IsActionable() => _aiBrain?.IsActionable() ?? false;
    public void CombatEnter(bool combat = true) { if (_aiBrain != null) _aiBrain.CombatEnter(combat); }

    public void OnEventTrigger(string eventName)
    {
        if (_aiBrain == null || !_aiBrain._isCombat) return;
        if (eventName == "OnSwingMiss") _aiBrain.blackboard.SetValue(EnemyBlackboardKeys.OnPlayerAirshot, true);
        else if (eventName == "OnHealing") _aiBrain.blackboard.SetValue(EnemyBlackboardKeys.OnPlayerRecovery, true);
    }

    public T GetService<T>() where T : ServiceNode
    {
        if (_behaviorTree == null || _behaviorTree.rootNode == null)
        {
            return null;
        }

        return FindServiceInNode<T>(_behaviorTree.rootNode);
    }

    private T FindServiceInNode<T>(Node node) where T : ServiceNode
    {
        if (node == null)
        {
            return null;
        }

        if (node is CompositeNode composite)
        {
            if (composite.services != null)
            {
                for (int i = 0; i < composite.services.Count; i++)
                {
                    ServiceNode service = composite.services[i];
                    if (service is T matched)
                    {
                        return matched;
                    }
                }
            }

            if (composite.nodes != null)
            {
                for (int i = 0; i < composite.nodes.Length; i++)
                {
                    T childResult = FindServiceInNode<T>(composite.nodes[i]);
                    if (childResult != null)
                    {
                        return childResult;
                    }
                }
            }
        }

        return null;
    }
}
