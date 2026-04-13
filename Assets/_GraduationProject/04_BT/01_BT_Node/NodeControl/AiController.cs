using BehaviorTree;
using Pathfinding;
using UnityEngine;

/// <summary>
/// 몬스터의 AI 업데이트를 제어합니다.
/// 전투/중요 상태는 즉시성을 유지하고, 원거리/비가시 상태는 업데이트 비용을 줄입니다.
/// </summary>
public class AiController : MonoBehaviour, IEventListener<string>
{
    private enum UpdateTier
    {
        Combat,
        Near,
        Mid,
        Far,
        Culled
    }

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
    [SerializeField] private float _viewMargin = 0.5f;
    [SerializeField] private float _nearDistanceRatio = 0.4f;

    [Header("Behavior Tick (Frame)")]
    [SerializeField] private int _combatFrameInterval = 1;
    [SerializeField] private int _nearFrameInterval = 2;
    [SerializeField] private int _midFrameInterval = 4;
    [SerializeField] private int _farFrameInterval = 8;
    [SerializeField] private int _culledFrameInterval = 12;

    [Header("Sensing Tick (Seconds)")]
    [SerializeField] private float _combatSensingInterval = 0.10f;
    [SerializeField] private float _nearSensingInterval = 0.15f;
    [SerializeField] private float _midSensingInterval = 0.25f;
    [SerializeField] private float _farSensingInterval = 0.40f;
    [SerializeField] private float _culledSensingInterval = 0.60f;

    private int _staggerOffset;
    private bool _eventsSubscribed;
    private bool _isPathCulled;

    private void OnEnable()
    {
        TrySubscribeEvents();
    }

    private void OnDisable()
    {
        TryUnsubscribeEvents();
        SetPathCulled(true);
    }

