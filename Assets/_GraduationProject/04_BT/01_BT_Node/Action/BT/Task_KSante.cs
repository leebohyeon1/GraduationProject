using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using System.Collections;
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
        Vector3 playerPos = runner.player.transform.position;
        Vector3 myPos = runner.transform.position;
        Vector3 dir = (playerPos - myPos);
        dir.y = 0;
        if (dir == Vector3.zero) dir = runner.transform.forward;
        dir.Normalize();

        Vector3 offset = Quaternion.Euler(0, Random.Range(0, 360), 0) * new Vector3(0.5f, 0, 0);
        _targetPos = playerPos + (dir * overshootDist) + offset;
        
        _hasHitPlayer = false;
        _isRushing = false;
        _rushStartTime = 0;

        runner.aIPath.enableRotation = false;
        Log("KSante 목표 설정 완료: " + _targetPos);
    }

    protected override void OnActionSOTriggered()
    {
        Log("KSante 돌진 시작 (OnActionSOTriggered)");
        _isRushing = true;
        _rushStartTime = Time.time;
        
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.isStopped = true;
            ai.canMove = false;
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isRushing) return;

        float rushElapsedTime = Time.time - _rushStartTime;
        float normalizedTime = rushElapsedTime / rushDuration;

        if (normalizedTime >= 1.0f)
        {
            Log("KSante 돌진 시간 만료");
            _isRushing = false;
            return;
        }

        float step = rushSpeed * Time.deltaTime;
        Vector3 currentPos = runner.transform.position;
        Vector3 nextPos = Vector3.MoveTowards(currentPos, _targetPos, step);
        Vector3 moveDir = (nextPos - currentPos).normalized;
        moveDir.y = 0;

        float moveDist = Vector3.Distance(currentPos, nextPos);

        if (moveDist > 0.0001f)
        {
            if (!Physics.Raycast(currentPos + Vector3.up * 0.5f, moveDir, moveDist + 1f, obstacleMask))
            {
                runner.transform.position = nextPos;
            }
            else
            {
                Log("KSante 장애물 충돌");
                _isRushing = false;
                return;
            }
        }

        if (!_hasHitPlayer)
        {
            float distToPlayer = Vector3.Distance(runner.transform.position, runner.player.transform.position);
            if (distToPlayer <= hitRadius)
            {
                Log("KSante 플레이어 포착 - 드래그 시작");
                PlayerTORush();
            }
        }

        if (Vector3.Distance(runner.transform.position, _targetPos) < 0.1f)
        {
            Log("KSante 목표 지점 도달");
            _isRushing = false;
        }
    }

    protected override bool IsMovementFinished => !_isRushing;

    private void PlayerTORush()
    {
        _hasHitPlayer = true;
        brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
        
        if (runner.player.TryGetComponent<IDragable>(out var dragable))
        {
            dragable.Drag();
        }

        runner.player.transform.parent = runner.transform;

        Vector3 currentPos = runner.transform.position;
        Vector3 pushDir = runner.transform.forward;
        Vector3 newDestination = currentPos + (pushDir * PushDistance);

        RaycastHit hit;
        if (Physics.Raycast(currentPos + Vector3.up * 0.5f, pushDir, out hit, PushDistance, obstacleMask))
        {
            float targetDist = Mathf.Max(0, hit.distance - 2f);
            newDestination = currentPos + (pushDir * targetDist);
        }

        _targetPos = newDestination;
        _rushStartTime = Time.time;
    }

    protected override void SpecificCleanup()
    {
        if (_hasHitPlayer)
        {
            Log("KSante 플레이어 드랍 및 넉백 적용");
            runner.player.transform.parent = null;
            AttackDataKnockback.AttackerTransform = runner.transform;
            
            if (runner.player.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(AttackDataKnockback);
            }

            if (runner.player.TryGetComponent<IDragable>(out var dragable))
            {
                dragable.Drop();
            }

            runner.player.Movement.Step(runner.transform.forward, new StepData()
            {
                StepDistance = AttackDataKnockback.KnockbackForce * AttackDataKnockback.KnockbackDuration,
                StepDuration = AttackDataKnockback.KnockbackDuration,
                StepCurve = AttackDataKnockback.KnockbackCurve,
                StepRotateSpeed = 0f
            }, this, false, null);
        }
    }

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
        node.hitRadius = this.hitRadius;
        node.overshootDist = this.overshootDist;
        node.obstacleMask = this.obstacleMask;
        node.rushDuration = this.rushDuration;
        node.PushDistance = this.PushDistance;
        node.AttackDataKnockback = this.AttackDataKnockback;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        return node;
    }
}
