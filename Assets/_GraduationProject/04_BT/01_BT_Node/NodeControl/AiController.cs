using BehaviorTree;
using Pathfinding;
using UnityEngine;

public class AiController : MonoBehaviour,IEventListener<string>
{
    [SerializeField] private ActionTree _behaviorTree;
    public AiBrain _aiBrain { get; private set; }
    Enemy _enemy;
    [SerializeField] private OnSwingMissSO _onSwingMissEvent;
    [SerializeField] private OnHealingSO _onHealingEvent;

    [field:SerializeField] public EnemyAttackData[] enemyAttackDatas{ get; private set; }
    [HideInInspector]public EnemyAttackData[] inGameenemyAttackDatas{ get; private set; }

    public void Initialize(Enemy owner,EnemyStatMultiplier statMultiplier = default)
    {
        _enemy = owner;
        _aiBrain = new AiBrain(owner);
        _behaviorTree = _behaviorTree.Clone();
        _behaviorTree.SetRunner(owner, _aiBrain);
        inGameenemyAttackDatas = new EnemyAttackData[enemyAttackDatas.Length];
        for(int i = 0; i < enemyAttackDatas.Length; i++)
        {
            inGameenemyAttackDatas[i] = Instantiate(enemyAttackDatas[i]);
            float c = statMultiplier != null ? statMultiplier.AttackMultiply:  1;
            inGameenemyAttackDatas[i].damageData.DamageAmount = (int)(inGameenemyAttackDatas[i].damageData.DamageAmount *c);
            Debug.Log($"[AiController] {owner.name} Attack Data {inGameenemyAttackDatas[i].name} Damage set to {inGameenemyAttackDatas[i].damageData.DamageAmount}");
            _aiBrain.AddEnemyAttackData(inGameenemyAttackDatas[i]);
        }
        _behaviorTree.rootNode?.initNode();
        
        // _onSwingMissEvent.Subscribe(this);
        // _onHealingEvent.Subscribe(this);
    }
    private void OnDisable()
    {
        _onSwingMissEvent.Unsubscribe(this);
        _onHealingEvent.Unsubscribe(this);
    }
    void OnDestroy()
    {
        for(int i = 0; i < inGameenemyAttackDatas.Length; i++)
        {
            Destroy(inGameenemyAttackDatas[i]);
        }
    }
    void Update()
    {
        if (_enemy.EnemyHealth.IsDead)
        {
            return;
        } 
            
        _aiBrain?.Tick(Time.deltaTime);
        _behaviorTree?.rootNode?.Evaluate();
    }
    public bool IsActionable()
    {
        if (_aiBrain == null) return false;
        return _aiBrain.IsActionable();
    }
    public void CombatEnter(bool combat = true)
    {
        if( !_aiBrain._isCombat )
        _aiBrain.CombatEnter(combat);
        
    }

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
