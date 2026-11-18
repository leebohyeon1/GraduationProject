using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MoreMountains.Tools;

public class SceneLoadingManager : MonoBehaviour
{
    // 어디서든 이 매니저를 부를 수 있게 싱글톤(Singleton)으로 만듭니다.
    public static SceneLoadingManager Instance;

    [SerializeField] private bool _useInitialScene = true;
    [SerializeField] private SceneDataSO _initializeScene;
    public SceneDataSO InitializeScene => _initializeScene;

    [Header("Loading UI")]
    [SerializeField] private CanvasGroup _loadingCanvasGroup; // 투명도(Alpha) 조절로 페이드 효과를 주기 위함
    [SerializeField] private Image _progressImage;
    [SerializeField] private TMP_Text _tipText; // ScriptableObject에서 가져올 팁 텍스트
    [SerializeField] private Image _backgroundImage; // 배경 이미지

    [Header("Database")]
    // 1. 방금 만든 데이터베이스를 연결할 변수
    public SceneDatabase SceneDatabase;

    // 2. 이름을 검색하면 씬 데이터를 즉시 찾아주는 사전(Dictionary)
    private Dictionary<string, SceneDataSO> _sceneDataLookup = new Dictionary<string, SceneDataSO>();
    private readonly Dictionary<string, SceneDataSO> _persistentSceneData = new Dictionary<string, SceneDataSO>();
    private Dictionary<string, SceneInstance> _loadedChunks = new Dictionary<string, SceneInstance>();
    // Non-owning copies: the loaded scene owns these until a transition takes a
    // temporary reference. Never acquire/release the scene load handle itself.
    private readonly Dictionary<string, List<AsyncOperationHandle>> _chunkDependencies = new Dictionary<string, List<AsyncOperationHandle>>();
    private bool _retainTransitionDependencies = true;

    // 중복 로딩 방지용 플래그
    public bool IsTeleporting { get; private set; } = false;
    public SceneDataSO CurrentActiveChunk;

    public enum SpawnMode { Default, LastPosition, Custom }
    private SpawnMode _currentSpawnMode = SpawnMode.Default;
    private Vector3? _customSpawnPosition = null;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeDuration;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _retainTransitionDependencies = System.Array.IndexOf(System.Environment.GetCommandLineArgs(),
                "--soom-disable-transition-retention") < 0;

