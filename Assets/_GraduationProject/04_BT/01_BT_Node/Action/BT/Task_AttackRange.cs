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
    }

    protected override void OnActionSOTriggered()
    {
        // [수정] 애니메이션 이벤트 시점에 실시간 플레이어 위치를 기반으로 발사 방향 계산
        Vector3 spawnPos = runner.transform.position + (runner.transform.rotation * spawnOffset);
        Vector3 targetPos = runner.player.transform.position + Vector3.up * 0.5f;
        _attackDir = (targetPos - spawnPos).normalized;
        _hasFired = false;

        runner.transform.rotation = Quaternion.LookRotation(_attackDir);
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
        _hasFired = true;
        Vector3 spawnPos = runner.transform.position + (runner.transform.rotation * spawnOffset);

        if (projectilePrefab != null)
        {
            GameObject bulletObj = ProjectilePoolManager.GetProjectile(projectilePrefab, spawnPos, Quaternion.LookRotation(_attackDir));
            if (bulletObj.TryGetComponent<EnemyProjectile>(out var projectileScript))
            {
                projectileScript.Setup(runner, _attackDir, projectileSpeed, runner.gameObject, damageData);
            }
            else
            {
                ProjectilePoolManager.ReleaseProjectile(bulletObj);
            }
        }
        Handler.CloseHitWindow();
    }
    protected override void SpecificCleanup()
    {
        base.SpecificCleanup();
        _hasFired = false;
    }
    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.animationStateName = this.animationStateName;
        node.transitionBuffer = this.transitionBuffer;
        node.SO = this.SO;
        node.LoopAttack = this.LoopAttack;
        node.NextBT = this.NextBT;
        node.debugMode = this.debugMode;
        node.checkRangeOnEnter = this.checkRangeOnEnter;
        node.rangeThreshold = this.rangeThreshold;
        node.ignoreYDistance = this.ignoreYDistance;
        node.allowOutOfCombat = this.allowOutOfCombat;
        node.projectilePrefab = this.projectilePrefab;
        node.projectileSpeed = this.projectileSpeed;
        node.spawnOffset = this.spawnOffset;
        node.damageData = this.damageData;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        node.escapeOnHitConfirm = this.escapeOnHitConfirm;
        node.hitEscapeDelay = this.hitEscapeDelay;
        return node;
    }
}
