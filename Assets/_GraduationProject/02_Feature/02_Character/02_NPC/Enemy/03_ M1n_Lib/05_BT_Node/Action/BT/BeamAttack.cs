using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "BeamAttack", menuName = "BehaviorTree/Action/BeamAttack")]
public class BeamAttack : Node
{
    [Header("Phase Timings")]
    [Tooltip("준비(조준) 시간입니다.")]
    public float prepDuration = 1.2f;
    [Tooltip("발사 직전, 조준이 고정되는 시간입니다.")]
    public float aimLockTime = 0.25f;
    [Tooltip("빔 발사(공격) 지속 시간입니다.")]
    public float activeDuration = 0.8f;
    [Tooltip("공격 후 회복(후딜레이) 시간입니다.")]
    public float recoverDuration = 1.0f;
    
    [Tooltip("빔의 공격 판정 두께(반지름)입니다.")]
    public float beamWidth = 0.5f;

    // 노드 내부의 상태를 관리할 enum
    private enum Phase { Prep, Active, Recover }

    // 각 단계의 진행 시간을 추적할 타이머

    public float beamLength = 50f;
    public override void OnEnter()
    {
        runner.Movement.StopMovement();
        runner.SetState(Enemy.EnemyState.Beam); // 방해 불가 상태로 설정
        runner.AnimationEvent("Do_Beam");         // 빔 공격 애니메이션 트리거
        
        // Enemy에게 "조준 경고 이펙트 시작" 명령 (기획서의 Evt_SBWarn)
        // runner.ToggleBeamWarning(true, beamLength);
    }

    protected override NodeState OnUpdate()
    {

        if (runner.IsHitWindowOpen)
        {
            // runner.StartBeamAttack(activeDuration, beamLength, beamWidth);
        }
        else if (!runner.IsHitWindowOpen)
        {
            // runner.UpdateAimingAtPlayer();
                // runner.StopBeamAttack();
        }
        // if(!runner.IsHitWindowOpen)
            // {
            // }
            if (runner.IsActionFinished)
            {
                return NodeState.SUCCESS;
            }
        // switch (_currentPhase)
        // {
        //     case Phase.Prep:
        //         // 조준이 고정되기 전까지는 계속 플레이어를 추적
        //         if (_phaseTimer < prepDuration - aimLockTime)
        //         {
        //             runner.UpdateAimingAtPlayer(); // 플레이어 조준해
        //         }

        //         // 준비 시간이 다 되면 '발사' 상태로 전환
        //         if (_phaseTimer >= prepDuration)
        //         {
        //             _currentPhase = Phase.Active;
        //             _phaseTimer = 0f;
        //             runner.StartBeamAttack(activeDuration, beamLength, beamWidth);
        //         }
        //         break;

        //     case Phase.Active:
        //         // 발사 시간이 다 되면 '회복' 상태로 전환
        //         if (_phaseTimer >= activeDuration)
        //         {
        //             _currentPhase = Phase.Recover;
        //             _phaseTimer = 0f;
        //             // Enemy에게 "빔 발사 종료" 명령 (기획서의 Evt_SBEnd)
        //             runner.StopBeamAttack();
        //         }
        //         break;

        //     case Phase.Recover:
        //         // 회복 시간이 다 되면 노드 전체를 성공으로 종료
        //         if (_phaseTimer >= recoverDuration)
        //         {
        //             return NodeState.SUCCESS;
        //         }
        //         break;
        // }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        // runner.ToggleBeamWarning(false, beamLength);
        // runner.StopBeamAttack();
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.prepDuration = this.prepDuration;
        node.aimLockTime = this.aimLockTime;
        node.activeDuration = this.activeDuration;
        node.beamLength = this.beamLength;
        node.recoverDuration = this.recoverDuration;
        node.beamWidth = this.beamWidth;
        return node;
    }
}