            // 게임이 켜질 때, 데이터베이스의 리스트를 Dictionary로 변환
            if (SceneDatabase != null)
            {
                foreach (var sceneData in SceneDatabase.AllScenes)
                {
                    if (!sceneData) continue;
                    _persistentSceneData[sceneData.name] = sceneData;
                    if (!_sceneDataLookup.ContainsKey(sceneData.SceneName))
                    {
                        _sceneDataLookup.Add(sceneData.SceneName, sceneData);
                    }
                }
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 시작 시 로딩 UI 숨기기
        if (_loadingCanvasGroup != null)
        {
            _loadingCanvasGroup.alpha = 0f;
            _loadingCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        if(_initializeScene == null || !_useInitialScene)
        {
            return;
        }

        if (_sceneDataLookup.TryGetValue(_initializeScene.SceneName, out SceneDataSO dataToLoad))
        {
            StartCoroutine(InitialTeleport(dataToLoad));
        }
    }

    // =================================================================
    // 기능 1: 전체 텔레포트 (로딩 화면 띄우고 기존 맵 싹 지운 뒤 새 맵 로드)
    // =================================================================

    public SceneDataSO GetSceneDataByName(string targetSceneName)
    {
        if (_sceneDataLookup.TryGetValue(targetSceneName, out SceneDataSO dataToLoad))
        {
            return dataToLoad;
        }
        return null;
    }

    public void TeleportToSceneByName(string targetSceneName)
    {
        // 사전에 해당 이름의 씬 데이터가 있는지 확인
        if (_sceneDataLookup.TryGetValue(targetSceneName, out SceneDataSO dataToLoad))
        {
            // 찾았다면 기존의 텔레포트 함수 실행!
            TeleportToScene(dataToLoad);
        }
        else
        {
            Debug.LogError($"[Scene Error] '{targetSceneName}' 씬 데이터를 찾을 수 없습니다! Database에 등록되었는지 확인하세요.");
        }
    }

    public void TeleportToSceneByName(string targetSceneName, SpawnMode spawnMode)
    {
        // 사전에 해당 이름의 씬 데이터가 있는지 확인
        if (_sceneDataLookup.TryGetValue(targetSceneName, out SceneDataSO dataToLoad))
        {
            // 찾았다면 기존의 텔레포트 함수 실행!
            TeleportToScene(dataToLoad, spawnMode);
        }
        else
        {
            Debug.LogError($"[Scene Error] '{targetSceneName}' 씬 데이터를 찾을 수 없습니다! Database에 등록되었는지 확인하세요.");
        }
    }

    public void TeleportToScene(SceneDataSO targetScene)
    {
        if (IsTeleporting)
        {
            return;
        }

        _currentSpawnMode = SpawnMode.Default;
        _customSpawnPosition = null;
        StartCoroutine(TeleportCoroutine(targetScene));
    }

    public void TeleportToScene(SceneDataSO targetScene, SpawnMode spawnMode)
    {
        if (IsTeleporting)
        {
            return;
        }

        _currentSpawnMode = spawnMode;
        _customSpawnPosition = null;
        StartCoroutine(TeleportCoroutine(targetScene));
    }

    public void TeleportToScene(SceneDataSO targetScene, Vector3 customPosition)
    {
        if (IsTeleporting)
        {
            return;
        }

        _currentSpawnMode = SpawnMode.Custom;
        _customSpawnPosition = customPosition;
        StartCoroutine(TeleportCoroutine(targetScene));
    }

    public string LastLoadError { get; private set; }
    private SceneDataSO _failedTarget;
    private bool _showLoadFailure;
    private SpawnMode _failedSpawnMode;
    private Vector3? _failedSpawnPosition;

    private IEnumerator InitialTeleport(SceneDataSO targetScene) => TeleportCoroutine(targetScene);

    // Drive the iterator here so exceptions cannot strand the loading overlay or lock.
    private IEnumerator TeleportCoroutine(SceneDataSO targetScene)
    {
        // A portal's bundled copy is destroyed when its source scene unloads.
        // Keep the CoreScene database instance instead. Asset names distinguish
        // Part2-A/B/C, which deliberately share the same displayed SceneName.
        string targetAssetName = targetScene ? targetScene.name : "<null>";
        targetScene = _persistentSceneData.TryGetValue(targetAssetName, out var persistentData) ? persistentData : null;
        IsTeleporting = true;
        LastLoadError = null;
        _showLoadFailure = false;
        var priority = Application.backgroundLoadingPriority;
        Application.backgroundLoadingPriority = ThreadPriority.High;
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        var routine = LoadSceneRoutine(targetScene);
        try
        {
            while (true)
            {
                bool next;
                object wait = null;
                try { next = routine.MoveNext(); if (next) wait = routine.Current; }
                catch (System.Exception error)
                {
                    LastLoadError = error.Message;
                    _failedTarget = targetScene;
                    _failedSpawnMode = _currentSpawnMode;
                    _failedSpawnPosition = _customSpawnPosition;
                    _showLoadFailure = true;
                    Debug.LogError($"[SceneLoading] Failed to load '{targetAssetName}' after {elapsed.Elapsed.TotalSeconds:F2}s: {error}");
                    break;
                }
                if (!next) break;
                yield return wait;
            }
        }
        finally
        {
            (routine as System.IDisposable)?.Dispose();
            IsTeleporting = false;
            Application.backgroundLoadingPriority = priority;
            if (_loadingCanvasGroup)
            {
                _loadingCanvasGroup.alpha = 0f;
                _loadingCanvasGroup.blocksRaycasts = false;
            }
            _currentSpawnMode = SpawnMode.Default;
            _customSpawnPosition = null;
            Debug.Log($"[SceneLoading] '{targetAssetName}' finished in {elapsed.Elapsed.TotalSeconds:F2}s; success={LastLoadError == null}.");
        }
    }

    private IEnumerator LoadSceneRoutine(SceneDataSO targetScene)
    {
        var phase = System.Diagnostics.Stopwatch.StartNew();
        if (!targetScene || targetScene.SceneReference == null || !targetScene.SceneReference.RuntimeKeyIsValid())
            throw new System.InvalidOperationException("Missing scene reference.");

        // Aliases A/B/C use one physical scene. Collect unused assets when changing
        // physical scenes (and on return to Title), while keeping save/retry fast.
        bool collectPreviousSceneAssets = _retainTransitionDependencies && CurrentActiveChunk &&
            (targetScene.name == "Title" || CurrentActiveChunk.SceneReference == null ||
             CurrentActiveChunk.SceneReference.AssetGUID != targetScene.SceneReference.AssetGUID);

        // Resolve the key before destroying the current playable scene.
        var locations = Addressables.LoadResourceLocationsAsync(targetScene.SceneReference.RuntimeKey, typeof(SceneInstance));
        try
        {
            while (!locations.IsDone) yield return null;
            if (locations.Status != AsyncOperationStatus.Succeeded || locations.Result == null || locations.Result.Count == 0)
                throw new System.InvalidOperationException("Scene is absent from the Addressables catalog.", locations.OperationException);
        }
        finally { if (locations.IsValid()) Addressables.Release(locations); }
        Debug.Log($"[SceneLoading] '{targetScene.name}' resolve: {phase.Elapsed.TotalSeconds:F2}s");

        if (_tipText) _tipText.text = targetScene.LoadingTip;
        if (_backgroundImage) _backgroundImage.sprite = targetScene.LoadingBackground;
        if (_progressImage) _progressImage.fillAmount = 0f;
        if (_loadingCanvasGroup) _loadingCanvasGroup.blocksRaycasts = true;
        float timer = 0f;
        while (timer < .5f)
        {
            timer += Time.unscaledDeltaTime;
            if (_loadingCanvasGroup) _loadingCanvasGroup.alpha = Mathf.Clamp01(timer / .5f);
            yield return null;
        }

        // Retain handles until checked. autoRelease=true can invalidate an earlier
        // operation while a later unload is still being polled.
        var chunks = new List<KeyValuePair<string, SceneInstance>>(_loadedChunks);
        var transitionDependencies = new List<AsyncOperationHandle>();
        try
        {
            if (_retainTransitionDependencies)
                AcquireTransitionDependencies(chunks, transitionDependencies);
            Debug.Log($"[SceneLoading] '{targetScene.name}' transition retention={_retainTransitionDependencies}; dependencyGroups={transitionDependencies.Count}");

            phase.Restart();
            // Restore the outgoing FEEL manager's original physics intervals
            // before its scene is removed. A new manager must not capture a
            // hit-stop interval as the next scene's normal fixed timestep.
            MoreMountains.Feedbacks.MMTimeScaleEvent.Reset();
            foreach (var chunk in chunks)
            {
                var unload = Addressables.UnloadSceneAsync(chunk.Value, false);
                try
                {
                    while (!unload.IsDone) yield return null;
                    if (unload.Status != AsyncOperationStatus.Succeeded)
                        throw new System.InvalidOperationException("Scene unload failed.", unload.OperationException);
                    _loadedChunks.Remove(chunk.Key);
                    _chunkDependencies.Remove(chunk.Key);
                }
                finally { if (unload.IsValid()) Addressables.Release(unload); }
            }
            CurrentActiveChunk = null;
            Debug.Log($"[SceneLoading] '{targetScene.name}' unload: {phase.Elapsed.TotalSeconds:F2}s");

            phase.Restart();
            var load = Addressables.LoadSceneAsync(targetScene.SceneReference, LoadSceneMode.Additive, true);
            bool retained = false;
            try
            {
                while (!load.IsDone)
                {
                    if (_progressImage) _progressImage.fillAmount = load.PercentComplete;
                    yield return null;
                }
                if (load.Status != AsyncOperationStatus.Succeeded || !load.Result.Scene.IsValid() || !load.Result.Scene.isLoaded)
                    throw new System.InvalidOperationException("Scene load failed.", load.OperationException);
                TrackChunkDependencies(targetScene.SceneName, load);
                _loadedChunks[targetScene.SceneName] = load.Result;
                retained = true;
                Debug.Log($"[SceneLoading] '{targetScene.name}' load: {phase.Elapsed.TotalSeconds:F2}s");
                phase.Restart();
                SetActiveChunk(targetScene);
                HandlePlayerSpawn(targetScene);
                if (DataManager.Instance && targetScene.SceneName != "Title") DataManager.Instance.SaveGame();
                if (_progressImage) _progressImage.fillAmount = 1f;
                Debug.Log($"[SceneLoading] '{targetScene.name}' setup: {phase.Elapsed.TotalSeconds:F2}s");
            }
            finally { if (!retained && load.IsValid()) Addressables.Release(load); }
        }
        finally
        {
            // The new scene now owns shared dependencies. Release our temporary
            // references also on failure/cancellation, before fading back to play.
            foreach (var dependency in transitionDependencies)
                if (dependency.IsValid()) Addressables.Release(dependency);
        }

        if (collectPreviousSceneAssets)
        {
            phase.Restart();
            long before = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            yield return Resources.UnloadUnusedAssets();
            long after = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            Debug.Log($"[SceneLoading] '{targetScene.name}' unused assets: {phase.Elapsed.TotalSeconds:F2}s; allocatedMB={before / 1048576}->{after / 1048576}");
        }

        timer = 0f;
        while (timer < _fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            if (_loadingCanvasGroup) _loadingCanvasGroup.alpha = 1f - Mathf.Clamp01(timer / _fadeDuration);
            yield return null;
        }
    }

    private void TrackChunkDependencies(string chunkName, AsyncOperationHandle<SceneInstance> load)
    {
        var dependencies = new List<AsyncOperationHandle>();
        load.GetDependencies(dependencies);
        _chunkDependencies[chunkName] = dependencies;
    }

    private void AcquireTransitionDependencies(List<KeyValuePair<string, SceneInstance>> chunks, List<AsyncOperationHandle> retained)
    {
        var seen = new HashSet<AsyncOperationHandle>();
        foreach (var chunk in chunks)
        {
            if (!_chunkDependencies.TryGetValue(chunk.Key, out var dependencies)) continue;
            foreach (var dependency in dependencies)
            {
                if (!dependency.IsValid() || !dependency.IsDone || dependency.Status != AsyncOperationStatus.Succeeded ||
                    dependency.Result is SceneInstance || !seen.Add(dependency)) continue;
                Addressables.ResourceManager.Acquire(dependency);
                retained.Add(dependency);
            }
        }
    }

    public void RetryFailedLoad()
    {
        if (!_showLoadFailure || IsTeleporting || !_failedTarget) return;
        if (_failedSpawnMode == SpawnMode.Custom && _failedSpawnPosition.HasValue)
            TeleportToScene(_failedTarget, _failedSpawnPosition.Value);
        else TeleportToScene(_failedTarget, _failedSpawnMode);
    }

    private void Update()
    {
        if (!_showLoadFailure || IsTeleporting) return;
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        var pad = UnityEngine.InputSystem.Gamepad.current;
        if ((keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame || keyboard.fKey.wasPressedThisFrame)) ||
            (pad != null && pad.buttonSouth.wasPressedThisFrame)) RetryFailedLoad();
        else if ((keyboard != null && keyboard.escapeKey.wasPressedThisFrame) ||
            (pad != null && pad.buttonEast.wasPressedThisFrame))
        {
            var title = GetSceneDataByName("Title");
            if (title) TeleportToScene(title);
        }
    }

