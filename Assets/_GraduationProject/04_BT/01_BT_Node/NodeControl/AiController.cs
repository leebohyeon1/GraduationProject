
using BehaviorTree;
using Pathfinding;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 몬스터의 AI 업데이트를 제어합니다. 화면 밖에서도 최소 업데이트를 보장하여 자연스러운 패트롤을 구현합니다.
/// </summary>
public class AiController : MonoBehaviour, IEventListener<string>
{
    [SerializeField] private ActionTree _behaviorTree;
    private ActionTree _runtimeBehaviorTree;
    private bool _runtimeAssetsInitialized;
    public AiBrain _aiBrain { get; private set; }
    private Enemy _enemy;
    private AIPath _aiPath;
    private Camera _mainCam;
    
    [SerializeField] private OnSwingMissSO _onSwingMissEvent;
    [SerializeField] private OnHealingSO _onHealingEvent;

    [field: SerializeField] public EnemyAttackData[] enemyAttackDatas { get; private set; }
    [HideInInspector] public EnemyAttackData[] inGameenemyAttackDatas { get; private set; }

    private static readonly FieldInfo RunningSubTreeInstanceField = typeof(RunSubTreeNode).GetField(
        "_runningSubTreeInstance",
        BindingFlags.Instance | BindingFlags.NonPublic);

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

        EnsureRuntimeAssets();
        _runtimeBehaviorTree?.SetRunner(owner, _aiBrain);
        RegisterRuntimeAttackData(statMultiplier);
        _runtimeBehaviorTree?.rootNode?.initNode();
    }

    private void OnDisable()
    {
        if (_onSwingMissEvent != null) _onSwingMissEvent.Unsubscribe(this);
        if (_onHealingEvent != null) _onHealingEvent.Unsubscribe(this);
    }

    private void OnDestroy()
    {
        ReleaseRuntimeAssets();
    }

    private void EnsureRuntimeAssets()
    {
        if (_runtimeAssetsInitialized)
        {
            return;
        }

        _runtimeBehaviorTree = _behaviorTree != null ? _behaviorTree.Clone() : null;

        int attackDataCount = enemyAttackDatas != null ? enemyAttackDatas.Length : 0;
        inGameenemyAttackDatas = new EnemyAttackData[attackDataCount];
        for (int i = 0; i < attackDataCount; i++)
        {
            EnemyAttackData source = enemyAttackDatas[i];
            if (source != null)
            {
                inGameenemyAttackDatas[i] = Instantiate(source);
            }
        }

        // 빈 공격 데이터 배열도 초기화가 끝난 상태입니다. 별도 플래그로 재활성화 시
        // 트리와 데이터를 다시 복제하지 않도록 보장합니다.
        _runtimeAssetsInitialized = true;
    }

    private void RegisterRuntimeAttackData(EnemyStatMultiplier statMultiplier)
    {
        if (inGameenemyAttackDatas == null)
        {
            return;
        }

        float attackMultiplier = statMultiplier != null ? statMultiplier.AttackMultiply : 1f;
        for (int i = 0; i < inGameenemyAttackDatas.Length; i++)
        {
            EnemyAttackData runtimeData = inGameenemyAttackDatas[i];
            EnemyAttackData source = enemyAttackDatas[i];
            if (runtimeData == null || source == null)
            {
                continue;
            }

            // 재초기화 때 누적 곱셈이 생기지 않도록 항상 원본 값을 기준으로 갱신합니다.
            DamageData damageData = runtimeData.damageData;
            damageData.DamageAmount = (int)(source.damageData.DamageAmount * attackMultiplier);
            runtimeData.damageData = damageData;
            _aiBrain.AddEnemyAttackData(runtimeData);
        }
    }

    private void ReleaseRuntimeAssets()
    {
        if (_runtimeBehaviorTree != null)
        {
            DestroyRuntimeNodeGraph(_runtimeBehaviorTree.rootNode, new HashSet<Node>());
            DestroyRuntimeObject(_runtimeBehaviorTree);
            _runtimeBehaviorTree = null;
        }

        if (inGameenemyAttackDatas != null)
        {
            for (int i = 0; i < inGameenemyAttackDatas.Length; i++)
            {
                DestroyRuntimeObject(inGameenemyAttackDatas[i]);
            }

            inGameenemyAttackDatas = null;
        }

        _runtimeAssetsInitialized = false;
    }

    private static void DestroyRuntimeNodeGraph(Node node, HashSet<Node> visited)
    {
        if (node == null || !visited.Add(node))
        {
            return;
        }

        if (node is CompositeNode composite)
        {
            if (composite.nodes != null)
            {
                for (int i = 0; i < composite.nodes.Length; i++)
                {
                    DestroyRuntimeNodeGraph(composite.nodes[i], visited);
                }
            }

            if (composite.services != null)
            {
                for (int i = 0; i < composite.services.Count; i++)
                {
                    DestroyRuntimeNodeGraph(composite.services[i], visited);
                }
            }
        }
        else if (node is Decorator_Inverter inverter)
        {
            DestroyRuntimeNodeGraph(inverter.child, visited);
        }

        // RunSubTreeNode는 실행할 때마다 비직렬화 노드를 별도로 만듭니다.
        // 해당 노드도 씬 언로드 시 추적해 명시적으로 제거합니다.
        if (node is RunSubTreeNode && RunningSubTreeInstanceField != null)
        {
            Node runningSubTree = RunningSubTreeInstanceField.GetValue(node) as Node;
            RunningSubTreeInstanceField.SetValue(node, null);
            DestroyRuntimeNodeGraph(runningSubTree, visited);
        }

        DestroyRuntimeObject(node);
    }

    private static void DestroyRuntimeObject(Object runtimeObject)
    {
        if (runtimeObject == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(runtimeObject);
        }
        else
        {
            Object.DestroyImmediate(runtimeObject);
        }
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
        _runtimeBehaviorTree?.rootNode?.Evaluate();
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
        if (_runtimeBehaviorTree == null || _runtimeBehaviorTree.rootNode == null)
        {
            return null;
        }

        return FindServiceInNode<T>(_runtimeBehaviorTree.rootNode);
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
