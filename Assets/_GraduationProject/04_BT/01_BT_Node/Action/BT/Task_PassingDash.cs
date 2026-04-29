using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_PassingDash", menuName = "BehaviorTree/Action/Task_PassingDash")]
public class Task_PassingDash : BaseAttackNode
{
    [Header("Dash Settings")]
    public float dashSpeed = 15.0f;
    public float extraDist = 10.0f;
    public LayerMask obstacleMask;
    public float arrivalThreshold = 0.5f;
    public float maxTriggerRange = 10f;

    private Vector3 _targetPos;
    private bool _isDashing;
    public string Exittrigger = "IsRushing";
    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _isDashing = false;
        runner.aIPath.enableRotation = false;
        runner.AnimationBool(Exittrigger, false);
    }

    protected override void OnActionSOTriggered()
    {
        // [수정] 애니메이션 이벤트 시점에 실시간 플레이어 위치를 기반으로 목표 지점 계산
        Vector3 startPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;
        Vector3 direction = (playerPos - startPos);
        direction.y = 0;

        if (direction.sqrMagnitude < 0.1f) direction = runner.transform.forward;
        else direction.Normalize();

        _targetPos = playerPos + (direction * extraDist);
        _targetPos.y = startPos.y;

        runner.transform.rotation = Quaternion.LookRotation(direction);
        
        _isDashing = true;

        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.isStopped = true;
            ai.canMove = false;
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isDashing) return;

        Vector3 currentPos = runner.transform.position;
        Vector3 moveDir = (_targetPos - currentPos);
        moveDir.y = 0;
        float distToTarget = moveDir.magnitude;

        if (distToTarget <= arrivalThreshold)
        {
            runner.transform.position = _targetPos;
            _isDashing = false;
            runner.AnimationBool(Exittrigger, true);
                        brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                        brain.blackboard.SetValue(EnemyBlackboardKeys.LastAttackSuccessTime, Time.time);
            return;
        }
        // ---------------------------------------------------------
        // [시각적 디버깅] Scene 뷰에서 확인하세요!
        // 빨간 선: 시작점 -> 목표점 (전체 경로)
        Debug.DrawLine(runner.transform.position, _targetPos, Color.red);
        // 초록 선: 내 위치 -> 목표점 (남은 경로)
        Debug.DrawLine(currentPos, _targetPos, Color.green);
        // ---------------------------------------------------------
        moveDir.Normalize();
        float moveDistance = dashSpeed * Time.deltaTime;

        if (Physics.Raycast(currentPos + Vector3.up * 1.0f, moveDir, moveDistance + 1f, obstacleMask))
        {
            _isDashing = false;
            runner.AnimationBool(Exittrigger, true);
            return;
        }

        runner.transform.position += moveDir * moveDistance;
    }

    protected override bool IsMovementFinished => !_isDashing;

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
        node.dashSpeed = this.dashSpeed;
        node.extraDist = this.extraDist;
        node.obstacleMask = this.obstacleMask;
        node.arrivalThreshold = this.arrivalThreshold;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        node.escapeOnHitConfirm = this.escapeOnHitConfirm;
        node.hitEscapeDelay = this.hitEscapeDelay;
        return node;
    }
}