    private void OnGUI()
    {
        if (!_showLoadFailure || IsTeleporting) return;
        // Recovery remains available even if the failed unload left no scene UI.
        var area = new Rect((Screen.width - 420) * .5f, (Screen.height - 160) * .5f, 420, 160);
        GUILayout.BeginArea(area, GUI.skin.box);
        string locale = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";
        string message = locale == "ko" ? "씬을 불러오지 못했습니다. 다시 시도해 주세요." :
            locale == "ja" ? "シーンを読み込めませんでした。再試行してください。" :
            locale.StartsWith("zh") ? "无法加载场景，请重试。" : "Unable to load the scene. Please retry.";
        string retry = locale == "ko" ? "재시도" : locale == "ja" ? "再試行" : locale.StartsWith("zh") ? "重试" : "Retry";
        string back = locale == "ko" ? "타이틀로 돌아가기" : locale == "ja" ? "タイトルに戻る" : locale.StartsWith("zh") ? "返回标题" : "Return to Title";
        GUILayout.Label(message);
        if (_failedTarget && GUILayout.Button(retry, GUILayout.Height(36)))
            RetryFailedLoad();
        var title = GetSceneDataByName("Title");
        if (title && GUILayout.Button(back, GUILayout.Height(36)))
            TeleportToScene(title);
        GUILayout.EndArea();
    }

