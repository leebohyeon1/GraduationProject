using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Opt-in packaged-player QA. Normal launches never create this component.
/// Usage: --soom-loading-smoke --soom-smoke-output "absolute/report.json"
/// Optional: --soom-smoke-quit exits with 0 on success or 1 on failure.
/// </summary>
public sealed class SceneLoadingSmokeTest : MonoBehaviour
{
    private const double TransitionTimeoutSeconds = 240;
    private const int MaxCapturedErrors = 50;
    private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    private readonly List<object> _steps = new List<object>();
    private readonly List<object> _errors = new List<object>();
    private string _reportPath;
    private string _isolatedSavePath;
    private string _failure;
    private string _status = "starting";
    private bool _quitWhenDone;
    private bool _writingReport;
    private int _errorCount;
    private DateTime _startedUtc;
    private Stopwatch _elapsed;
    private static string[] _launchArguments;

    internal static string[] GetArguments()
    {
        if (_launchArguments != null) return _launchArguments;
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            if (!UnityEditor.SessionState.GetBool("Soom.LoadingSmoke.Enabled", false)) return Array.Empty<string>();
            return _launchArguments = new[] { "--soom-loading-smoke", "--soom-smoke-output",
                UnityEditor.SessionState.GetString("Soom.LoadingSmoke.Output", ""), "--soom-smoke-quit" }
                .Concat(UnityEditor.SessionState.GetString("Soom.LoadingSmoke.Options", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
        }
#endif
        return _launchArguments = Environment.GetCommandLineArgs();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        _launchArguments = null;
        var args = GetArguments();
        if (!args.Contains("--soom-loading-smoke")) return;
        int outputIndex = Array.IndexOf(args, "--soom-smoke-output");
        if (outputIndex < 0 || outputIndex + 1 >= args.Length) return;

        string reportPath;
        try
        {
            string requested = args[outputIndex + 1];
            if (!Path.IsPathRooted(requested)) throw new ArgumentException("Smoke output must be an absolute path.");
            reportPath = Path.GetFullPath(requested);
            string directory = Path.GetDirectoryName(reportPath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                throw new DirectoryNotFoundException("Smoke output parent directory must already exist.");
            string userSaveDirectory = Path.GetFullPath(Application.persistentDataPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (reportPath.StartsWith(userSaveDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Smoke output cannot be placed in the user's save directory.");
        }
        catch (Exception error)
        {
            UnityEngine.Debug.LogError("[LoadingSmoke] Refused diagnostic launch: " + error.Message);
            if (args.Contains("--soom-smoke-quit")) Application.Quit(1);
            return;
        }

        var owner = new GameObject("Loading Smoke Test (opt-in)");
        DontDestroyOnLoad(owner);
        var runner = owner.AddComponent<SceneLoadingSmokeTest>();
        runner._reportPath = reportPath;
        runner._quitWhenDone = args.Contains("--soom-smoke-quit");
        runner._startedUtc = DateTime.UtcNow;
        runner._elapsed = Stopwatch.StartNew();
        runner._isolatedSavePath = Path.Combine(Path.GetDirectoryName(reportPath),
            Path.GetFileNameWithoutExtension(reportPath) + "." + Guid.NewGuid().ToString("N") + ".smoke-save.json");
        Application.logMessageReceived += runner.CaptureLog;

        try
        {
            // AfterSceneLoad runs after scene Awake and before Start. Redirect before
            // development-start, test CreateNewGame, pause, or quit can write a slot.
            var manager = DataManager.Instance;
            if (manager == null) throw new InvalidOperationException("DataManager must be present in the startup CoreScene.");
            var saveField = typeof(DataManager).GetField("_saveFileName", PrivateInstance);
            if (saveField == null) throw new MissingFieldException("DataManager._saveFileName");
            string combined = Path.GetFullPath(Path.Combine(Application.persistentDataPath, runner._isolatedSavePath));
            if (!string.Equals(combined, runner._isolatedSavePath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Isolated save path did not override the persistent directory.");
            saveField.SetValue(manager, runner._isolatedSavePath);
            typeof(DataManager).GetField("_useDevelopment", PrivateInstance)?.SetValue(manager, false);
            manager.DataList = new List<GameData>();
            // Keep the redirect for the full process lifetime, including OnApplicationQuit.
            runner.WriteReport();
            Application.runInBackground = true;
            if (args.Contains("--soom-top-layer-audit") || args.Contains("--soom-ai-soak-seconds") || args.Contains("--soom-combat-soak-seconds") || args.Contains("--soom-f-projectile-audit"))
                AudioListener.volume = 0f; // Keep the audio clock/pool lifecycle running during silent long QA.
            runner.StartCoroutine(runner.Guard(runner.Run()));
        }
        catch (Exception error)
        {
            runner.Finish(error);
        }
    }

    private IEnumerator Guard(IEnumerator routine)
    {
        while (true)
        {
            bool next;
            object wait = null;
            try
            {
                next = routine.MoveNext();
                if (next) wait = routine.Current;
            }
            catch (Exception error)
            {
                Finish(error);
                yield break;
            }
            if (!next)
            {
                Finish(null);
                yield break;
            }
            yield return wait;
        }
    }

    private IEnumerator Run()
    {
        // Let all scene Start callbacks begin the initial Title transition.
        yield return null;
        var loader = SceneLoadingManager.Instance;
        if (loader == null) throw new InvalidOperationException("SceneLoadingManager is missing.");
        while (loader.IsTeleporting || loader.CurrentActiveChunk == null)
        {
            if (_elapsed.Elapsed.TotalSeconds > TransitionTimeoutSeconds)
                throw new TimeoutException("Startup Title did not become playable.");
            yield return null;
        }
        if (_elapsed.Elapsed.TotalSeconds > TransitionTimeoutSeconds)
            throw new TimeoutException("Startup Title exceeded 240 seconds.");
        if (loader.LastLoadError != null) throw new InvalidOperationException(loader.LastLoadError);
        if (loader.CurrentActiveChunk.name != "Title") throw new InvalidOperationException("Smoke test requires Title startup.");
        _steps.Add(new { stage = "loaded", target = "StartupTitle", utc = DateTime.UtcNow.ToString("O"), seconds = Time.realtimeSinceStartupAsDouble,
            frame = Time.frameCount, valid = true, memory = MemorySnapshot() });
        WriteReport();

        var args = GetArguments();
        if (args.Contains("--soom-boss-ending-audit"))
        {
            var endingAudit = BossEndingRuntimeAudit.Run(row => { _steps.Add(row); WriteReport(); });
            while (endingAudit.MoveNext()) yield return endingAudit.Current;
            yield break;
        }

        var main = loader.SceneDatabase.AllScenes.First(x => x != null && x.name == "Part1");
        DataManager.Instance.CreateNewGame(0, main);
        GamePlayTagManager.Instance.ClearTags();
        GamePlayTagManager.Instance.Initialize();
        GamePlayTagManager.Instance.AddTag(DataManager.Instance.GetGamePlayTag("GameStart_TimeLiine_End"));
        GamePlayTagManager.Instance.AddTag(DataManager.Instance.GetGamePlayTag("SwordTouch"));

        var targetField = typeof(SceneTransitionInteractor).GetField("_targetSceneData", PrivateInstance);
        var savePlayerField = typeof(SavePoint).GetField("_playerController", PrivateInstance);
        if (targetField == null || savePlayerField == null) throw new MissingFieldException("Smoke interaction reflection targets changed.");
        Vector3? previousMainCheckpoint = null;
        string previousMainCheckpointScene = null;
        bool verifyMainCheckpointOnReturn = false;
        int soakIndex = Array.IndexOf(args, "--soom-ai-soak-seconds");
        int soakSeconds = 0;
        if (soakIndex >= 0 && (soakIndex + 1 >= args.Length || !int.TryParse(args[soakIndex + 1], out soakSeconds)))
            throw new ArgumentException("AI soak duration must be an integer in seconds.");
        bool soakAudit = soakIndex >= 0;
        int combatSoakIndex = Array.IndexOf(args, "--soom-combat-soak-seconds");
        int combatSoakSeconds = 0;
        if (combatSoakIndex >= 0 && (combatSoakIndex + 1 >= args.Length || !int.TryParse(args[combatSoakIndex + 1], out combatSoakSeconds)))
            throw new ArgumentException("Combat soak duration must be an integer in seconds.");
        bool combatSoakAudit = combatSoakIndex >= 0;
        bool feedbackAudit = args.Contains("--soom-combat-feedback-audit");
        bool inputAudit = args.Contains("--soom-combat-input-audit");
        bool fProjectileAudit = args.Contains("--soom-f-projectile-audit");
        bool fParryAudit = args.Contains("--soom-f-parry-audit");
        bool destructibleAudit = args.Contains("--soom-destructible-audit");
        bool compactAudit = soakAudit || feedbackAudit || inputAudit || combatSoakAudit || fProjectileAudit || fParryAudit || destructibleAudit;
        // Do not save or unload between the dwell and C entry: that would clear
        // accumulated objects and hide the long-play loading regression.
        string[] sequence = destructibleAudit ? new[] { "Part1", "Title" } :
            (fProjectileAudit || fParryAudit) ? new[] { "Part1", "Part2-B", "Part1", "Title" } :
            compactAudit ? new[] { "Part1", "Part2-C", "Part1", "Title" } :
            new[] { "Part1", "Save", "Part2-A", "Part1", "Save", "Part2-B", "Part1", "Part2-C", "Part1", "Title" };
        for (int index = 0; index < sequence.Length; index++)
        {
            string target = sequence[index];
            var player = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            var canonical = target == "Save" ? loader.CurrentActiveChunk : loader.SceneDatabase.AllScenes.First(x => x != null && x.name == target);
            int startFrame = Time.frameCount;
            var timer = Stopwatch.StartNew();
            _status = "running";
            _steps.Add(new { stage = "loading", index, target, from = loader.CurrentActiveChunk.name, memory = MemorySnapshot() });
            WriteReport();

            if (target == "Save")
            {
                if (player == null) throw new InvalidOperationException("Save interaction has no player.");
                var point = FindObjectsByType<SavePoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .OrderBy(x => Vector3.Distance(x.transform.position, player.transform.position)).FirstOrDefault();
                if (point == null) throw new InvalidOperationException("No active save point for the current scene.");
                savePlayerField.SetValue(point, player);
                point.Interact();
            }
            else if (index == 0 || target == "Title") loader.TeleportToScene(canonical);
            else
            {
                if (player == null) throw new InvalidOperationException("Portal interaction has no player.");
                var portal = FindObjectsByType<SceneTransitionInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Where(x => { var data = targetField.GetValue(x) as SceneDataSO; return data != null && data.name == target; })
                    .OrderBy(x => Vector3.Distance(x.transform.position, player.transform.position)).FirstOrDefault();
                if (portal == null) throw new InvalidOperationException("No portal to " + target);
                // Runtime instances only: repeated QA visits must not complete dungeons.
                portal.completedDungeonTag = null;
                portal.completesDungeonOnExit = false;
                portal.TransitionToScene();
            }
            if (!loader.IsTeleporting) throw new InvalidOperationException("Transition did not start: " + target);
            while (loader.IsTeleporting)
            {
                if (timer.Elapsed.TotalSeconds > TransitionTimeoutSeconds) throw new TimeoutException(target + " exceeded 240 seconds.");
                yield return null;
            }
            if (timer.Elapsed.TotalSeconds > TransitionTimeoutSeconds)
                throw new TimeoutException(target + " exceeded 240 seconds.");
            yield return null;
            player = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            var controller = player != null ? player.GetComponent<CharacterController>() : null;
            bool healthReady = player != null && player.Health != null;
            bool alive = healthReady && !player.Health.IsDead;
            bool movement = controller != null && controller.enabled && player.gameObject.activeInHierarchy;
            bool valid = loader.CurrentActiveChunk == canonical && loader.LastLoadError == null && Time.frameCount > startFrame &&
                         (target == "Title" || (movement && alive));
            _steps.Add(new { stage = "loaded", index, target, seconds = timer.Elapsed.TotalSeconds, frames = Time.frameCount - startFrame,
                valid, alive, healthReady, movement, error = loader.LastLoadError, position = player != null ? player.transform.position.ToString("F3") : null,
                memory = MemorySnapshot() });
            WriteReport();
            if (!valid) throw new InvalidOperationException("Transition/spawn/health assertion failed: " + target);

            if (target != "Title" && args.Contains("--soom-balance-audit"))
            {
                _steps.Add(BalanceRuntimeAudit.Capture(player));
                WriteReport();
            }

            if (inputAudit && index == 0)
            {
                var combat = CombatInputRuntimeAudit.Run(player, row => { _steps.Add(row); WriteReport(); });
                while (combat.MoveNext()) yield return combat.Current;
            }

            if (destructibleAudit && index == 0)
            {
                var blockers = DestructibleBlockerRuntimeAudit.Run(player, row => { _steps.Add(row); WriteReport(); });
                while (blockers.MoveNext()) yield return blockers.Current;
            }

            if (fProjectileAudit && target == "Part2-B")
            {
                var fProjectiles = FProjectileRuntimeAudit.Run(player, 600, row => { _steps.Add(row); WriteReport(); });
                while (fProjectiles.MoveNext()) yield return fProjectiles.Current;
            }

            if (fParryAudit && target == "Part2-B")
            {
                var fParry = FProjectileParryRuntimeAudit.Run(player, row => { _steps.Add(row); WriteReport(); });
                while (fParry.MoveNext()) yield return fParry.Current;
            }

            if (feedbackAudit && (index == 0 || target == "Part2-C"))
            {
                var feedback = CombatFeedbackRuntimeAudit.Run(player, row => { _steps.Add(row); WriteReport(); });
                while (feedback.MoveNext()) yield return feedback.Current;
            }

            if (target == "Part2-C" && args.Contains("--soom-gordian-feedback-hold"))
            {
                _steps.Add(new { stage = "gordian-external-audit-window", durationSeconds = 90 });
                WriteReport();
                yield return new WaitForSecondsRealtime(90f);
            }

            if (index == 0 && args.Contains("--soom-top-layer-audit"))
            {
                yield return new WaitForSecondsRealtime(1f);
                string screenshot = Path.ChangeExtension(_reportPath, ".png");
                var capture = CaptureWorld(screenshot);
                while (capture.MoveNext()) yield return capture.Current;
                _steps.Add(new { stage = "top-layer-audit", target,
                    propsIntensity = Shader.GetGlobalFloat("ASPP_TopLayerIntensity"),
                    heightStart = Shader.GetGlobalFloat("ASPT_TopLayerHeightStart"),
                    heightFade = Shader.GetGlobalFloat("ASPT_TopLayerHeightFade"), screenshot });
                WriteReport();
                yield return new WaitForSecondsRealtime(0.5f);
                yield break;
            }

            if (index == 0 && soakAudit)
            {
                var prepare = AiSceneSoakAudit.Prepare(player, row => { _steps.Add(row); WriteReport(); });
                while (prepare.MoveNext()) yield return prepare.Current;
                var soak = AiSceneSoakAudit.Run(soakSeconds, row => { _steps.Add(row); WriteReport(); });
                while (soak.MoveNext()) yield return soak.Current;
            }

            if (index == 0 && combatSoakAudit)
            {
                var combatSoak = CombatSoakRuntimeAudit.Run(player, combatSoakSeconds, row => { _steps.Add(row); WriteReport(); });
                while (combatSoak.MoveNext()) yield return combatSoak.Current;
            }

            if (target == "Save" && canonical.name == "Part1")
            {
                var savedPlayer = DataManager.Instance.GetGameData().PlayerData;
                previousMainCheckpoint = savedPlayer.RespawnPosition;
                previousMainCheckpointScene = savedPlayer.RespawnSceneName;
            }
            if (target == "Part2-C" && !compactAudit)
            {
                if (!previousMainCheckpoint.HasValue) throw new InvalidOperationException("No previous Part1 save point was exercised.");
                var data = DataManager.Instance.GetGameData();
                bool checkpointValid = data.PlayerData.RespawnSceneName == canonical.SceneName &&
                    Vector3.Distance(data.PlayerData.RespawnPosition, canonical.DefaultSpawnPosition) < 1f &&
                    !data.IsFirstVisit("checkpoint:Part2-C") &&
                    data.PlayerData.MainRespawnSceneName == previousMainCheckpointScene &&
                    Vector3.Distance(data.PlayerData.MainRespawnPosition, previousMainCheckpoint.Value) < 1f;
                // Read only the QA save: check that the automatically created checkpoint reached disk.
                var disk = JsonUtility.FromJson<SaveDataContainer>(File.ReadAllText(_isolatedSavePath));
                var diskData = disk?.DataList?.FirstOrDefault();
                bool persisted = diskData != null && diskData.PlayerData.RespawnSceneName == canonical.SceneName &&
                    Vector3.Distance(diskData.PlayerData.RespawnPosition, canonical.DefaultSpawnPosition) < 1f &&
                    !diskData.IsFirstVisit("checkpoint:Part2-C");
                _steps.Add(new { stage = "checkpoint", target = "Part2-C-first-entry", valid = checkpointValid && persisted,
                    persisted, position = data.PlayerData.RespawnPosition.ToString("F3"),
                    expected = canonical.DefaultSpawnPosition.ToString("F3"), previousMainCheckpoint = previousMainCheckpoint.Value.ToString("F3") });
                WriteReport();
                if (!checkpointValid || !persisted) throw new InvalidOperationException("Part2-C first-entry checkpoint was not retained and saved.");
                var restart = VerifyDeathAndRestart("Part2-C-checkpoint-restart", canonical, data.PlayerData.RespawnPosition);
                // Flatten the iterator so Guard catches failures inside the restart assertions too.
                while (restart.MoveNext()) yield return restart.Current;
                verifyMainCheckpointOnReturn = true;
            }
            else if (target == "Part1" && verifyMainCheckpointOnReturn)
            {
                var savedPlayer = DataManager.Instance.GetGameData().PlayerData;
                bool restored = savedPlayer.RespawnSceneName == previousMainCheckpointScene &&
                    Vector3.Distance(savedPlayer.RespawnPosition, previousMainCheckpoint.Value) < 1f;
                _steps.Add(new { stage = "checkpoint", target = "Part1-after-C-return", valid = restored,
                    position = savedPlayer.RespawnPosition.ToString("F3"), expected = previousMainCheckpoint.Value.ToString("F3") });
                WriteReport();
                if (!restored) throw new InvalidOperationException("C exit did not restore the previous Part1 checkpoint.");
                var restart = VerifyDeathAndRestart("Part1-previous-checkpoint-restart", main, previousMainCheckpoint.Value);
                while (restart.MoveNext()) yield return restart.Current;
                verifyMainCheckpointOnReturn = false;
            }
            yield return new WaitForSecondsRealtime(0.25f);
        }
    }

    private IEnumerator VerifyDeathAndRestart(string label, SceneDataSO expectedScene, Vector3 expectedPosition)
    {
        var loader = SceneLoadingManager.Instance;
        var player = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player == null || player.Health == null || player.Health.IsDead)
            throw new InvalidOperationException("Restart audit requires a living player: " + label);
        var playerField = typeof(GameOverUI).GetField("_player", PrivateInstance);
        var panelField = typeof(GameOverUI).GetField("_panel", PrivateInstance);
        var busyField = typeof(GameOverUI).GetField("_isActionProcessing", PrivateInstance);
        if (playerField == null || panelField == null || busyField == null)
            throw new MissingFieldException("GameOverUI audit fields changed.");
        var gameOver = FindObjectsByType<GameOverUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(ui => playerField.GetValue(ui) as PlayerController == player);
        if (gameOver == null) throw new InvalidOperationException("GameOverUI is not bound to the current player.");
        var panel = panelField.GetValue(gameOver) as GameObject;
        if (panel == null) throw new InvalidOperationException("GameOverUI panel is missing.");

        var deathTimer = Stopwatch.StartNew();
        int deathFrame = Time.frameCount;
        _steps.Add(new { stage = "death", target = label, trigger = "Health.ChangeHealth then Health.Die", expected = expectedPosition.ToString("F3") });
        WriteReport();
        player.Health.ChangeHealth(-player.Health.CurrentHealth);
        player.Health.Die();
        while (!panel.activeInHierarchy || (bool)busyField.GetValue(gameOver))
        {
            if (deathTimer.Elapsed.TotalSeconds > 10) throw new TimeoutException("Game-over reveal did not finish: " + label);
            yield return null;
        }
        var panelGroup = panel.GetComponent<CanvasGroup>();
        bool deathValid = player.Health.IsDead && !player.gameObject.activeInHierarchy && panelGroup != null &&
                          panelGroup.interactable && panelGroup.alpha >= 0.99f && Time.frameCount > deathFrame;
        _steps.Add(new { stage = "game-over-ready", target = label, valid = deathValid,
            seconds = deathTimer.Elapsed.TotalSeconds, frames = Time.frameCount - deathFrame });
        WriteReport();
        if (!deathValid) throw new InvalidOperationException("Game-over did not finish its normal death/reveal flow: " + label);
        if (UnityEngine.EventSystems.EventSystem.current == null) throw new InvalidOperationException("Restart UI has no EventSystem.");

        var restartTimer = Stopwatch.StartNew();
        int restartFrame = Time.frameCount;
        gameOver.OnContinueButton(); // The same public handler wired to the actual retry button.
        while (!loader.IsTeleporting)
        {
            if (restartTimer.Elapsed.TotalSeconds > 5) throw new TimeoutException("Retry button did not start scene loading: " + label);
            yield return null;
        }
        while (loader.IsTeleporting)
        {
            if (restartTimer.Elapsed.TotalSeconds > TransitionTimeoutSeconds) throw new TimeoutException(label + " exceeded 240 seconds.");
            yield return null;
        }
        if (restartTimer.Elapsed.TotalSeconds > TransitionTimeoutSeconds) throw new TimeoutException(label + " exceeded 240 seconds.");
        yield return null;
        player = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        var controller = player != null ? player.GetComponent<CharacterController>() : null;
        bool alive = player != null && player.Health != null && !player.Health.IsDead;
        bool movement = controller != null && controller.enabled && player.gameObject.activeInHierarchy;
        // A/B/C share one native scene and SceneName; retry may resolve the A data entry
        // while correctly using the C custom checkpoint. Verify the scene GUID and position.
        bool correctScene = loader.CurrentActiveChunk != null &&
            loader.CurrentActiveChunk.SceneReference.AssetGUID == expectedScene.SceneReference.AssetGUID;
        float distance = player != null ? Vector3.Distance(player.transform.position, expectedPosition) : float.PositiveInfinity;
        Vector3 offset = player != null ? player.transform.position - expectedPosition : Vector3.positiveInfinity;
        float horizontalDistance = new Vector2(offset.x, offset.z).magnitude;
        float verticalOffset = Mathf.Abs(offset.y);
        // Authored spawn points sit above the floor; observed controller settling is
        // already 1.13m in dungeon A. Check checkpoint identity horizontally and allow
        // up to 3m of vertical settling. This is not an airborne-spawn/grounding test.
        bool valid = loader.LastLoadError == null && correctScene && alive && movement &&
            horizontalDistance < 1f && verticalOffset < 3f && Time.frameCount > restartFrame;
        _steps.Add(new { stage = "restarted", target = label, seconds = restartTimer.Elapsed.TotalSeconds,
            frames = Time.frameCount - restartFrame, valid, correctScene, alive, movement, distance, horizontalDistance, verticalOffset,
            position = player != null ? player.transform.position.ToString("F3") : null, expected = expectedPosition.ToString("F3"),
            error = loader.LastLoadError, memory = MemorySnapshot() });
        WriteReport();
        if (!valid) throw new InvalidOperationException("Retry did not revive the player at the expected checkpoint: " + label);
    }

    private static object MemorySnapshot()
    {
        long allocated = Profiler.GetTotalAllocatedMemoryLong();
        long managed = Profiler.GetMonoUsedSizeLong();
        return new { unityAllocatedBytes = allocated, unityReservedBytes = Profiler.GetTotalReservedMemoryLong(),
            managedUsedBytes = managed, nativeEstimateBytes = Math.Max(0, allocated - managed) };
    }

    private static IEnumerator CaptureWorld(string path)
    {
        yield return null;
        var camera = Camera.main;
        if (camera == null) throw new InvalidOperationException("Main camera missing for world capture.");
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            throw new InvalidOperationException("World capture requires a graphics device.");
        int width = 1280;
        int height = Mathf.Max(1, Mathf.RoundToInt(width / Mathf.Max(0.01f, camera.aspect)));
        var texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Texture2D image = null;
        var oldActive = RenderTexture.active;
        var oldTarget = camera.targetTexture;
        try
        {
            texture.Create();
            var request = new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = texture };
            if (!UnityEngine.Rendering.RenderPipeline.SupportsRenderRequest(camera, request))
                throw new InvalidOperationException("URP world render request is unsupported.");
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, request);
            RenderTexture.active = texture;
            image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            if (camera != null) camera.targetTexture = oldTarget;
            RenderTexture.active = oldActive;
            texture.Release();
            Destroy(texture);
            if (image != null) Destroy(image);
        }
    }

    private void CaptureLog(string message, string stack, LogType type)
    {
        if (_writingReport || message.StartsWith("[LoadingSmoke]", StringComparison.Ordinal)) return;
        if (message.Contains("EnemyGizmoDrawer") && message.Contains("missing")) return;
        bool tweenError = message.Contains("DOTWEEN") &&
            (message.Contains("error") || message.Contains("failed") || message.Contains("missing/null"));
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert && !tweenError) return;
        _errorCount++;
        if (_errors.Count < MaxCapturedErrors)
            _errors.Add(new { seconds = Time.realtimeSinceStartupAsDouble, type = type.ToString(), message, stack });
    }

    private void Finish(Exception error)
    {
        _failure = error?.ToString();
        _status = error == null && _errorCount == 0 ? "passed" : "failed";
        try { WriteReport(); }
        catch (Exception reportError) { UnityEngine.Debug.LogError("[LoadingSmoke] Report write failed: " + reportError.Message); }
        UnityEngine.Debug.Log("[LoadingSmoke] " + _status + ": " + _reportPath);
        if (_quitWhenDone)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                UnityEditor.SessionState.EraseBool("Soom.LoadingSmoke.Enabled");
                UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
#endif
            Application.Quit(_status == "passed" ? 0 : 1);
        }
    }

    private void WriteReport()
    {
        _writingReport = true;
        try
        {
            var report = new { status = _status, startedUtc = _startedUtc, updatedUtc = DateTime.UtcNow,
                elapsedSeconds = _elapsed.Elapsed.TotalSeconds, unityVersion = Application.unityVersion,
                reportPath = _reportPath, isolatedSavePath = _isolatedSavePath, failure = _failure,
                capturedErrorCount = _errorCount, steps = _steps, errors = _errors };
            File.WriteAllText(_reportPath, JsonConvert.SerializeObject(report, Formatting.Indented));
        }
        finally { _writingReport = false; }
    }

    private void OnDestroy() => Application.logMessageReceived -= CaptureLog;
}
