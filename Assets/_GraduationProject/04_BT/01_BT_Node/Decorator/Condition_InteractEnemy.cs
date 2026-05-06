
using BehaviorTree;
using UnityEngine;

public class Condition_InteractEnemy : ConditionNode
{
    private EnemyInteract interactComponent;
    public override void initNode()
    {
        base.initNode();
        interactComponent = runner?.GetComponent<EnemyInteract>();
        Debug.Log($"[Condition_InteractEnemy] Initialized with EnemyInteract component: {interactComponent != null}");
    }
    protected override bool CheckCondition()
    {
        bool isInteracted = interactComponent != null && interactComponent._isInteracted;   
        Debug.Log($"[Condition_InteractEnemy] Checking interaction condition: Is Interacted: {isInteracted}");
        // runner의 EnemyInteract 컴포넌트를 확인하여 상호작용 여부를 판단합니다.
        return isInteracted;
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}