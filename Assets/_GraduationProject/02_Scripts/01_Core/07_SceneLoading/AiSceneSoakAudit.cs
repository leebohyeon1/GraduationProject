using System;
using System.Collections;
using BehaviorTree;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Opt-in observation of an already running scene. The caller owns the coroutine
/// and result storage; this helper never changes gameplay state or starts itself.
/// </summary>
public static class AiSceneSoakAudit
{
    public static IEnumerator Prepare(PlayerController player, Action<object> record)
    {
        // Distant authored enemies have deferred initialization, so their runtime
        // Data/GroupAi/Health references are not available until we approach.
        var enemies = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        var anchor = enemies.FirstOrDefault(e => e.GetComponentInParent<GroupAiZone>() is GroupAiZone zone &&
            zone.name.Replace("_", "").ToLowerInvariant().EndsWith("zone1"));
        if (anchor == null) throw new InvalidOperationException("AI soak requires an authored Zone 1 enemy.");
        var zones = UnityEngine.Object.FindObjectsByType<GroupAiZone>(FindObjectsSortMode.None);
        Vector3? safePoint = null;
        float bestDistance = 0f;
        int ground = LayerMask.GetMask("Ground");
        for (int index = 0; index < 48; index++)
        {
            float angle = index * Mathf.PI * 2f / 48f;
            Vector3 candidate = anchor.transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 36f;
            if (!Physics.Raycast(candidate + Vector3.up * 40f, Vector3.down, out var hit, 100f, ground, QueryTriggerInteraction.Ignore) || hit.normal.y < .75f) continue;
            candidate = hit.point + Vector3.up;
            if (zones.Any(z => z.IsInZone(candidate))) continue;
            if (Physics.CheckCapsule(candidate + Vector3.up * .3f, candidate + Vector3.up * 1.2f, .4f,
                LayerMask.GetMask("Wall", "Default"), QueryTriggerInteraction.Ignore)) continue;
            float distance = enemies.Min(e => Vector3.Distance(candidate, e.transform.position));
            if (distance <= 28f || distance <= bestDistance) continue;
            bestDistance = distance;
            safePoint = candidate;
        }
        if (!safePoint.HasValue) throw new InvalidOperationException("No safe grounded observation point outside combat zones.");
        var controller = player.GetComponent<CharacterController>();
        bool wasEnabled = controller != null && controller.enabled;
        if (controller != null) controller.enabled = false;
        player.transform.position = safePoint.Value;
        Physics.SyncTransforms();
        if (controller != null) controller.enabled = wasEnabled;
        double started = Time.realtimeSinceStartupAsDouble;
        while (!enemies.Any(e => e != null && e._aiController != null && e._aiController.isActiveAndEnabled &&
            e._initializer != null && e._initializer.IsInitialized("Final") && Vector3.Distance(e.transform.position, player.transform.position) < 60f))
        {
            if (Time.realtimeSinceStartupAsDouble - started > 10) throw new TimeoutException("Nearby AI did not activate naturally.");
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1f);
        record?.Invoke(new { stage = "ai-soak-positioned", anchor = anchor.name, position = player.transform.position.ToString("F3"),
            initialNearestEnemyDistance = bestDistance, currentNode = Node.CurrentNodeName?.ToString(),
            setup = "Grounded observation outside zones; normal GroupAi distance activation; no AI/stat/input overrides" });
    }

    public static IEnumerator Run(int seconds, Action<object> record)
    {
        int durationSeconds = Mathf.Clamp(seconds, 30, 600);
        double startTime = Time.realtimeSinceStartupAsDouble;
        double nextSample = 30d;
        var wait = new WaitForSecondsRealtime(1f);

        Record("ai-soak-start", startTime, durationSeconds, record);
        while (Time.realtimeSinceStartupAsDouble - startTime < durationSeconds)
        {
            yield return wait;
            double elapsed = Time.realtimeSinceStartupAsDouble - startTime;
            if (elapsed >= durationSeconds) break;
            if (elapsed < nextSample) continue;

            Record("ai-soak-sample", startTime, durationSeconds, record);
            // Do not emit duplicate snapshots if a stalled frame crossed several
            // sample boundaries. Real elapsed time remains visible in each row.
            nextSample = (Math.Floor(elapsed / 30d) + 1d) * 30d;
        }
        Record("ai-soak-final", startTime, durationSeconds, record);
    }

    private static void Record(string stage, double startTime, int durationSeconds, Action<object> record)
    {
        int nodeCount = Resources.FindObjectsOfTypeAll<Node>().Length;
        int activeAiCount = 0;
        foreach (var controller in UnityEngine.Object.FindObjectsByType<AiController>(FindObjectsSortMode.None))
            if (controller.isActiveAndEnabled) activeAiCount++;

        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        var health = player != null ? player.Health : null;
        if (activeAiCount == 0 || health == null || health.IsDead || Time.timeScale <= 0f)
            throw new InvalidOperationException("AI soak lost its living player or active AI; this is not a valid running-AI sample.");
        long allocated = Profiler.GetTotalAllocatedMemoryLong();
        long managed = Profiler.GetMonoUsedSizeLong();
        record?.Invoke(new
        {
            stage,
            scene = SceneManager.GetActiveScene().name,
            durationSeconds,
            elapsedSeconds = Time.realtimeSinceStartupAsDouble - startTime,
            realtimeSeconds = Time.realtimeSinceStartupAsDouble,
            frame = Time.frameCount,
            nodeCount,
            activeAiControllerCount = activeAiCount,
            timeScale = Time.timeScale,
            playerFound = player != null,
            playerAlive = health != null && !health.IsDead,
            playerHealth = health != null ? (int?)health.CurrentHealth : null,
            unityAllocatedBytes = allocated,
            unityReservedBytes = Profiler.GetTotalReservedMemoryLong(),
            managedUsedBytes = managed,
            nativeEstimateBytes = Math.Max(0L, allocated - managed)
        });
    }
}
