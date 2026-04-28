using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_HomingMissile", menuName = "BehaviorTree/Action/Task_HomingMissile")]
public class Task_HomingMissile : BaseAttackNode
{
    [Header("Prefab Settings")]
    public GameObject projectilePrefab;
    public Vector3 spawnOffset = new Vector3(0, 1.5f, 0.5f);

    [Header("Projectile Stats")]
    public float HomingDuration = 5.0f;
    public float HomingStartSpeed = 5.0f;
    public float HomingAcceleration = 5.0f;
    public float HomingMaxSpeed = 20.0f;
    public float TurningForce = 120.0f;
    public float StraightSpeed = 30.0f;
    public float maxTriggerRange = 30f;

    [Header("Settings")]
    public LayerMask obstacleMask;
    public DamageData damageData;
    public bool facePlayerOnUpdate = true;

    private GameObject _projectileInstance;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _projectileInstance = null;
    }

    protected override void OnActionSOTriggered()
    {
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
        }

        // [수정] 발사 시점에 플레이어 방향으로 즉시 회전
        Vector3 dirToPlayer = (runner.player.transform.position - runner.transform.position).normalized;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero) runner.transform.rotation = Quaternion.LookRotation(dirToPlayer);

        Vector3 spawnPos = runner.transform.position + runner.transform.TransformDirection(spawnOffset);
        _projectileInstance = ProjectilePoolManager.GetProjectile(projectilePrefab, spawnPos, runner.transform.rotation);

        if (_projectileInstance.TryGetComponent<HomingProjectile>(out var projectileScript))
        {
            projectileScript.Initialize(
                runner.player.transform,
                damageData,
                obstacleMask,
                runner,
                HomingDuration,
                HomingStartSpeed,
                HomingAcceleration,
                HomingMaxSpeed,
                TurningForce,
                StraightSpeed
            );
        }
        else
        {
            ProjectilePoolManager.ReleaseProjectile(_projectileInstance);
            _projectileInstance = null;
        }
    }

    protected override void UpdateMovement()
    {
        if (facePlayerOnUpdate && runner.player != null)
        {
            Vector3 dirToPlayer = (runner.player.transform.position - runner.transform.position).normalized;
            // dirToPlayer.y = 0;
            if (dirToPlayer != Vector3.zero)
            {
                runner.transform.rotation = Quaternion.Slerp(runner.transform.rotation, Quaternion.LookRotation(dirToPlayer), Time.deltaTime * 5f);
            }
        }
    }

    protected override bool IsMovementFinished => (_projectileInstance == null || !_projectileInstance.activeInHierarchy) && (Time.time - _nodeEntryTime > transitionBuffer + 0.5f);

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
        node.spawnOffset = this.spawnOffset;
        node.HomingDuration = this.HomingDuration;
        node.HomingStartSpeed = this.HomingStartSpeed;
        node.HomingAcceleration = this.HomingAcceleration;
        node.HomingMaxSpeed = this.HomingMaxSpeed;
        node.TurningForce = this.TurningForce;
        node.StraightSpeed = this.StraightSpeed;
        node.obstacleMask = this.obstacleMask;
        node.damageData = this.damageData;
        node.facePlayerOnUpdate = this.facePlayerOnUpdate;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        node.escapeOnHitConfirm = this.escapeOnHitConfirm;
        node.hitEscapeDelay = this.hitEscapeDelay;
        return node;
    }
}
