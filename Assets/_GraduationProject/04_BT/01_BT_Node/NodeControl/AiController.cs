using BehaviorTree;
using Pathfinding;
using UnityEngine;

/// <summary>
/// 몬스터의 Behavior Tree AI를 제어하고 업데이트 빈도를 관리하는 컴포넌트입니다.
/// </summary>
public class AiController : MonoBehaviour, IEventListener<string>
{
    [SerializeField] private ActionTree _behaviorTree;
    
    /// <summary>
    /// AI의 뇌(Brain) 시스템을 참조합니다.
    /// </summary>
    public AiBrain _aiBrain { get; private set; }
    
    private Enemy _enemy;
    [SerializeField] private OnSwingMissSO _onSwingMissEvent;
    [SerializeField] private OnHealingSO _onHealingEvent;

    /// <summary>
    /// 몬스터가 가진 원본 공격 데이터 배열입니다.
    /// </summary>
    [field: SerializeField] public EnemyAttackData[] enemyAttackDatas { get; private set; }

    /// <summary>
    /// 인게임에서 복제되어 사용되는 공격 데이터 배열입니다.
    /// </summary>
    [HideInInspector] public EnemyAttackData[] inGameenemyAttackDatas { get; private set; }

    [Header("Optimization Settings")]
    [SerializeField] private float _lodDistance = 25f;
    [SerializeField] private int _tickInterval = 5;
    private int _frameCounter;

    /// <summary>
    /// AI 시스템을 초기화합니다.
    /// </summary>
    /// <param name="owner">Enemy 본체 참조</param>
    /// <param name="statMultiplier">스탯 배율 데이터</param>
    public void Initialize(Enemy owner, EnemyStatMultiplier statMultiplier = default)
    {
        _enemy = owner;
        _aiBrain = new AiBrain(owner);
        _behaviorTree = _behaviorTree.Clone();
        _behaviorTree.SetRunner(owner, _aiBrain);
        inGameenemyAttackDatas = new EnemyAttackData[enemyAttackDatas.Length];
        for (int i = 0; i < enemyAttackDatas.Length; i++)
        {
            inGameenemyAttackDatas[i] = Instantiate(enemyAttackDatas[i]);
            float c = statMultiplier != null ? statMultiplier.AttackMultiply : 1;
            inGameenemyAttackDatas[i].damageData.DamageAmount = (int)(inGameenemyAttackDatas[i].damageData.DamageAmount * c);
            Debug.Log($"[AiController] {owner.name} Attack Data {inGameenemyAttackDatas[i].name} Damage set to {inGameenemyAttackDatas[i].damageData.DamageAmount}");
            _aiBrain.AddEnemyAttackData(inGameenemyAttackDatas[i]);
        }
        _behaviorTree.rootNode?.initNode();
    }

    private void OnDisable()
    {
        _onSwingMissEvent.Unsubscribe(this);
        _onHealingEvent.Unsubscribe(this);
    }

    private void OnDestroy()
    {
        if (inGameenemyAttackDatas != null)
        {
            for (int i = 0; i < inGameenemyAttackDatas.Length; i++)
            {
                if (inGameenemyAttackDatas[i] != null)
                    Destroy(inGameenemyAttackDatas[i]);
            }
        }
    }

    private void Update()
    {
        if (_enemy.EnemyHealth.IsDead)
        {
            return;
        }

        // Optimization: LOD Check (플레이어와의 거리에 따른 업데이트 조절)
        if (_enemy.player != null)
        {
            float distSq = (transform.position - _enemy.player.transform.position).sqrMagnitude;
            if (distSq > _lodDistance * _lodDistance)
            {
                _frameCounter++;
                if (_frameCounter < _tickInterval) return;
                _frameCounter = 0;
            }
        }

        _aiBrain?.Tick(Time.deltaTime);
        _behaviorTree?.rootNode?.Evaluate();
    }

    /// <summary>
    /// 현재 AI가 행동 가능한 상태인지 확인합니다.
    /// </summary>
    /// <returns>행동 가능 여부</returns>
    public bool IsActionable()
    {
        if (_aiBrain == null) return false;
        return _aiBrain.IsActionable();
    }

    /// <summary>
    /// 전투 모드로 진입하거나 해제합니다.
    /// </summary>
    /// <param name="combat">전투 상태 여부</param>
    public void CombatEnter(bool combat = true)
    {
        if (!_aiBrain._isCombat)
            _aiBrain.CombatEnter(combat);
    }

    /// <summary>
    /// 외부 이벤트를 수신하여 블랙보드 값을 업데이트합니다.
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    public void OnEventTrigger(string eventName)
    {
        if (_aiBrain._isCombat && eventName == "OnSwingMiss")
        {
            _aiBrain.blackboard.SetValue("OnPlayerAirshot", true);
        }

        if (_aiBrain._isCombat && eventName == "OnHealing")
        {
            _aiBrain.blackboard.SetValue("OnPlayerRecovery", true);
        }
    }
}
