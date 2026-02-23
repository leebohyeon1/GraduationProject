using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_RushAttack", menuName = "BehaviorTree/Action/Task_RushAttack")]
public class Task_RushAttack : BaseAttackNode
{
    [Header("Rush Settings")]
    public float rushSpeed = 15f;
    public float stopDistance = 1.0f;
    public float speed = 6f;
    public float maxTriggerRange = 15f;

    private bool _endStrategy;
    private bool _isRushing;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _endStrategy = false;
        _isRushing = false;
        Log("돌진 공격 준비 완료 (ActionSO 대기)");
    }

    protected override void OnActionSOTriggered()
    {
        Log("돌진 공격 시작 (OnActionSOTriggered)");
        _isRushing = true;
        
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.isStopped = false;
            ai.canMove = true;
            if (ai is AIPath aiPath)
            {
                aiPath.maxSpeed = rushSpeed;
                aiPath.enableRotation = false;
            }
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isRushing || runner.player == null) return;

        float dist = Vector3.Distance(runner.transform.position, runner.player.transform.position);

        if (dist > stopDistance && !Handler.IsHitWindowOpen && !_endStrategy)
        {
            runner.Movement.StartRush(runner.player.transform.position, speed);
        }
        else
        {
            Log("목표 거리 도달 혹은 히트 윈도우 오픈으로 돌진 중단");
            runner.Movement.StopMovement();
            _endStrategy = true;
        }
    }

    protected override bool IsMovementFinished => _endStrategy;

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.animationStateName = this.animationStateName;
        node.transitionBuffer = this.transitionBuffer;
        node.continuousRotation = this.continuousRotation;
        node.maintainAtk = this.maintainAtk;
        node.SO = this.SO;
        node.LoopAttack = this.LoopAttack;
        node.NextBT = this.NextBT;
        node.debugMode = this.debugMode;
        node.checkRangeOnEnter = this.checkRangeOnEnter;
        node.rangeThreshold = this.rangeThreshold;
        node.rushSpeed = this.rushSpeed;
        node.stopDistance = this.stopDistance;
        node.speed = this.speed;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        return node;
    }
}
