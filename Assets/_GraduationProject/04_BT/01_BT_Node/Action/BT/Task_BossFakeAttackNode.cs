using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Task_BossFakeAttackNode", menuName = "BehaviorTree/Action/Task_BossFakeAttackNode")]
public class Task_BossFakeAttackNode : BaseAttackNode
{
    protected override void InitialMovementSetup() { }

    protected override void UpdateMovement() { }

    protected override bool IsMovementFinished => true;

    protected override void SpecificCleanup()
    {
        base.SpecificCleanup(); // BaseAttackNode??怨듯넻 ?⑤━ 李⑤떒 濡쒖쭅 ?ㅽ뻾

        _isActionFinishedInternally = true;

        LogParryDebugInfo();
    }

    private void LogParryDebugInfo()
    {
        bool isStunned = runner.ParrySystem != null && runner.ParrySystem._isStunned;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.animationStateName = this.animationStateName;
        node.transitionBuffer = this.transitionBuffer;
        
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
