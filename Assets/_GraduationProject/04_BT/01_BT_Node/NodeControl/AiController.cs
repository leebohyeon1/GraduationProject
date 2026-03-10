using BehaviorTree;
using Pathfinding;
using UnityEngine;

/// <summary>
/// 몬스터의 AI 업데이트를 제어합니다. 거리에 따라 AIPath 활성화/비활성화 및 업데이트 빈도를 조절합니다.
/// </summary>
public class AiController : MonoBehaviour, IEventListener<string>
{
    [SerializeField] private ActionTree _behaviorTree;
    public AiBrain _aiBrain { get; private set; }
    private Enemy _enemy;
    private AIPath _aiPath;
    [SerializeField] private OnSwingMissSO _onSwingMissEvent;
    [SerializeField] private OnHealingSO _onHealingEvent;

    [field: SerializeField] public EnemyAttackData[] enemyAttackDatas { get; private set; }
    [HideInInspector] public EnemyAttackData[] inGameenemyAttackDatas { get; private set; }

    [Header("Optimization Settings")]
    [SerializeField] private float _lodDistance = 25f;
    [SerializeField] private int _tickInterval = 5;
    private int _staggerOffset;

    public void Initialize(Enemy owner, EnemyStatMultiplier statMultiplier = default)
    {
        _enemy = owner;
        _aiPath = owner.GetComponent<AIPath>();
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
        
        bool isImportantState = isCombat || isReturningHome || isEngaged ||
                                _enemy.CurrentState == EnemyStateController.EnemyState.Stunned || 
                                _enemy.CurrentState == EnemyStateController.EnemyState.Hit;

        if (!isImportantState && _enemy.player != null)
        {
            float distSq = (transform.position - _enemy.player.transform.position).sqrMagnitude;
            bool isFar = distSq > _lodDistance * _lodDistance;

            // [Change] CharacterController 대신 AIPath 컴포넌트 자체를 제어
            // 멀리 있으면 길찾기 및 이동 연산 자체를 중단 (물리는 중력 정도만 처리되도록 내버려둠)
            if (_aiPath != null && _aiPath.enabled == isFar) 
            {
                _aiPath.enabled = !isFar;
            }
            
            if (isFar) return; 
            
            int effectiveInterval = _tickInterval;
            if ((Time.frameCount + _staggerOffset) % effectiveInterval != 0) return;
        }
        else
        {
            // 중요 상태면 무조건 AIPath 활성화
            if (_aiPath != null && !_aiPath.enabled) _aiPath.enabled = true;
        }

        _aiBrain?.Tick(Time.deltaTime);
        _behaviorTree?.rootNode?.Evaluate();
    }

    public bool IsActionable() => _aiBrain?.IsActionable() ?? false;
    public void CombatEnter(bool combat = true) { if (_aiBrain != null) _aiBrain.CombatEnter(combat); }

    public void OnEventTrigger(string eventName)
    {
        if (_aiBrain == null || !_aiBrain._isCombat) return;
        if (eventName == "OnSwingMiss") _aiBrain.blackboard.SetValue(EnemyBlackboardKeys.OnPlayerAirshot, true);
        else if (eventName == "OnHealing") _aiBrain.blackboard.SetValue(EnemyBlackboardKeys.OnPlayerRecovery, true);
    }
}
