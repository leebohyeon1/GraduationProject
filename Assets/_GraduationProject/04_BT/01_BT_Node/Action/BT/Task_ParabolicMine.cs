using BehaviorTree;
using Pathfinding;
using UnityEngine;

[CreateAssetMenu(fileName = "Task_ParabolicMine", menuName = "BehaviorTree/Action/Task_ParabolicMine")]
public class Task_ParabolicMine : BaseAttackNode
{
    private const string TargetPositionKeySuffix = "_ParabolicMine_TargetPosition";
    private const string AimForwardKeySuffix = "_ParabolicMine_AimForward";
    private const string ShotPreparedKeySuffix = "_ParabolicMine_ShotPrepared";
    private const string HasFiredKeySuffix = "_ParabolicMine_HasFired";

    [Header("Projectile Settings")]
    /// <summary>
    /// 발사할 포물선 지뢰 프리팹입니다.
    /// </summary>
    public GameObject projectilePrefab;
    /// <summary>
    /// 발사 위치 오프셋입니다.
    /// </summary>
    public Vector3 spawnOffset = new Vector3(0f, 1.0f, 0.5f);
    /// <summary>
    /// 포물선 비행 총 시간입니다.
    /// </summary>
    public float projectileDuration = 1.2f;
    /// <summary>
    /// 포물선 정점 높이입니다.
    /// </summary>
    public float jumpHeight = 2.5f;
    /// <summary>
    /// 폭발에 사용할 기본 데미지 데이터입니다.
    /// </summary>
    public DamageData damageData;

    [Header("Mine Settings")]
    /// <summary>
    /// 지뢰 유지 시간입니다.
    /// </summary>
    public float mineDuration = 4.0f;
    /// <summary>
    /// 지뢰 감지 반경입니다.
    /// </summary>
    public float detectRadius = 2.5f;
    /// <summary>
    /// 감지 후 폭발까지의 지연 시간입니다.
    /// </summary>
    public float explodeDelay = 0.35f;
    /// <summary>
    /// 폭발 시 적용할 데미지 값입니다.
    /// </summary>
    public int explosionDamage = 10;
    /// <summary>
    /// 폭발 반경입니다.
    /// </summary>
    public float explosionRadius = 5.0f;

    [Header("Collision Settings")]
    /// <summary>
    /// 지면 판정 레이어입니다.
    /// </summary>
    public LayerMask groundLayer;
    /// <summary>
    /// 벽 판정 레이어입니다.
    /// </summary>
    public LayerMask wallLayer;
    /// <summary>
    /// 플레이어 판정 레이어입니다.
    /// </summary>
    public LayerMask playerLayer;

    [Header("Targeting")]
    /// <summary>
    /// 공격 진입 최대 거리입니다.
    /// </summary>
    public float maxTriggerRange = 25f;
    /// <summary>
    /// 폭발 시 재생할 피드백 이름입니다.
    /// </summary>
    public string feedbackName = string.Empty;

