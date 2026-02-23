using UnityEngine;

public class CheckAttackState : StateMachineBehaviour
{
    // Sub-State Machine(육각형)에 진입할 때 1번 실행됨
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        var runner = animator.GetComponent<EnemyAnimationBridge>(); // 본인의 컴포넌트 가져오기
        if (runner != null)
        {
            Debug.Log("<color=red>[Animation] 공격 애니메이션 진입 - IsAttacking true</color>");
            runner.IsAttacking = true; 
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var runner = animator.GetComponent<EnemyAnimationBridge>();
        if (runner != null)
        {
            runner.IsAttacking = false;
        }
    }


}