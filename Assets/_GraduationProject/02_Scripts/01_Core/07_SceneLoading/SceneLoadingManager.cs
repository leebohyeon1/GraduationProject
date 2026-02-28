using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadingManager : MonoBehaviour
{
    // 어디서든 이 매니저를 부를 수 있게 싱글톤(Singleton)으로 만듭니다.
    public static SceneLoadingManager Instance;
    [SerializeField] private string _initializeSceneName;

    [Header("Loading UI")]
    [SerializeField] private CanvasGroup _loadingCanvasGroup; // 투명도(Alpha) 조절로 페이드 효과를 주기 위함
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TMP_Text _progressText;
    [SerializeField] private TMP_Text _tipText; // ScriptableObject에서 가져올 팁 텍스트
    [SerializeField] private Image _backgroundImage; // 배경 이미지

    [Header("Database")]
    // 1. 방금 만든 데이터베이스를 연결할 변수
    public SceneDatabase SceneDatabase;

    // 2. 이름을 검색하면 씬 데이터를 즉시 찾아주는 사전(Dictionary)
    private Dictionary<string, SceneDataSO> _sceneDataLookup = new Dictionary<string, SceneDataSO>();
    private Dictionary<string, SceneInstance> _loadedChunks = new Dictionary<string, SceneInstance>();

    // 중복 로딩 방지용 플래그
    private bool isTeleporting = false;
    public string CurrentActiveChunkName { get; private set; } = "";

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
        if(_initializeSceneName == "")
        {
            return;
        }

        TeleportToSceneByName(_initializeSceneName);
    }

    // =================================================================
    // 기능 1: 전체 텔레포트 (로딩 화면 띄우고 기존 맵 싹 지운 뒤 새 맵 로드)
    // =================================================================

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

    public void TeleportToScene(SceneDataSO targetScene)
    {
        if (isTeleporting)
        {
            return;
        }

        StartCoroutine(TeleportCoroutine(targetScene));
    }

    private IEnumerator TeleportCoroutine(SceneDataSO targetScene)
    {
        isTeleporting = true;

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
        float fadeTimer = 0f;
        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            _loadingCanvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeTimer / 0.5f);
            yield return null;
        }
        _loadingCanvasGroup.alpha = 1f;
        if (_progressBar != null)
        {
            _progressBar.value = 0f;
        }

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
            if (_progressBar != null) _progressBar.value = loadOp.PercentComplete;
            if (_progressText != null) _progressText.text = Mathf.RoundToInt(loadOp.PercentComplete * 100f) + "%";
            yield return null;
        }

        // 로드 완료 처리
        if (_progressBar != null)
        {
            _progressBar.value = 1f;
        }
        if (_progressText != null)
        {
            _progressText.text = "100%";
        }

        SceneInstance newSceneInstance = loadOp.Result;
        _loadedChunks.Add(targetScene.SceneName, newSceneInstance); // 새 씬 명부에 추가

        // 씬 활성화 및 메인 씬으로 세팅
        AsyncOperation activateOp = newSceneInstance.ActivateAsync();
        while (!activateOp.isDone)
        {
            yield return null;
        }
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

        // 4. 페이드 아웃 (화면 밝게)
        fadeTimer = 0f;
        while (fadeTimer < 0.5f)
        {
            fadeTimer += Time.deltaTime;
            _loadingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / 0.5f);
            yield return null;
        }
        _loadingCanvasGroup.alpha = 0f;
        _loadingCanvasGroup.blocksRaycasts = false;

        isTeleporting = false;
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
        string chunkName = chunkData.SceneName;

        // 이미 거기가 활성 씬이면 무시
        if (CurrentActiveChunkName == chunkName) return;

        // 명부(로드된 씬 목록)에 해당 씬이 있는지 확인
        if (_loadedChunks.TryGetValue(chunkName, out var sceneInstance))
        {
            // 1. 세이브용 이름표 갱신
            CurrentActiveChunkName = chunkName;

            // 2. 유니티 시스템상의 메인 씬(Active Scene) 교체! 
            // (이제 새로 생성되는 오브젝트나 조명 기준이 이 씬으로 바뀝니다)
            SceneManager.SetActiveScene(sceneInstance.Scene);

            // 3. (선택) 스카이박스나 조명도 이 구역의 것으로 부드럽게 교체
            if (chunkData.skyboxMaterial != null)
            {
                RenderSettings.skybox = chunkData.skyboxMaterial;
                DynamicGI.UpdateEnvironment();
            }

            Debug.Log($"[Level Streaming] 메인 씬이 '{chunkName}'(으)로 변경되었습니다!");
        }
    }
}
