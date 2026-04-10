using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Task_NormalAttackNode", menuName = "BehaviorTree/Action/Task_NormalAttackNode")]
public class Task_NormalAttackNode : BaseAttackNode
{
    protected override void InitialMovementSetup() { }

    protected override void UpdateMovement() { }

    protected override bool IsMovementFinished => true;

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.animationStateName = this.animationStateName;
        node.transitionBuffer = this.transitionBuffer;
        
        node.maintainAtk = this.maintainAtk;
        node.SO = this.SO;
        node.ExceptKey = this.ExceptKey;
        node.LoopAttack = this.LoopAttack;
        node.NextBT = this.NextBT;
        node.debugMode = this.debugMode;
        node.checkRangeOnEnter = this.checkRangeOnEnter;
        node.rangeThreshold = this.rangeThreshold;
        node.ignoreYDistance = this.ignoreYDistance;
        node.allowOutOfCombat = this.allowOutOfCombat;
        return node;
    }
}
