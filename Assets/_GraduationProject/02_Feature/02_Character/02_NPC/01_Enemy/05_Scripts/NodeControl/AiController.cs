using BehaviorTree;
using Pathfinding;
using UnityEngine;

public class AiController : MonoBehaviour
{
    [SerializeField] private ActionTree _behaviorTree;
    public AiBrain _aiBrain { get; private set; }
    AIPath aIPath;
    Enemy _enemy;

    public void Initialize(Enemy owner)
    {
        _enemy = owner;
        _aiBrain = new AiBrain(_behaviorTree, owner);
    }
    void Update()
    {
        if(_enemy.IsDead) return;
        _aiBrain?.Tick(Time.deltaTime);
    }
    public bool IsActionable()
    {
        if (_aiBrain == null) return false;
        return _aiBrain.IsActionable();
    }
    public void CombatEnter()
    {
        _aiBrain.CombatEnter();
    }

}
