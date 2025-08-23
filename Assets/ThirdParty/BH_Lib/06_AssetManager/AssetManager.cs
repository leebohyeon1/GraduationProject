using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace BH_Lib.AssetManager
{
    using Log;
    using DI;

    /// <summary>
    /// Addressable Asset System을 사용하는 에셋 및 씬 관리 클래스
    /// </summary>
    [Register(LifetimeScope.Singleton)]
    public class AssetManager : MonoBehaviour
    {
        private readonly Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();
        private readonly Dictionary<string, int> _referenceCount = new Dictionary<string, int>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        #region Public API

        /// <summary>
        /// (Owner 방식) 주소를 이용해 에셋을 비동기적으로 로드합니다.
        /// owner가 파괴될 때 참조 카운트를 감소시켜 자동으로 에셋을 관리합니다.
        /// </summary>
        /// <param name="key">에셋의 Addressable 주소</param>
        /// <param name="owner">에셋의 생명주기를 연결할 게임오브젝트</param>
        /// <returns>로드된 에셋</returns>
        public async Task<T> LoadAssetAsync<T>(string key, GameObject owner) where T : class
        {
            if (owner == null)
            {
                Log.PrintErr("Owner-based LoadAssetAsync는 owner 파라미터가 반드시 필요합니다.");
                return null;
            }

            if (_referenceCount.ContainsKey(key))
            {
                _referenceCount[key]++;
            }
            else
            {
                _referenceCount[key] = 1;
            }

            var autoRelease = owner.GetComponent<AutoReleaseComponent>() ?? owner.AddComponent<AutoReleaseComponent>();
            autoRelease.Initialize(this);
            autoRelease.RegisterAssetKey(key);

            if (_assetHandles.TryGetValue(key, out var existingHandle) && existingHandle.IsValid())
            {
                if (existingHandle.Status == AsyncOperationStatus.Succeeded) return existingHandle.Result as T;
                await existingHandle.Task;
                return existingHandle.Status == AsyncOperationStatus.Succeeded ? existingHandle.Result as T : null;
            }

            var asyncOperationHandle = Addressables.LoadAssetAsync<T>(key);
            _assetHandles[key] = asyncOperationHandle;
            await asyncOperationHandle.Task;

            if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return asyncOperationHandle.Result;
            }
            else
            {
                Log.PrintErr($"에셋 로드 실패: {key}, 에러: {asyncOperationHandle.OperationException}");
                ReleaseAsset(key); // 실패 시 참조 카운트 롤백
                return null;
            }
        }

        /// <summary>
        /// (Handle 방식) 프리팹을 인스턴스화합니다. 생성된 인스턴스가 스스로 생명주기를 관리합니다.
        /// </summary>
        /// <param name="key">프리팹의 Addressable 주소</param>
        /// <param name="parent">생성될 인스턴스의 부모 Transform</param>
        /// <returns>생성된 게임오브젝트. 실패 시 null.</returns>
        public async Task<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            var handle = Addressables.InstantiateAsync(key, parent);
            var instance = await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var autoRelease = instance.AddComponent<AutoReleaseComponent>();
                autoRelease.Initialize(this);
                autoRelease.RegisterInstanceHandle(handle);
                return instance;
            }
            else
            {
                Log.PrintErr($"프리팹 인스턴스화 실패: {key}, 에러: {handle.OperationException}");
                return null;
            }
        }

        /// <summary>
        /// (Handle 방식) 씬을 로드하고, 생명주기를 관리하는 핸들 게임오브젝트를 반환합니다.
        /// </summary>
        /// <param name="key">씬의 Addressable 주소</param>
        /// <param name="loadMode">씬 로드 모드</param>
        /// <returns>씬의 생명주기를 제어하는 핸들 게임오브젝트. 로드 실패 시 null.</returns>
        public async Task<GameObject> LoadSceneAsHandleAsync(string key, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            var sceneHandle = Addressables.LoadSceneAsync(key, loadMode, false);
            var sceneInstance = await sceneHandle.Task;

            if (sceneHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject handle = CreateHandleObject($"Handle: Scene - {key}");
                var autoRelease = handle.AddComponent<AutoReleaseComponent>();
                autoRelease.Initialize(this);
                autoRelease.RegisterSceneHandle(sceneHandle);
                await sceneInstance.ActivateAsync(); // 수동 활성화
                return handle;
            }
            else
            {
                Log.PrintErr($"씬 로드 실패: {key}, 에러: {sceneHandle.OperationException}");
                return null;
            }
        }

        #endregion

        #region Internal Logic & Callbacks

        internal void OnComponentDestroyed(HashSet<string> managedAssetKeys, HashSet<AsyncOperationHandle> managedInstanceHandles, HashSet<AsyncOperationHandle<SceneInstance>> managedSceneHandles)
        {
            foreach (var key in managedAssetKeys)
            {
                ReleaseAsset(key);
            }
            foreach (var handle in managedInstanceHandles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            foreach (var sceneHandle in managedSceneHandles)
            {
                if (sceneHandle.IsValid()) Addressables.UnloadSceneAsync(sceneHandle);
            }
        }

        private void ReleaseAsset(string key)
        {
            if (!_referenceCount.ContainsKey(key)) return;

            _referenceCount[key]--;
            if (_referenceCount[key] <= 0)
            {
                if (_assetHandles.TryGetValue(key, out var handle) && handle.IsValid())
                {
                    Addressables.Release(handle);
                    _assetHandles.Remove(key);
                }
                _referenceCount.Remove(key);
                Log.Print($"참조 카운트 0, 에셋 해제됨: {key}");
            }
        }

        private GameObject CreateHandleObject(string name)
        {
            var handle = new GameObject(name);
            handle.transform.SetParent(this.transform);
            return handle;
        }

        #endregion

        private void OnDestroy()
        {
            foreach (var handle in _assetHandles.Values)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _assetHandles.Clear();
            _referenceCount.Clear();
            Log.Print("AssetManager 파괴, 모든 참조 카운팅 에셋을 해제합니다.");
        }
    }
}
