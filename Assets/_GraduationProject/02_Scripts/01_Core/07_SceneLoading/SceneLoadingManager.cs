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
    private Dictionary<string, SceneInstance> _loadedChunks = new Dictionary<string, SceneInstance>();

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

            // 게임이 켜질 때, 데이터베이스의 리스트를 Dictionary로 변환
            if (SceneDatabase != null)
            {
                foreach (var sceneData in SceneDatabase.AllScenes)
                {
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

    private IEnumerator TeleportCoroutine(SceneDataSO targetScene)
    {
        IsTeleporting = true;
        Application.backgroundLoadingPriority = ThreadPriority.High;

        // 1. UI 세팅 및 페이드 인 (화면 까맣게)
        if (_tipText != null)
        {
            _tipText.text = targetScene.LoadingTip;
        }

        if (_backgroundImage != null && targetScene.LoadingBackground != null)
        {
            _backgroundImage.sprite = targetScene.LoadingBackground;
        }

        _loadingCanvasGroup.blocksRaycasts = true;

        _loadingCanvasGroup.alpha = 1f;
        if (_progressImage != null)
        {
            _progressImage.fillAmount = 0f;
        }

        float fadeTimer = 0f;
        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            _loadingCanvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeTimer / 0.5f);
            yield return null;
        }


        yield return new WaitForSeconds(0.2f);

        // 2. 기존에 로드된 모든 씬 언로드 (메모리 비우기)
        List<AsyncOperationHandle> unloadOps = new List<AsyncOperationHandle>();
        foreach (var chunk in _loadedChunks.Values)
        {
            unloadOps.Add(Addressables.UnloadSceneAsync(chunk));
        }

        foreach (var op in unloadOps)
        {
            while (!op.IsDone) yield return null;
        }
        _loadedChunks.Clear(); // 명부 초기화

        // 3. 새로운 타겟 씬 로드 (Additive)
        AsyncOperationHandle<SceneInstance> loadOp = Addressables.LoadSceneAsync(targetScene.SceneReference, LoadSceneMode.Additive, false);

        while (!loadOp.IsDone)
        {
            if (_progressImage != null)
            {
                _progressImage.fillAmount = loadOp.PercentComplete;
            }

            yield return null;
        }

        // 로드 완료 처리
        if (_progressImage != null)
        {
            _progressImage.fillAmount = 1f;
        }

        SceneInstance newSceneInstance = loadOp.Result;
        _loadedChunks.Add(targetScene.SceneName, newSceneInstance); // 새 씬 명부에 추가

        // 씬 활성화 및 메인 씬으로 세팅
        AsyncOperation activateOp = newSceneInstance.ActivateAsync();
        while (!activateOp.isDone)
        {
            yield return null;
        }

        SetActiveChunk(targetScene);
        SceneManager.SetActiveScene(newSceneInstance.Scene);

        // ==========================================================
        // 스카이박스 및 환경광(Ambient Light) 즉시 교체
        // ==========================================================
        if (targetScene.skyboxMaterial != null)
        {
            // 1. 스카이박스 머티리얼 교체
            RenderSettings.skybox = targetScene.skyboxMaterial;

            // 2. 바뀐 스카이박스에 맞춰서 환경광(주변 빛) 다시 계산
            DynamicGI.UpdateEnvironment();
        }
        // ==========================================================

        // 플레이어 스폰 위치 처리
        HandlePlayerSpawn(targetScene);

        // 오토 세이브: 씬 이동을 완료하고 플레이어가 배치된 시점의 데이터를 확실하게 저장합니다.
        if (DataManager.Instance != null && targetScene.SceneName != "Title")
        {
            DataManager.Instance.SaveGame();
        }

        // 4. 페이드 아웃 (화면 밝게)
        fadeTimer = 0f;
        while (fadeTimer < _fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            _loadingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / _fadeDuration);
            yield return null;
        }
        _loadingCanvasGroup.alpha = 0f;
        _loadingCanvasGroup.blocksRaycasts = false;

        IsTeleporting = false;
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
    }

    private IEnumerator InitialTeleport(SceneDataSO targetScene)
    {
        IsTeleporting = true;

        // 2. 기존에 로드된 모든 씬 언로드 (메모리 비우기)
        List<AsyncOperationHandle> unloadOps = new List<AsyncOperationHandle>();
        foreach (var chunk in _loadedChunks.Values)
        {
            unloadOps.Add(Addressables.UnloadSceneAsync(chunk));
        }

        foreach (var op in unloadOps)
        {
            while (!op.IsDone)
            {
                yield return null;
            }
        }
        _loadedChunks.Clear(); // 명부 초기화

        // 3. 새로운 타겟 씬 로드 (Additive)
        AsyncOperationHandle<SceneInstance> loadOp = Addressables.LoadSceneAsync(targetScene.SceneReference, LoadSceneMode.Additive, false);

        while (!loadOp.IsDone)
        {
            yield return null;
        }

        SceneInstance newSceneInstance = loadOp.Result;
        _loadedChunks.Add(targetScene.SceneName, newSceneInstance); // 새 씬 명부에 추가

        // 씬 활성화 및 메인 씬으로 세팅
        AsyncOperation activateOp = newSceneInstance.ActivateAsync();
        while (!activateOp.isDone)
        {
            yield return null;
        }

        SetActiveChunk(targetScene);

        // 플레이어 스폰 위치 처리
        HandlePlayerSpawn(targetScene);

        // 오토 세이브: 씬 이동을 완료하고 플레이어가 배치된 시점의 데이터를 확실하게 저장합니다.
        if (DataManager.Instance != null && targetScene.SceneName != "Title")
        {
            DataManager.Instance.SaveGame();
        }

        IsTeleporting = false;
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

            // 2. 스카이박스나 조명 교체 (Title 씬이어도 시각적 요소는 갱신)
            if (chunkData.skyboxMaterial != null)
            {
                RenderSettings.skybox = chunkData.skyboxMaterial;
                DynamicGI.UpdateEnvironment();
            }

            // 3. 배경음악(BGM) 자동 재생
            if (chunkData.BackgroundMusic != null)
            {
                MMSoundManager.Instance.PlaySound(
                    chunkData.BackgroundMusic, 
                    MMSoundManager.MMSoundManagerTracks.Music, 
                    Vector3.zero, 
                    true
                );
            }
            else
            {
                // 브금이 지정되지 않은 씬이라면 음악 정지 (필요에 따라 주석 처리)
                MMSoundManager.Instance.StopTrack(MMSoundManager.MMSoundManagerTracks.Music);
            }

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

        // 방문 기록 남기기
        if (gameData.IsFirstVisit(sceneName))
        {
            gameData.MarkSceneAsVisited(sceneName);
        }

        // 초기화
        _customSpawnPosition = null;
        _currentSpawnMode = SpawnMode.Default;

        // 4. 컨트롤러 다시 활성화 (이동이 끝난 후)
        if (cc != null) cc.enabled = true;
    }
}
