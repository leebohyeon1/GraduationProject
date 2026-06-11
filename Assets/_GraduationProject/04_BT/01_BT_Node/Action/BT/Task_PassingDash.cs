using UnityEngine;
using BehaviorTree;
using Pathfinding;

/// <summary>
/// 플레이어를 관통하는 직선 대시를 수행하되, 각 이동 지점을 A* 워커블 좌표에 붙여 높이 오차를 보정하는 공격 노드입니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_PassingDash", menuName = "BehaviorTree/Action/Task_PassingDash")]
public class Task_PassingDash : BaseAttackNode
{
    [Header("Dash Settings")]
    [Tooltip("대시 중 초당 이동 속도입니다. PassingDash 직선 이동량 계산에 사용합니다.")]
    public float dashSpeed = 15.0f;

    [Tooltip("플레이어를 지난 뒤 얼마나 더 전진할지 설정합니다. 직선 대시의 최종 목표점 계산에 사용합니다.")]
    public float extraDist = 10.0f;

    [Tooltip("직선 대시 도중 벽을 감지할 레이어입니다. Environment/Wall 같은 장애물 레이어를 넣어 런타임 충돌 종료에 사용합니다.")]
    public LayerMask obstacleMask;

    [Tooltip("목표점에 얼마나 가까워지면 도착으로 볼지 설정합니다. PassingDash 종료 판정에 사용합니다.")]
    public float arrivalThreshold = 0.5f;

    [Tooltip("이 공격을 시작할 수 있는 최대 거리입니다. BT 진입 가능 범위 판정에 사용합니다.")]
    public float maxTriggerRange = 10f;

    [Tooltip("직선 대시 후보 지점이 A* 워커블 좌표로 보정될 때 허용할 최대 수평 보정량입니다. 너무 크게 틀어지면 대시를 종료합니다.")]
    public float maxGraphSnapDistance = 1.25f;

    [Tooltip("대시 종료 시 켜줄 애니메이션 Bool 이름입니다. Animator의 IsRushing 계열 Bool을 넣어 후속 상태 전환에 사용합니다.")]
    public string Exittrigger = "IsRushing";

    private Vector3 _targetPos;
    private Vector3 _dashDirection;
    private bool _isDashing;
    private IAstarAI _astarAi;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _isDashing = false;
        _dashDirection = Vector3.zero;
        _astarAi = runner != null ? runner.aIPath : null;

        if (runner.aIPath != null)
        {
            runner.aIPath.enableRotation = false;
        }

        runner.AnimationBool(Exittrigger, false);
    }

    protected override void OnActionSOTriggered()
    {
        Vector3 startPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;
        Vector3 direction = playerPos - startPos;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.1f)
        {
            direction = runner.transform.forward;
            direction.y = 0f;
        }

        direction.Normalize();
        _dashDirection = direction;

        Vector3 rawTarget = playerPos + (direction * extraDist);
        _targetPos = runner.Movement != null
            ? runner.Movement.GetNearestWalkableDashPosition(rawTarget, startPos, maxGraphSnapDistance)
            : rawTarget;

        runner.transform.rotation = Quaternion.LookRotation(direction);
        _isDashing = true;
        Debug.Log($"[Task_PassingDash] {runner.name} dash started toward {_targetPos}.");

        if (_astarAi != null)
        {
            _astarAi.isStopped = true;
            _astarAi.canMove = false;
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isDashing)
        {
            return;
        }

        Vector3 currentPos = runner.transform.position;
        Vector3 toTarget = _targetPos - currentPos;
        Vector3 flatToTarget = toTarget;
        flatToTarget.y = 0f;
        float remainingDistance = flatToTarget.magnitude;

        if (remainingDistance <= arrivalThreshold)
        {
            CompleteDash(_targetPos, true);
            return;
        }

        Debug.DrawLine(runner.transform.position, _targetPos, Color.red);
        Debug.DrawLine(currentPos, _targetPos, Color.green);

        float moveDistance = Mathf.Min(dashSpeed * Time.deltaTime, remainingDistance);
        Vector3 desiredRawPosition = currentPos + (_dashDirection * moveDistance);
        desiredRawPosition.y = currentPos.y;

        Vector3 moveDir = _dashDirection;
        if (Physics.Raycast(currentPos + Vector3.up * 1.0f, moveDir, moveDistance + 1f, obstacleMask))
        {
            EndDash(false);
            return;
        }

        Vector3 correctedPosition = desiredRawPosition;
        if (runner.Movement != null)
        {
            correctedPosition = runner.Movement.GetNearestWalkableDashPosition(desiredRawPosition, currentPos, maxGraphSnapDistance);
            if (!runner.Movement.IsMeaningfulSafeMove(currentPos, correctedPosition))
            {
                EndDash(false);
                return;
            }
        }

        Vector3 correctedFlatOffset = correctedPosition - desiredRawPosition;
        correctedFlatOffset.y = 0f;
        if (correctedFlatOffset.magnitude > maxGraphSnapDistance)
        {
            EndDash(false);
            return;
        }

        runner.transform.position = correctedPosition;
        if (_astarAi != null)
        {
            _astarAi.Teleport(correctedPosition, false);
        }
    }

    protected override bool IsMovementFinished => !_isDashing;

    /// <summary>
    /// 도착한 좌표로 정렬한 뒤 대시 종료와 히트 성공 기록을 함께 처리합니다.
    /// </summary>
    private void CompleteDash(Vector3 finalPosition, bool recordHit)
    {
        runner.transform.position = finalPosition;
        if (_astarAi != null)
        {
            _astarAi.Teleport(finalPosition, false);
        }

        EndDash(recordHit);
    }

    /// <summary>
    /// 대시 상태를 종료하고 후속 애니메이션/블랙보드 상태를 정리합니다.
    /// </summary>
    private void EndDash(bool recordHit)
    {
        _isDashing = false;
        runner.AnimationBool(Exittrigger, true);
        Debug.Log($"[Task_PassingDash] {runner.name} dash ended. hit={recordHit}");

        if (recordHit)
        {
            brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
            brain.blackboard.SetValue(EnemyBlackboardKeys.LastAttackSuccessTime, Time.time);
        }
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = attackKey;
        node.transitionBuffer = transitionBuffer;
        node.SO = SO;
        node.LoopAttack = LoopAttack;
        node.NextBT = NextBT;
        node.debugMode = debugMode;
        node.checkRangeOnEnter = checkRangeOnEnter;
        node.rangeThreshold = rangeThreshold;
        node.ignoreYDistance = ignoreYDistance;
        node.allowOutOfCombat = allowOutOfCombat;
        node.dashSpeed = dashSpeed;
        node.extraDist = extraDist;
        node.obstacleMask = obstacleMask;
        node.arrivalThreshold = arrivalThreshold;
        node.maxTriggerRange = maxTriggerRange;
        node.maxGraphSnapDistance = maxGraphSnapDistance;
        node.Exittrigger = Exittrigger;
        node.ExceptKey = ExceptKey;
        node.escapeOnHitConfirm = escapeOnHitConfirm;
        node.hitEscapeDelay = hitEscapeDelay;
        return node;
    }
}