    // =================================================================
    // 기능 2: 심리스 로드 (로딩 화면 없이 백그라운드에서 씬 덧붙이기)
    // =================================================================
    public void LoadChunkSeamless(SceneDataSO chunkData)
    {
        string chunkName = chunkData.SceneName;

        // 이미 로드된 씬이면 무시
        if (_loadedChunks.ContainsKey(chunkName)) return;

        Addressables.LoadSceneAsync(chunkData.SceneReference, LoadSceneMode.Additive).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // 로드되는 사이 다른 곳에서 로드했을 수도 있으니 더블 체크
                if (!_loadedChunks.ContainsKey(chunkName))
                {
                    TrackChunkDependencies(chunkName, handle);
                    _loadedChunks.Add(chunkName, handle.Result);
                    Debug.Log($"[Level Streaming] {chunkName} 로드 완료!");
                }
            }
        };
    }

    // =================================================================
    // 기능 3: 심리스 언로드 (로딩 화면 없이 백그라운드에서 씬 지우기)
    // =================================================================
    public void UnloadChunkSeamless(SceneDataSO chunkData)
    {
        string chunkName = chunkData.SceneName;

        // 로드되어 있는 씬일 때만 작동
        if (_loadedChunks.TryGetValue(chunkName, out SceneInstance sceneToUnload))
        {
            _loadedChunks.Remove(chunkName); // 중복 삭제 방지를 위해 명부에서 먼저 제거
            _chunkDependencies.Remove(chunkName);

            Addressables.UnloadSceneAsync(sceneToUnload).Completed += (handle) =>
            {
                Debug.Log($"[Level Streaming] {chunkName} 언로드 완료!");
            };
        }
    }

    public void SetActiveChunk(SceneDataSO chunkData)
    {
        if (chunkData == null) return;
        string chunkName = chunkData.SceneName;

        // 명부(로드된 씬 목록)에 해당 씬이 있는지 확인
        if (_loadedChunks.TryGetValue(chunkName, out var sceneInstance))
        {
            CurrentActiveChunk = chunkData;

            // 1. 유니티 시스템상의 메인 씬(Active Scene) 교체! 
            SceneManager.SetActiveScene(sceneInstance.Scene);

            // Successful path requests can produce thousands of lines per session.
            // Keep failures visible in release builds without per-path disk traffic.
            if (!Debug.isDebugBuild && AstarPath.active != null)
                AstarPath.active.logPathResults = Pathfinding.PathLog.OnlyErrors;

            // 2. 스카이박스나 조명 교체 (Title 씬이어도 시각적 요소는 갱신)
            if (chunkData.skyboxMaterial != null)
            {
                RenderSettings.skybox = chunkData.skyboxMaterial;
                DynamicGI.UpdateEnvironment();
            }

            CombatMusicController.Ensure().SetScene(chunkData);

            // 4. 타이틀 씬이 아닐 때만 게임 데이터 관련 갱신 수행
            if (chunkName != "Title")
            {
                if (DataManager.Instance != null && DataManager.Instance.GetGameData() != null)
                {
                    DataManager.Instance.GetGameData().LastMainScene = chunkName;
                }
            }

            Debug.Log($"[Level Streaming] 메인 씬이 '{chunkName}'(으)로 변경되었습니다 (시각 요소 갱신 포함).");
        }
    }

    private void HandlePlayerSpawn(SceneDataSO loadedSceneData)
    {
        var gameData = DataManager.Instance.GetGameData();
        if (gameData == null) return;

        string sceneName = loadedSceneData.SceneName;

        // 1. 플레이어 찾기 (비활성화된 오브젝트도 포함해서 찾기 위해 FindObjectOfType 사용)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            // FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include)는 최신 유니티에서 권장되지만,
            // 호환성을 위해 FindObjectsOfType<PlayerController>(true)로 비활성 오브젝트까지 검색
            PlayerController pc = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (pc != null) player = pc.gameObject;
        }

        if (player == null)
        {
            Debug.LogWarning("[Spawn] 플레이어 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // 2. 플레이어 활성화 (사망 시 비활성화되었을 수 있음)
        if (!player.activeSelf)
        {
            player.SetActive(true);
            Debug.Log("[Spawn] 비활성화된 플레이어를 다시 활성화했습니다.");
        }

        // 3. 위치 이동 및 컨트롤러 처리
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        try
        {
        switch (_currentSpawnMode)
        {
            case SpawnMode.Custom:
                if (_customSpawnPosition.HasValue)
                {
                    player.transform.position = _customSpawnPosition.Value;
                    Debug.Log($"[Spawn] 커스텀 지정 위치({_customSpawnPosition.Value})로 스폰합니다.");
                }
                break;

            case SpawnMode.LastPosition:
                player.transform.position = gameData.PlayerData.LastPosition;
                Debug.Log($"[Spawn] 마지막 저장 위치({gameData.PlayerData.LastPosition})로 스폰합니다.");
                break;

            case SpawnMode.Default:
            default:
                player.transform.position = loadedSceneData.DefaultSpawnPosition;
                player.transform.rotation = Quaternion.Euler(loadedSceneData.DefaultSpawnRotation);
                Debug.Log($"[Spawn] 기본 위치({loadedSceneData.DefaultSpawnPosition})로 스폰합니다.");
                break;
        }

        var checkpointScene = string.IsNullOrEmpty(gameData.PlayerData.RespawnSceneName) ? null : GetSceneDataByName(gameData.PlayerData.RespawnSceneName);
        if (loadedSceneData.name.StartsWith("Part2-") && checkpointScene && checkpointScene.name == "Part1")
        {
            gameData.PlayerData.MainRespawnSceneName = gameData.PlayerData.RespawnSceneName;
            gameData.PlayerData.MainRespawnPosition = gameData.PlayerData.RespawnPosition;
        }
        else if (loadedSceneData.name == "Part1" && checkpointScene && checkpointScene.name.StartsWith("Part2-") &&
                 !string.IsNullOrEmpty(gameData.PlayerData.MainRespawnSceneName))
        {
            gameData.PlayerData.RespawnSceneName = gameData.PlayerData.MainRespawnSceneName;
            gameData.PlayerData.RespawnPosition = gameData.PlayerData.MainRespawnPosition;
        }

        // The three side dungeons share one scene, so track the third entrance independently.
        const string thirdCheckpoint = "checkpoint:Part2-C";
        if (_currentSpawnMode == SpawnMode.Default && loadedSceneData.name == "Part2-C" && gameData.IsFirstVisit(thirdCheckpoint))
        {
            gameData.PlayerData.RespawnPosition = player.transform.position;
            gameData.PlayerData.RespawnSceneName = loadedSceneData.SceneName;
            gameData.MarkSceneAsVisited(thirdCheckpoint);
        }
        // 방문 기록 남기기
        if (gameData.IsFirstVisit(sceneName))
        {
            gameData.MarkSceneAsVisited(sceneName);
        }
        }
        finally
        {
            _customSpawnPosition = null;
            _currentSpawnMode = SpawnMode.Default;
            // Never leave movement disabled if checkpoint/spawn bookkeeping fails.
            if (cc != null) cc.enabled = true;
        }
    }
}
