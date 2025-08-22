using UnityEngine;
using BH_Lib.DI;
using System;
using BH_Lib.Log;

namespace ProjectStudio.DI.Examples
{
    /// <summary>
    /// 씬 스코프 서비스의 인터페이스
    /// </summary>
    public interface ISceneService
    {
        string GetSceneName();
        void LogSceneMessage(string message);
    }

    /// <summary>
    /// 씬 스코프로 등록되는 서비스 구현
    /// 해당 씬이 언로드될 때 자동으로 정리됩니다.
    /// </summary>
    [Register(typeof(ISceneService), LifetimeScope.Scene)]
    public class SceneService : ISceneService, IDisposable
    {
        private readonly string _sceneName;
        private bool _isDisposed = false;

        public SceneService()
        {
            _sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Log.Print($"SceneService created for scene: {_sceneName}");
        }

        public string GetSceneName()
        {
            return _sceneName;
        }

        public void LogSceneMessage(string message)
        {
            Log.Print($"[{_sceneName}] {message}");
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Debug.Log($"SceneService for {_sceneName} is being disposed");
                _isDisposed = true;
                
                // 여기서 필요한 자원 정리 작업 수행
            }
        }
    }

    /// <summary>
    /// 씬 스코프 서비스를 사용하는 컴포넌트 예제
    /// </summary>
    public class SceneServiceConsumer : DIMonoBehaviour
    {
        [Inject]
        private ISceneService _sceneService;

        protected override void Awake()
        {
            base.Awake();
            Log.Print("SceneServiceConsumer initialized");
        }

        private void Start()
        {
            if (_sceneService != null)
            {
                _sceneService.LogSceneMessage("SceneServiceConsumer started!");
                Log.Print($"Current scene: {_sceneService.GetSceneName()}");
            }
            else
            {
                Log.PrintErr("Scene service not injected!");
            }
        }
    }

    /// <summary>
    /// 씬 전환 예제를 위한 유틸리티 클래스
    /// </summary>
    public class SceneExampleUtil : MonoBehaviour
    {
        [SerializeField]
        private string _nextSceneName;

        public void LoadNextScene()
        {
            if (!string.IsNullOrEmpty(_nextSceneName))
            {
                Log.Print($"Loading scene: {_nextSceneName}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(_nextSceneName);
            }
        }
    }
}
