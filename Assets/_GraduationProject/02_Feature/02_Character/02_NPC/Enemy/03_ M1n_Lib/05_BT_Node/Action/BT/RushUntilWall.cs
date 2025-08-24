using UnityEngine;
using BehaviorTree;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "RushUntilWall", menuName = "BehaviorTree/Action/RushUntilWall")]
public class RushUntilWall : Node
{
    [Tooltip("돌진 속도입니다.")]
    public float rushSpeed = 25f;
    [Tooltip("벽을 감지할 거리입니다. (보스 크기보다 약간 크게)")]
    public float wallDetectionDistance = 1.5f;
    [Tooltip("최대 돌진 시간입니다. (안전장치)")]
    public float timeout = 4.0f;
    private float _startTime;
    bool _isRushing = false;
    public override void OnEnter()
    {
        runner.SetState(Enemy.EnemyState.Rush); // 상태를 Rush로 설정
        runner.AnimationEvent("Do_Rush");
        _startTime = Time.time;
    }

    protected override NodeState OnUpdate()
    {

        if (runner.IsHitWindowOpen)
        {
            Debug.Log("<color=magenta>--WALL RUSH--: OnEnter</color>");
            _startTime = Time.time;
            runner.Movement.StartWallRush(rushSpeed);
            runner.GetComponent<Animator>().SetBool("Rush_Running", true);
            runner.AnimationEvent_CloseHitWindow();
            _isRushing = true;
        }
        if (Time.time - _startTime > timeout)
        {
            Debug.LogWarning("--RUSH--: Timeout!");
            // 타임아웃도 '돌진 종료'이므로 SUCCESS를 반환하여 다음 판단으로 넘깁니다.
            runner.GetComponent<Animator>().SetBool("Rush_Running", false);   
            return NodeState.SUCCESS;
        }

        if (_isRushing && runner.GetLastRushHitObject() != null)
        {
            Debug.Log($"--RUSH--: Collision confirmed with {runner.GetLastRushHitObject().name}. SUCCESS.");
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        _isRushing = false;
        runner.Movement.StopMovement();
    }

    public override Node Clone()
    {
        RushUntilWall node = Instantiate(this);
        node.rushSpeed = this.rushSpeed;
        node.wallDetectionDistance = this.wallDetectionDistance;
        node.timeout = this.timeout;
        return node;
    }
}