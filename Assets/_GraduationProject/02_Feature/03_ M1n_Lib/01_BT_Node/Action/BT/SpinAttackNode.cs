using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "SpinAttackNode", menuName = "BehaviorTree/SpinAttackNode")]
public class SpinAttackNode : Node
{
    [Tooltip("공격의 유효 반경입니다.")]
    public float damageRadius = 3f;
    [Tooltip("초당 데미지 또는 총 데미지입니다.")]
    public int damage = 15;
    [Tooltip("공격이 지속되는 시간입니다.")]
    public float AtkDuration;

    private float _attackTimer = 0f;

    public override void OnEnter()
    {
        _attackTimer = 0f; // ★ 타이머 초기화는 OnEnter에서 하는 것이 안전합니다.
        runner.SetState(Enemy.EnemyState.Attack); // 스핀 공격도 공격 상태의 일종으로 설정
    }

    protected override NodeState OnUpdate()
    {
        _attackTimer += Time.deltaTime;

        // 공격 지속 시간 체크
        if (_attackTimer >= AtkDuration)
        {
            return NodeState.SUCCESS; // 공격 시간이 끝나면 성공
        }

        // ★ Enemy.cs의 중앙 제어 함수를 통해 이동
        runner.Movement.StartOrUpdateChase(runner.player.transform);
        runner.AnimationEvent("SpinAttack");

        // 데미지 판정 로직 (주기적으로 데미지를 주도록 수정 가능)
        // 예: 1초에 한 번씩 데미지 등... 현재는 매 프레임 데미지를 줍니다.
        Collider[] hitColliders = Physics.OverlapSphere(runner.transform.position, damageRadius);
        foreach (Collider col in hitColliders)
        {
            if (col.TryGetComponent<IDamageable>(out IDamageable player))
            {
                player.TakeDamage(damage);
            }
        }

        // 공격이 아직 진행 중이므로 RUNNING
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        _attackTimer = 0f;
        // ★★★ 가장 중요한 수정: 노드가 종료될 때 반드시 이동을 멈춥니다.
        runner.Movement.StopMovement();
    }

    public override Node Clone()
    {
        // 모든 인스펙터 값을 복사하도록 수정
        SpinAttackNode clone = Instantiate(this);
        clone.damageRadius = this.damageRadius;
        clone.damage = this.damage;
        clone.AtkDuration = this.AtkDuration;
        return clone;
    }
}