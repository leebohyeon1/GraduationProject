using UnityEngine;

public class CheckAttackState : StateMachineBehaviour
{
    // Sub-State Machine(육각형)에 진입할 때 1번 실행됨
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        // 예: 블랙보드나 EnemyAnimationBridge에 "공격 중"이라고 알림
        var runner = animator.GetComponent<EnemyAnimationBridge>(); // 본인의 컴포넌트 가져오기
        if (runner != null)
        {
            runner.IsAttacking = true; 
        }
    }

    // Sub-State Machine(육각형)을 완전히 빠져나갈 때 1번 실행됨
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        var runner = animator.GetComponent<EnemyAnimationBridge>();
        if (runner != null)
        {
            runner.IsAttacking = false;
        }
    }
}