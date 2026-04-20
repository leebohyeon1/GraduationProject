// RushToPlayer.cs ?뚯씪
using UnityEngine;
using BehaviorTree;

public class RushToPlayer : Node
{
    [SerializeField] float _rushSpeed = 20f;
    [SerializeField] float _successDistance = 1.5f;
    [SerializeField] float _timeout = 5.0f;

    private float _startTime;
    private Vector3 _targetPosition;

    public override void OnEnter()
    {
        // _startTime = Time.time;
        
        _targetPosition = runner.player.transform.position;
        _targetPosition.y = runner.transform.position.y;
        
        runner.SetState(EnemyStateController.EnemyState.Rush);
        runner.Movement.StartRush(_targetPosition, _rushSpeed);
    }

    protected override NodeState OnUpdate()
    {
        // OnUpdate??留??꾨젅???몄텧?섎?濡?濡쒓렇???꾩슂 ?쒖뿉留??쒖꽦??

        // if (Time.time - _startTime > _timeout)
        // {
        //     Debug.LogError("<color=red>--RUSH--: TIMEOUT! AI did not reach the target in time.</color>");
        //     return NodeState.FAILURE;
        // }

        float distanceToTarget = Vector3.Distance(runner.transform.position, _targetPosition);
        if (distanceToTarget <= _successDistance)
        {
            return NodeState.SUCCESS;
        }
        
        return NodeState.RUNNING;
    }

    public override void OnExit()
{

    // StopMovement()瑜??몄텧?섍린 ?꾩뿉, ?먯떊???곹깭瑜?癒쇱? 蹂寃쏀븯??
    // StopMovement()??蹂댄샇 濡쒖쭅???뺤긽?곸쑝濡??듦낵?????덇쾶 ?⑸땲??
    if (runner.CurrentState == EnemyStateController.EnemyState.Rush)
    {
        runner.SetState(EnemyStateController.EnemyState.Idle);
    }
    // ?끸쁾???ш린源뚯? 異붽? ?끸쁾??

    runner.Movement.StopMovement();
}
    
    public override Node Clone()
    {
        RushToPlayer clone = Instantiate(this);
        clone._rushSpeed = _rushSpeed;
        clone._successDistance = _successDistance;
        clone._timeout = _timeout;
        return clone;
    }
}