    private string _targetPositionKey;
    private string _aimForwardKey;
    private string _shotPreparedKey;
    private string _hasFiredKey;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        InitializeBlackboardKeys();
        brain.blackboard.SetValue(_shotPreparedKey, false);
        brain.blackboard.SetValue(_hasFiredKey, false);
    }

    protected override void OnActionSOTriggered()
    {
        InitializeBlackboardKeys();

        if (runner.player == null)
        {
            brain.blackboard.SetValue(_shotPreparedKey, false);
            brain.blackboard.SetValue(_hasFiredKey, true);
            return;
        }

        Vector3 spawnPosition = runner.transform.position + (runner.transform.rotation * spawnOffset);
        Vector3 capturedTargetPosition = runner.player.transform.position;
        Vector3 lookDirection = capturedTargetPosition - spawnPosition;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude <= 0.0001f)
        {
            lookDirection = runner.transform.forward;
        }

        Vector3 aimForward = lookDirection.normalized;

        brain.blackboard.SetValue(_targetPositionKey, capturedTargetPosition);
        brain.blackboard.SetValue(_aimForwardKey, aimForward);
        brain.blackboard.SetValue(_shotPreparedKey, true);
        brain.blackboard.SetValue(_hasFiredKey, false);

        runner.transform.rotation = Quaternion.LookRotation(aimForward);
        Debug.Log($"[Task_ParabolicMine] Cached target position in blackboard: {capturedTargetPosition}");

        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
        }
    }

    protected override void UpdateMovement()
    {
        if (!brain.blackboard.GetValueOrDefault(_shotPreparedKey, false) ||
            brain.blackboard.GetValueOrDefault(_hasFiredKey, false))
        {
            return;
        }

        Vector3 aimForward = brain.blackboard.GetValueOrDefault(_aimForwardKey, runner.transform.forward);
        if (aimForward.sqrMagnitude > 0.0001f)
        {
            runner.transform.rotation = Quaternion.LookRotation(aimForward);
        }

        if (Handler.IsHitWindowOpen)
        {
            Fire();
        }
    }

    protected override bool IsMovementFinished => brain.blackboard.GetValueOrDefault(_hasFiredKey, false);

    private void Fire()
    {
        InitializeBlackboardKeys();

        if (!brain.blackboard.GetValue(_targetPositionKey, out Vector3 capturedTargetPosition))
        {
            Debug.LogWarning("[Task_ParabolicMine] Cached target position not found in blackboard.");
            brain.blackboard.SetValue(_hasFiredKey, true);
            Handler.CloseHitWindow();
            return;
        }

        Vector3 aimForward = brain.blackboard.GetValueOrDefault(_aimForwardKey, runner.transform.forward);
        if (aimForward.sqrMagnitude <= 0.0001f)
        {
            aimForward = runner.transform.forward;
        }

        Quaternion lockedRotation = Quaternion.LookRotation(aimForward);
        Vector3 spawnPosition = runner.transform.position + (lockedRotation * spawnOffset);
        brain.blackboard.SetValue(_hasFiredKey, true);

        if (projectilePrefab != null)
        {
            Debug.Log($"[Task_ParabolicMine] Fire with cached target: {capturedTargetPosition}");
            GameObject projectileObject = ProjectilePoolManager.GetProjectile(projectilePrefab, spawnPosition, lockedRotation);
            if (projectileObject.TryGetComponent<ParabolicMineProjectile>(out var projectile))
            {
                projectile.Setup(
                    runner,
                    spawnPosition,
                    capturedTargetPosition,
                    runner.gameObject,
                    projectileDuration,
                    jumpHeight,
                    mineDuration,
                    detectRadius,
                    explodeDelay,
                    damageData,
                    explosionDamage,
                    explosionRadius,
                    groundLayer,
                    wallLayer,
                    playerLayer,
                    feedbackName);
            }
            else
            {
                Debug.LogWarning($"[Task_ParabolicMine] {projectilePrefab.name} 에 ParabolicMineProjectile 컴포넌트가 없습니다.");
                ProjectilePoolManager.ReleaseProjectile(projectileObject);
            }
        }

        Handler.CloseHitWindow();
    }

    protected override void SpecificCleanup()
    {
        base.SpecificCleanup();
        InitializeBlackboardKeys();
        brain.blackboard.RemoveKey(_targetPositionKey);
        brain.blackboard.RemoveKey(_aimForwardKey);
        brain.blackboard.RemoveKey(_shotPreparedKey);
        brain.blackboard.RemoveKey(_hasFiredKey);
    }

    private void InitializeBlackboardKeys()
    {
        if (!string.IsNullOrEmpty(_targetPositionKey))
        {
            return;
        }

        string resolvedAttackKey = string.IsNullOrEmpty(attackKey) ? name : attackKey;
        _targetPositionKey = resolvedAttackKey + TargetPositionKeySuffix;
        _aimForwardKey = resolvedAttackKey + AimForwardKeySuffix;
        _shotPreparedKey = resolvedAttackKey + ShotPreparedKeySuffix;
        _hasFiredKey = resolvedAttackKey + HasFiredKeySuffix;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = attackKey;
        node.transitionBuffer = transitionBuffer;
        node.SO = SO;
        node.LoopAttack = LoopAttack;
        node.NextBT = NextBT;
        node.debugMode = debugMode;
        node.checkRangeOnEnter = checkRangeOnEnter;
        node.rangeThreshold = rangeThreshold;
        node.ignoreYDistance = ignoreYDistance;
        node.allowOutOfCombat = allowOutOfCombat;
        node.projectilePrefab = projectilePrefab;
        node.spawnOffset = spawnOffset;
        node.projectileDuration = projectileDuration;
        node.jumpHeight = jumpHeight;
        node.damageData = damageData;
        node.mineDuration = mineDuration;
        node.detectRadius = detectRadius;
        node.explodeDelay = explodeDelay;
        node.explosionDamage = explosionDamage;
        node.explosionRadius = explosionRadius;
        node.groundLayer = groundLayer;
        node.wallLayer = wallLayer;
        node.playerLayer = playerLayer;
        node.maxTriggerRange = maxTriggerRange;
        node.feedbackName = feedbackName;
        node.ExceptKey = ExceptKey;
        node.escapeOnHitConfirm = escapeOnHitConfirm;
        node.hitEscapeDelay = hitEscapeDelay;
        return node;
    }
}
