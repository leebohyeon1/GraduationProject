using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Task_TripleRushEffect", menuName = "BehaviorTree/Action/Task_TripleRushEffect")]
public class Task_TripleRushEffect : BaseAttackNode
{
    [Header("Rush Settings")]
    public float maxDashDist = 2.0f;
    public float dashSpeed = 10.0f;
    public float leapDistance = 4.0f;
    public float leapDuration = 0.7f;
    public AnimationCurve leapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private int _rushIndex;
    private bool _isMoving;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _startTime;
    private float _duration;

    protected override void InitialMovementSetup()
    {
        _rushIndex = 0;
        _isMoving = false;
        _duration = 0f;
    }

    protected override void OnActionSOTriggered()
    {
        _rushIndex++;
        if (_rushIndex > 3)
        {
            _isMoving = false;
            return;
        }

        SetupNextRush(_rushIndex);
    }

    protected override void UpdateMovement()
    {
        if (!_isMoving)
        {
            return;
        }

        float duration = Mathf.Max(0.001f, _duration);
        float progress = (Time.time - _startTime) / duration;

        if (progress >= 1.0f)
        {
            runner.transform.position = _targetPos;
            _isMoving = false;
            return;
        }

        Vector3 dir = (_targetPos - _startPos).normalized;
        if (dir != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.Slerp(
                runner.transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 15f);
        }

        if (_rushIndex == 3)
        {
            float curveValue = leapCurve.Evaluate(Mathf.Clamp01(progress));
            runner.transform.position = Vector3.Lerp(_startPos, _targetPos, curveValue);
        }
        else
        {
            runner.transform.position = Vector3.Lerp(_startPos, _targetPos, Mathf.Clamp01(progress));
        }
    }

    protected override bool IsMovementFinished => !_isMoving;

    protected override void SpecificCleanup()
    {
        _isMoving = false;
        _rushIndex = 0;
        base.SpecificCleanup();
    }

    private void SetupNextRush(int index)
    {
        Vector3 currentPos = runner.transform.position;
        Vector3 playerPos = runner.player != null
            ? runner.player.transform.position
            : (currentPos + runner.transform.forward);

        Vector3 dirToPlayer = playerPos - currentPos;
        dirToPlayer.y = 0;

        float distToPlayer = dirToPlayer.magnitude;
        if (distToPlayer > 0.001f)
        {
            dirToPlayer /= distToPlayer;
        }
        else
        {
            dirToPlayer = runner.transform.forward;
        }

        if (index == 1 || index == 2)
        {
            float moveDist = Mathf.Min(maxDashDist, distToPlayer);
            _targetPos = currentPos + (dirToPlayer * moveDist);
            _duration = Mathf.Max(0.1f, moveDist / Mathf.Max(0.01f, dashSpeed));
        }
        else
        {
            _targetPos = currentPos + (dirToPlayer * leapDistance);
            _duration = Mathf.Max(0.1f, leapDuration);
        }

        _startPos = currentPos;
        _startTime = Time.time;
        _isMoving = true;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = attackKey;
        node.animationStateName = animationStateName;
        node.transitionBuffer = transitionBuffer;
        node.SO = SO;
        node.LoopAttack = LoopAttack;
        node.NextBT = NextBT;
        node.debugMode = debugMode;
        node.checkRangeOnEnter = checkRangeOnEnter;
        node.rangeThreshold = rangeThreshold;
        node.ignoreYDistance = ignoreYDistance;
        node.allowOutOfCombat = allowOutOfCombat;

        node.ExceptKey = ExceptKey;
        node.escapeOnHitConfirm = escapeOnHitConfirm;
        node.hitEscapeDelay = hitEscapeDelay;

        node.maxDashDist = maxDashDist;
        node.dashSpeed = dashSpeed;
        node.leapDistance = leapDistance;
        node.leapDuration = leapDuration;
        node.leapCurve = leapCurve;
        return node;
    }
}
