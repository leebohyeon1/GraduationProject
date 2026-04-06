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
        base.SpecificCleanup();

        _isActionFinishedInternally = true;

        if (ShouldStartCooldown())
        {
            StartBossFakeAttackCooldown();
        }
        else
        {
            LogCooldownSkipReason();
        }
    }

    private bool ShouldStartCooldown()
    {
        return !_wasParriedDuringAttack;
    }

    private void StartBossFakeAttackCooldown()
    {
        brain.StartSkillCooldown(attackKey);
        Debug.Log($"[Task_BossFakeAttackNode] {attackKey} 쿨타임 시작");
    }

    private void LogCooldownSkipReason()
    {
        Debug.Log($"[Task_BossFakeAttackNode] 패리당했으므로 {attackKey} 쿨타임 시작하지 않음");
    }

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