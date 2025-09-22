using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "NoiseActionNode", menuName = "BehaviorTree/NoiseActionNode")]
public class NoiseActionNode : Node
{
    private bool _hasCalled;
    public float Noise_distance;
    public AudioClip EnemyCallingSoundClip;
    public override void OnEnter()
    {
        runner.SetState(Enemy.EnemyState.Noise);
        runner.Movement.StopMovement();
        runner.AnimationEvent("Noise"); // 애니메이션 이름은 정확히 맞춰주세요
    }
    
    protected override NodeState OnUpdate()
    {
        if (_hasCalled)
        {
            return NodeState.SUCCESS;
        }

        AnimatorStateInfo stateInfo = runner.GetAnimator().GetCurrentAnimatorStateInfo(0);
        // IsName의 인자는 애니메이션 상태(State)의 이름이어야 합니다.
        if (stateInfo.IsName("Noise") && stateInfo.normalizedTime >= 0.85f)
        {
            // runner.soundManager.PlaySFXAtPosition(runner.EnemyCallingSoundClip, runner.transform.position);

            Collider[] hitColliders = Physics.OverlapSphere(runner.transform.position, Noise_distance);
            foreach (Collider col in hitColliders)
            {
                if (col.TryGetComponent<Enemy>(out Enemy enemy) && enemy != runner)
                {
                    brain.CombatEnter();
                }
            }
            _hasCalled = true;
            brain.CombatEnter(); // 전투 상태로 전환
            return NodeState.SUCCESS;
        }
        
        return NodeState.RUNNING;
    }

    public override void initNode()
    {
        base.initNode();
        _hasCalled = false;
    }
    
    public override Node Clone()
    {
        return Instantiate(this);
    }
}