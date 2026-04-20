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

        if (!isImportantState)
        {
            bool isVisible = IsInExtendedView();
            float distSq = (_enemy.player != null) ? (transform.position - _enemy.player.transform.position).sqrMagnitude : float.MaxValue;
            
            // 1. 아주 먼 거리 하드 컬링 (최소화)
            if (distSq > _hardCullDistance * _hardCullDistance) return;

            // 2. 가변 업데이트 주기 (Soft LOD)
            // 화면 안이면 정상 속도, 화면 밖이면 4배 느리게 (0.3초 주기)
            int effectiveInterval = isVisible ? Mathf.Max(1, _onScreenTickInterval) : _tickInterval * 4;
            if ((Time.frameCount + _staggerOffset) % effectiveInterval != 0) return;

            // 3. AIPath 컴포넌트는 항상 켜두고 isStopped로만 제어하여 즉각 반응 유도
            if (_aiPath != null && !_aiPath.enabled) _aiPath.enabled = true;
        }
        else
        {
            if (_aiPath != null && !_aiPath.enabled) _aiPath.enabled = true;
        }

        _aiBrain?.Tick(Time.deltaTime);
        _behaviorTree?.rootNode?.Evaluate();
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
