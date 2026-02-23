using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_AttackRange", menuName = "BehaviorTree/Action/Task_AttackRange")]
public class Task_AttackRange : BaseAttackNode
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public Vector3 spawnOffset = new Vector3(0, 1.0f, 0.5f);
    public DamageData damageData;
    public float maxTriggerRange = 25f;

    private Vector3 _attackDir;
    private bool _hasFired;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _hasFired = false;

        Vector3 spawnPos = runner.transform.position + (runner.transform.rotation * spawnOffset);
        Vector3 targetPos = runner.player.transform.position + Vector3.up * 0.5f;
        _attackDir = (targetPos - spawnPos).normalized;
        _attackDir.y = 0;

        runner.transform.rotation = Quaternion.LookRotation(_attackDir);
        Log("원거리 공격 준비 완료. 방향: " + _attackDir);
    }

    protected override void UpdateMovement()
    {
        if (Handler.IsHitWindowOpen && !_hasFired)
        {
            Fire();
        }
    }

    protected override bool IsMovementFinished => _hasFired;

    private void Fire()
    {
        Log("원거리 투사체 발사");
        _hasFired = true;
        Vector3 spawnPos = runner.transform.position + (runner.transform.rotation * spawnOffset);

        if (projectilePrefab != null)
        {
            GameObject bulletObj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(_attackDir));
            if (bulletObj.TryGetComponent<EnemyProjectile>(out var projectileScript))
            {
                projectileScript.Setup(_attackDir, projectileSpeed, runner.gameObject, damageData);
                brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
            }
        }
        Handler.CloseHitWindow();
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.animationStateName = this.animationStateName;
        node.transitionBuffer = this.transitionBuffer;
        node.continuousRotation = this.continuousRotation;
        node.maintainAtk = this.maintainAtk;
        node.SO = this.SO;
        node.LoopAttack = this.LoopAttack;
        node.NextBT = this.NextBT;
        node.debugMode = this.debugMode;
        node.checkRangeOnEnter = this.checkRangeOnEnter;
        node.rangeThreshold = this.rangeThreshold;
        node.projectilePrefab = this.projectilePrefab;
        node.projectileSpeed = this.projectileSpeed;
        node.spawnOffset = this.spawnOffset;
        node.damageData = this.damageData;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        return node;
    }
}
