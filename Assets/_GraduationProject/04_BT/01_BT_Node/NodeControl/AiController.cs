using BehaviorTree;
using Pathfinding;
using UnityEngine;

public class AiController : MonoBehaviour,IEventListener<string>
{
    [SerializeField] private ActionTree _behaviorTree;
    public AiBrain _aiBrain { get; private set; }
    Enemy _enemy;
    [SerializeField] private float MaxTargetRange = 10f;
    [SerializeField] private OnSwingMiss _onSwingMissEvent;
    [SerializeField] private OnHealing _onHealingEvent;
    public void Initialize(Enemy owner)
    {
        _enemy = owner;
        _aiBrain = new AiBrain(owner);
        _behaviorTree = _behaviorTree.Clone();
        _behaviorTree.SetRunner(owner, _aiBrain);
        _behaviorTree.rootNode?.initNode();
        // _onSwingMissEvent.Subscribe(this);
        // _onHealingEvent.Subscribe(this);
    }
    private void OnDisable()
    {
        _onSwingMissEvent.Unsubscribe(this);
        _onHealingEvent.Unsubscribe(this);
    }
    void Update()
    {
        if (_enemy.EnemyHealth.IsDead)
        {
            Debug.Log("EnemyHealth:" + _enemy.EnemyHealth.Health);
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
