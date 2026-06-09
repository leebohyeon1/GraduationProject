using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_KSante", menuName = "BehaviorTree/Action/Task_KSante")]
public class Task_KSante : BaseAttackNode
{
    [Header("KSante Settings")]
    public float rushSpeed = 20f;
    public float hitRadius = 1.5f;
    public float overshootDist = 3.0f;
    public LayerMask obstacleMask;
    public float rushDuration = 1.0f;
    public float PushDistance = 5.0f;
    public DamageData AttackDataKnockback;
    public float maxTriggerRange = 10f;

    private Vector3 _targetPos;
    private bool _hasHitPlayer;
    private float _rushStartTime;
    private bool _isRushing;

    

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _hasHitPlayer = false;
        _isRushing = false;
        _rushStartTime = 0;
        runner.aIPath.enableRotation = false;
    }

    protected override void OnActionSOTriggered()
    {
        // [수정] 애니메이션 이벤트 시점에 실시간 플레이어 위치를 기반으로 목표 지점 계산
        Vector3 playerPos = runner.player.transform.position;
        Vector3 myPos = runner.transform.position;
        Vector3 dir = (playerPos - myPos);
        dir.y = 0;
        if (dir == Vector3.zero) dir = runner.transform.forward;
        dir.Normalize();

        Vector3 offset = Quaternion.Euler(0, Random.Range(0, 360), 0) * new Vector3(0.5f, 0, 0);
        _targetPos = playerPos + (dir * overshootDist) + offset;

        _isRushing = false;
        _rushStartTime = Time.time;
        runner.AnimationBool("IsRushing", _isRushing);
        
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.isStopped = true;
            ai.canMove = false;
        }
         runner.Movement.StartOrUpdateChase(_targetPos, EnemyStateController.EnemyState.Attack, rushSpeed);

    }

    protected override void UpdateMovement()
    {
        if (_isRushing) return;

        float rushElapsedTime = Time.time - _rushStartTime;
        float normalizedTime = rushElapsedTime / rushDuration;

        if (normalizedTime >= 1.0f)
        {
            StopRush();
            return;
        }



        if (!_hasHitPlayer)
        {
            float distToPlayer = Vector3.Distance(runner.transform.position, runner.player.transform.position);
            if (distToPlayer <= hitRadius)
            {
                PlayerTORush();
            }
        }

        if (Vector3.Distance(runner.transform.position, _targetPos) < 0.1f)
        {
            StopRush();
            return;
        }
    }

    protected override bool IsMovementFinished => _isRushing;

    private void PlayerTORush()
    {
        _hasHitPlayer = true;
                brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                brain.blackboard.SetValue(EnemyBlackboardKeys.LastAttackSuccessTime, Time.time);
        

        KsanteKnockback();


    }
    private void StopRush()
    {
        runner.Movement.StopMovement();
        _isRushing = true;
        runner.AnimationBool("IsRushing", _isRushing);
        brain.blackboard.SetValue(LoopAction.EndKey, true);

    }
    

    private void KsanteKnockback()
    {
        StopRush();
        if (runner.player.TryGetComponent<IDamageable>(out var damageable))
        {
            AttackDataKnockback.AttackerTransform = runner.transform;
            damageable.TakeDamage(AttackDataKnockback);
        }


        // runner.player.Movement.Step(runner.transform.forward, new StepData()
        // {
        //     StepDistance = AttackDataKnockback.KnockbackForce * AttackDataKnockback.KnockbackDuration,
        //     StepDuration = AttackDataKnockback.KnockbackDuration,
        //     StepCurve = AttackDataKnockback.KnockbackCurve,
        //     StepRotateSpeed = 0f
        // }, this, false, null);
        

    }

    protected override void SpecificCleanup()
    {
        StopRush();
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.transitionBuffer = this.transitionBuffer;
        node.SO = this.SO;
        node.LoopAttack = this.LoopAttack;
        node.NextBT = this.NextBT;
        node.debugMode = this.debugMode;
        node.checkRangeOnEnter = this.checkRangeOnEnter;
        node.rangeThreshold = this.rangeThreshold;
        node.ignoreYDistance = this.ignoreYDistance;
        node.allowOutOfCombat = this.allowOutOfCombat;
        node.rushSpeed = this.rushSpeed;
        node.hitRadius = this.hitRadius;
        node.overshootDist = this.overshootDist;
        node.obstacleMask = this.obstacleMask;
        node.rushDuration = this.rushDuration;
        node.PushDistance = this.PushDistance;
        node.AttackDataKnockback = this.AttackDataKnockback;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        node.escapeOnHitConfirm = this.escapeOnHitConfirm;
        node.hitEscapeDelay = this.hitEscapeDelay;
        return node;
    }
}
