// Patrol.cs 파일
using BehaviorTree;
using UnityEngine;

[CreateAssetMenu(fileName = "Patrol", menuName = "BehaviorTree/Patrol")]
public class Patrol : Node
{
    public override void OnEnter()
    {
        runner.SetState(Enemy.EnemyState.Patrol);
        // ★ runner에게 순찰 시작을 명령합니다.
        runner.Movement.StartPatrol();
    }

    protected override NodeState OnUpdate()
    {
        // ★ runner의 순찰 로직을 실행하고 그 결과를 그대로 반환합니다.
        runner.AnimationEvent("Walk"); 
        return runner.Movement.Patrols();
    }

    public override void OnExit()
    {
        // ★ runner에게 이동 중지를 명령하여 다른 노드와의 충돌을 방지합니다.
        runner.Movement.StopMovement();
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}