    public void Initialize(Enemy owner, EnemyStatMultiplier statMultiplier = default)
    {
        _enemy = owner;
        _aiPath = owner.GetComponent<AIPath>();
        _mainCam = Camera.main;
        _staggerOffset = Random.Range(0, Mathf.Max(1, _culledFrameInterval));
        _isPathCulled = false;

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
                float c = statMultiplier != null ? statMultiplier.AttackMultiply : 1f;
                inGameenemyAttackDatas[i].damageData.DamageAmount = (int)(inGameenemyAttackDatas[i].damageData.DamageAmount * c);
                _aiBrain.AddEnemyAttackData(inGameenemyAttackDatas[i]);
            }
        }

        _behaviorTree.rootNode?.initNode();
        TrySubscribeEvents();
    }

    private void Update()
    {
        if (_enemy == null || _enemy.EnemyHealth == null || _enemy.EnemyHealth.IsDead || _aiBrain == null) return;
        if (_enemy._initializer != null && !_enemy._initializer.IsInitialized("Final")) return;

        bool isImportantState = IsImportantState();
        UpdateTier tier = isImportantState ? UpdateTier.Combat : ResolveUpdateTier();

        _aiBrain.TickSensing(Time.time, GetSensingInterval(tier));
        SetPathCulled(tier == UpdateTier.Culled && !isImportantState);

        if (!ShouldRunBehavior(isImportantState, tier)) return;

        _aiBrain.Tick(Time.deltaTime);
        _behaviorTree?.rootNode?.Evaluate();
    }

    private bool IsImportantState()
    {
        bool isCombat = _aiBrain._isCombat;
        bool isReturningHome = _aiBrain.blackboard.GetValue<bool>("GoHome");
        bool isEngaged = _aiBrain.blackboard.GetValue<bool>(EnemyBlackboardKeys.Engage);
        bool isDetecting = _aiBrain.blackboard.GetValue<bool>(EnemyBlackboardKeys.DetectPlayer) ||
                           _aiBrain.blackboard.GetValue<bool>(EnemyBlackboardKeys.IsHasLOS);

        return isCombat || isReturningHome || isEngaged || isDetecting ||
               _enemy.CurrentState == EnemyStateController.EnemyState.Stunned ||
               _enemy.CurrentState == EnemyStateController.EnemyState.Hit;
    }

    private UpdateTier ResolveUpdateTier()
    {
        if (_enemy.player == null) return IsInExtendedView() ? UpdateTier.Mid : UpdateTier.Far;

        float distSq = (transform.position - _enemy.player.transform.position).sqrMagnitude;
        float hardCullSq = _hardCullDistance * _hardCullDistance;
        if (distSq > hardCullSq) return UpdateTier.Culled;

        float nearDistance = Mathf.Max(3f, _lodDistance * Mathf.Clamp01(_nearDistanceRatio));
        float nearSq = nearDistance * nearDistance;
        if (distSq <= nearSq) return UpdateTier.Near;

        float lodSq = _lodDistance * _lodDistance;
        if (distSq <= lodSq) return UpdateTier.Mid;

        return IsInExtendedView() ? UpdateTier.Mid : UpdateTier.Far;
    }

    private bool ShouldRunBehavior(bool isImportantState, UpdateTier tier)
    {
        if (isImportantState || tier == UpdateTier.Combat) return true;
        if (tier == UpdateTier.Culled) return false;

        int interval = GetBehaviorInterval(tier);
        return (Time.frameCount + _staggerOffset) % interval == 0;
    }

    private int GetBehaviorInterval(UpdateTier tier)
    {
        switch (tier)
        {
            case UpdateTier.Combat: return Mathf.Max(1, _combatFrameInterval);
            case UpdateTier.Near: return Mathf.Max(1, _nearFrameInterval);
            case UpdateTier.Mid: return Mathf.Max(1, _midFrameInterval);
            case UpdateTier.Far: return Mathf.Max(1, _farFrameInterval);
            case UpdateTier.Culled: return Mathf.Max(1, _culledFrameInterval);
            default: return 1;
        }
    }

    private float GetSensingInterval(UpdateTier tier)
    {
        switch (tier)
        {
            case UpdateTier.Combat: return Mathf.Max(0.02f, _combatSensingInterval);
            case UpdateTier.Near: return Mathf.Max(0.02f, _nearSensingInterval);
            case UpdateTier.Mid: return Mathf.Max(0.02f, _midSensingInterval);
            case UpdateTier.Far: return Mathf.Max(0.02f, _farSensingInterval);
            case UpdateTier.Culled: return Mathf.Max(0.02f, _culledSensingInterval);
            default: return 0.1f;
        }
    }

    private void SetPathCulled(bool culled)
    {
        if (_aiPath == null || _isPathCulled == culled) return;

        _isPathCulled = culled;
        _aiPath.canMove = !culled;
        _aiPath.isStopped = culled;
    }

    private void TrySubscribeEvents()
    {
        if (_eventsSubscribed) return;

        if (_onSwingMissEvent != null) _onSwingMissEvent.Subscribe(this);
        if (_onHealingEvent != null) _onHealingEvent.Subscribe(this);
        _eventsSubscribed = true;
    }

    private void TryUnsubscribeEvents()
    {
        if (!_eventsSubscribed) return;

        if (_onSwingMissEvent != null) _onSwingMissEvent.Unsubscribe(this);
        if (_onHealingEvent != null) _onHealingEvent.Unsubscribe(this);
        _eventsSubscribed = false;
    }

    private bool IsInExtendedView()
    {
        if (_mainCam == null) _mainCam = Camera.main;
        if (_mainCam == null) return true;

        Vector3 viewPos = _mainCam.WorldToViewportPoint(transform.position);
        return viewPos.z > 0 &&
               viewPos.x > -_viewMargin && viewPos.x < 1f + _viewMargin &&
               viewPos.y > -_viewMargin && viewPos.y < 1f + _viewMargin;
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
