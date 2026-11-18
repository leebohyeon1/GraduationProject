using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Opt-in, isolated-save QA. Moves the actual player through authored triggers.
/// Does not call trigger callbacks, alter dissolve values, destroy totems or change tags.
/// </summary>
public static class TutorialIllustrationRuntimeAudit
{
    private const string Scope = "QA player teleport/CharacterController movement; real physics trigger and dissolve Update. Not keyboard navigation or obstacle-destruction gameplay.";

    public static IEnumerator Run(PlayerController player, string reportPath, Action<object> record)
    {
        RequireIsolation();
        var loader = SceneLoadingManager.Instance;
        if (!player || !player.Health || player.Health.IsDead || player.Events == null || !Camera.main ||
            !loader || loader.IsTeleporting || !loader.CurrentActiveChunk || loader.CurrentActiveChunk.name != "Part1")
            throw new InvalidOperationException("Tutorial illustration QA requires a ready Part1 player/camera.");
        var controller = player.GetComponent<CharacterController>();
        if (!controller || !controller.enabled) throw new InvalidOperationException("Player CharacterController is unavailable.");
        var zones = UnityEngine.Object.FindObjectsByType<TutorialIllustrationZone>(FindObjectsSortMode.None);
        var charge = zones.SingleOrDefault(z => z.illustrations.Any(e => e && e.name == "Tutorial_9_Illustration"));
        var normal = zones.SingleOrDefault(z => z.illustrations.Any(e => e && e.name == "Tutorial_10_Illustration"));
        if (!charge || !normal || !charge.obstacle || charge.obstacle.IsDead)
            throw new InvalidOperationException("Fresh charged/normal tutorial zones and a living charged obstacle are required.");
        int obstacleHealth = charge.obstacle.CurrentHealth;
        string sourceData = JsonUtility.ToJson(player.Data);
        string[] tags = DataManager.Instance.GetGameData().GamePlayTagIdSet.OrderBy(x => x).ToArray();
        Vector3 originalPosition = player.transform.position;
        Quaternion originalRotation = player.transform.rotation;
        int originalHealth = player.Health.CurrentHealth, guardedAttacks = 0;
        var events = player.Events;
        void Protect(ref PlayerDamageContext context)
        {
            guardedAttacks++;
            context.Data.DamageAmount = 0; context.Data.StiffnessAmount = 0;
            context.Data.KnockbackForce = 0; context.Data.KnockbackDuration = 0;
            context.HasSuperArmor = true;
        }
        var previousBlend = CinemachineCore.GetBlendOverride;
        var cameraObject = new GameObject("TutorialIllustrationAuditCamera");
        var focus = cameraObject.AddComponent<CinemachineCamera>();
        focus.Priority = -100;
        focus.Lens = LensSettings.FromCamera(Camera.main);
        focus.Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
        focus.Lens.FieldOfView = 50f;
        CinemachineCore.GetBlendOverrideDelegate blend = (from, to, definition, owner) => to == focus
            ? new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0)
            : previousBlend != null ? previousBlend(from, to, definition, owner) : definition;
        events.BeforeDamaged += Protect;
        try
        {
            foreach (var zone in new[] { charge, normal })
            {
                var collider = zone.GetComponent<Collider>();
                if (!collider || !collider.enabled || !collider.isTrigger || zone.illustrations.Length != 3 || zone.illustrations.Any(e => !e))
                    throw new InvalidOperationException("Authored tutorial trigger/three dissolve images are missing.");
                var beforeBounds = collider.bounds;
                var beforeMatrix = collider.transform.localToWorldMatrix;
                string label = zone == charge ? "charge9-2" : "normal10-7";
                Vector3 inside = GroundInside(collider, player);
                Vector3 outside = originalPosition;
                if (collider.bounds.Contains(outside) || Vector3.Distance(collider.ClosestPoint(outside), outside) < 4)
                    throw new InvalidOperationException("QA initial player position must be outside both guide zones.");
                var canvas = zone.illustrations[0].GetComponentInParent<Canvas>();
                if (!canvas || canvas.renderMode != RenderMode.WorldSpace)
                    throw new InvalidOperationException("Expected the actual world-space tutorial Canvas.");
                FrameIllustrations(focus, zone, canvas.transform, Camera.main.aspect);
                CinemachineCore.GetBlendOverride = blend;
                focus.Priority = 2000;

                string[] phases = { "outside", "enter-alive", "exit", "reenter-alive" };
                for (int index = 0; index < phases.Length; index++)
                {
                    bool visible = index % 2 == 1;
                    MovePlayer(player, controller, visible ? inside : outside);
                    var wait = AwaitDissolve(zone, visible, record, label, phases[index]);
                    while (wait.MoveNext()) yield return wait.Current;
                    if (charge.obstacle.IsDead || charge.obstacle.CurrentHealth != obstacleHealth)
                        throw new InvalidOperationException("QA changed the obstacle instead of testing guide visibility.");
                    if (collider.bounds != beforeBounds || collider.transform.localToWorldMatrix != beforeMatrix || !collider.enabled)
                        throw new InvalidOperationException("Authored tutorial collider changed during QA.");
                    string screenshot = Path.Combine(Path.GetDirectoryName(reportPath),
                        Path.GetFileNameWithoutExtension(reportPath) + "-tutorial-" + label + "-" + phases[index] + ".png");
                    var capture = SceneLoadingSmokeTest.CaptureWorld(screenshot);
                    while (capture.MoveNext()) yield return capture.Current;
                    var screens = zone.illustrations.Select(e =>
                    {
                        var point = Camera.main.WorldToScreenPoint(e.transform.position);
                        var corners = new Vector3[4];
                        ((RectTransform)e.transform).GetWorldCorners(corners);
                        var projected = corners.Select(c => Camera.main.WorldToViewportPoint(c)).ToArray();
                        return new { name = e.name, x = point.x, y = point.y, depth = point.z,
                            minX = projected.Min(p => p.x), maxX = projected.Max(p => p.x),
                            minY = projected.Min(p => p.y), maxY = projected.Max(p => p.y),
                            inFrame = projected.All(p => p.z > 0 && p.x >= 0 && p.x <= 1 && p.y >= 0 && p.y <= 1) };
                    }).ToArray();
                    record?.Invoke(new { stage = "tutorial-illustration-phase", guide = label, phase = phases[index], pass = screens.All(s => s.inFrame),
                        scope = Scope, screenshot, player = player.transform.position.ToString("F3"),
                        obstacleAlive = !charge.obstacle.IsDead, obstacleHealth, screens,
                        values = zone.illustrations.Select(e => new { e.name, e.location }).ToArray() });
                    if (screens.Any(s => !s.inFrame)) throw new InvalidOperationException("QA tutorial camera did not frame all three image rectangles.");
                }
            }
            bool unchanged = sourceData == JsonUtility.ToJson(player.Data);
            var currentTags = DataManager.Instance.GetGameData().GamePlayTagIdSet;
            record?.Invoke(new { stage = "tutorial-illustration-complete", pass = unchanged && player.Health.CurrentHealth == originalHealth,
                guides = 2, phasesPerGuide = 4, obstacleAlive = !charge.obstacle.IsDead, sourceDataUnchanged = unchanged,
                tagsAddedByActualTriggers = currentTags.Except(tags).ToArray(), tagsRemovedByActualTriggers = tags.Except(currentTags).ToArray(),
                originalHealth, health = player.Health.CurrentHealth, guardedAttacks, scope = Scope });
            if (!unchanged || player.Health.CurrentHealth != originalHealth)
                throw new InvalidOperationException("Tutorial QA changed source data or damaged the protected player.");
        }
        finally
        {
            events.BeforeDamaged -= Protect;
            if (player && controller) { MovePlayer(player, controller, originalPosition); player.transform.rotation = originalRotation; }
            if (CinemachineCore.GetBlendOverride == blend) CinemachineCore.GetBlendOverride = previousBlend;
            if (cameraObject) UnityEngine.Object.Destroy(cameraObject);
        }
    }

    private static void FrameIllustrations(CinemachineCamera focus, TutorialIllustrationZone zone, Transform canvas, float aspect)
    {
        // Authored illustrations extend outside the Canvas rect. Fit their actual
        // twelve world corners in camera-aligned space, including any depth offsets.
        Quaternion rotation = Quaternion.LookRotation(canvas.forward, canvas.up);
        Quaternion inverse = Quaternion.Inverse(rotation);
        Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
        var corners = new Vector3[4];
        foreach (var effect in zone.illustrations)
        {
            ((RectTransform)effect.transform).GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                Vector3 point = inverse * (corner - canvas.position);
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
            }
        }
        Vector3 half = (max - min) * .5f;
        Vector3 center = canvas.position + rotation * ((min + max) * .5f);
        float halfFov = focus.Lens.FieldOfView * .5f * Mathf.Deg2Rad;
        float distance = Mathf.Max(half.y, half.x / Mathf.Max(.1f, aspect)) * 1.25f / Mathf.Tan(halfFov) + half.z;
        focus.transform.SetPositionAndRotation(center - canvas.forward * Mathf.Max(distance, 1f), rotation);
    }

    private static IEnumerator AwaitDissolve(TutorialIllustrationZone zone, bool visible, Action<object> record, string guide, string phase)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + 8;
        int beganFrame = Time.frameCount;
        bool intermediateSeen = false;
        while (true)
        {
            intermediateSeen |= zone.illustrations.Any(e => e.location > .02f && e.location < .98f);
            bool arrived = zone.illustrations.All(e => visible ? e.location <= .001f : e.location >= .999f);
            if (Time.frameCount > beganFrame + 2 && arrived) break;
            if (Time.realtimeSinceStartupAsDouble > deadline)
                throw new TimeoutException("Real tutorial trigger/dissolve did not reach " + guide + "/" + phase + ": " +
                    string.Join(",", zone.illustrations.Select(e => e.location.ToString("F3"))));
            yield return null;
        }
        // A visible-to-hidden/hidden-to-visible phase must animate, not snap.
        if (phase != "outside" && !intermediateSeen)
            throw new InvalidOperationException("No intermediate dissolve frame observed: " + guide + "/" + phase);
        record?.Invoke(new { stage = "tutorial-illustration-dissolve", guide, phase, visible, intermediateSeen,
            frames = Time.frameCount - beganFrame });
    }

    private static Vector3 GroundInside(Collider zone, PlayerController player)
    {
        Vector3 center = zone.bounds.center;
        foreach (float radius in new[] { 0f, 2f, 4f, 6f })
        for (int i = 0; i < (radius == 0 ? 1 : 16); i++)
        {
            var candidate = center + Quaternion.AngleAxis(i * 22.5f, Vector3.up) * Vector3.forward * radius;
            if (!Physics.Raycast(candidate + Vector3.up * 15, Vector3.down, out var hit, 50,
                    LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore) || hit.normal.y < .75f) continue;
            candidate = hit.point + Vector3.up * .12f;
            var check = candidate + Vector3.up;
            if ((zone.ClosestPoint(check) - check).sqrMagnitude > .001f) continue;
            if (Physics.OverlapCapsule(candidate + Vector3.up * .4f, candidate + Vector3.up * 1.2f, .45f,
                LayerMask.GetMask("Wall", "Default", "Enemy"), QueryTriggerInteraction.Ignore)
                .Any(c => !c.GetComponentInParent<PlayerController>())) continue;
            return candidate;
        }
        throw new InvalidOperationException("No safe floor inside tutorial collider " + zone.name);
    }

    private static void MovePlayer(PlayerController player, CharacterController controller, Vector3 point)
    {
        bool enabled = controller.enabled;
        try { controller.enabled = false; player.transform.position = point; }
        finally { controller.enabled = enabled; }
        Physics.SyncTransforms();
        if (enabled) controller.Move(Vector3.down * .01f);
    }

    private static void RequireIsolation()
    {
        var args = SceneLoadingSmokeTest.GetArguments();
        if (!args.Contains("--soom-loading-smoke") || !args.Contains("--soom-world-presentation-audit") || !DataManager.Instance)
            throw new InvalidOperationException("Tutorial QA requires explicit isolated smoke/world flags.");
        string save = (string)typeof(DataManager).GetField("_saveFileName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DataManager.Instance);
        string persistent = Path.GetFullPath(Application.persistentDataPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrEmpty(save) || !Path.IsPathRooted(save) || !save.EndsWith(".smoke-save.json", StringComparison.OrdinalIgnoreCase) ||
            Path.GetFullPath(save).StartsWith(persistent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Refusing tutorial QA without an isolated save path.");
    }
}
