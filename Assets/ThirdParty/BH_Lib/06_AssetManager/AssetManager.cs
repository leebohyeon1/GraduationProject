
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BH_Lib.AssetManager
{
    /// <summary>
    /// Addressable Asset System을 사용하는 에셋 관리 싱글톤 클래스
    /// </summary>
    public class AssetManager : MonoBehaviour
    {
        private static AssetManager _instance;
        public static AssetManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject(name: "@AssetManager");
                    _instance = go.AddComponent<AssetManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private readonly Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();
        private readonly Dictionary<string, int> _referenceCount = new Dictionary<string, int>();
        private readonly Dictionary<AutoReleaseComponent, HashSet<string>> _componentAssets = new Dictionary<AutoReleaseComponent, HashSet<string>>();

        /// <summary>
        /// 주소(key)를 이용해 에셋을 비동기적으로 로드합니다.
        /// </summary>
        /// <typeparam name="T">로드할 에셋 타입</typeparam>
        /// <param name="key">에셋의 Addressable 주소</param>
        /// <param name="owner">에셋을 소유할 게임오브젝트 (자동 해제용)</param>
        /// <returns>로드된 에셋</returns>
        public async Task<T> LoadAssetAsync<T>(string key, GameObject owner = null) where T : class
        {
            // 참조 카운트 증가
            if (_referenceCount.ContainsKey(key))
            {
                _referenceCount[key]++;
            }
            else
            {
                _referenceCount[key] = 1;
            }

            // 이미 성공적으로 로드된 에셋이 있으면 재사용
            if (_assetHandles.TryGetValue(key, out var existingHandle))
            {
                // 핸들이 유효한지 먼저 확인
                if (existingHandle.IsValid())
                {
                    try
                    {
                        // Status 체크를 try-catch로 보호
                        if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                        {
                            if (owner != null)
                            {
                                RegisterAssetToComponent(key, owner);
                            }
                            return existingHandle.Result as T;
                        }
                        else if (existingHandle.Status == AsyncOperationStatus.Failed)
                        {
                            // 실패한 핸들 제거
                            Debug.LogWarning($"기존 핸들이 실패 상태입니다. 다시 로드합니다: {key}");
                            Addressables.Release(existingHandle);
                            _assetHandles.Remove(key);
                        }
                        else
                        {
                            // 아직 로딩 중인 경우 대기
                            await existingHandle.Task;
                            if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                            {
                                if (owner != null)
                                {
                                    RegisterAssetToComponent(key, owner);
                                }
                                return existingHandle.Result as T;
                            }
                        }
                    }
                    catch (System.InvalidOperationException)
                    {
                        // 핸들이 무효한 상태가 되었을 경우
                        Debug.LogWarning($"핸들이 무효한 상태입니다. 다시 로드합니다: {key}");
                        _assetHandles.Remove(key);
                    }
                }
                else
                {
                    // 무효한 핸들 제거
                    Debug.LogWarning($"무효한 핸들을 제거합니다: {key}");
                    _assetHandles.Remove(key);
                }
            }

            var asyncOperationHandle = Addressables.LoadAssetAsync<T>(key);
            _assetHandles[key] = asyncOperationHandle;

            await asyncOperationHandle.Task;

            if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                if (owner != null)
                {
                    RegisterAssetToComponent(key, owner);
                }
                Debug.Log($"에셋 로드 성공: {key}");
                return asyncOperationHandle.Result;
            }
            else
            {
                Debug.LogError($"에셋 로드 실패: {key}, 에러: {asyncOperationHandle.OperationException}");
                _assetHandles.Remove(key);
                _referenceCount.Remove(key);
                
                // 실패한 핸들 해제
                if (asyncOperationHandle.IsValid())
                {
                    Addressables.Release(asyncOperationHandle);
                }
                
                return null;
            }
        }

        /// <summary>
        /// 주소(key)를 이용해 프리팹을 비동기적으로 인스턴스화합니다.
        /// </summary>
        /// <param name="key">프리팹의 Addressable 주소</param>
        /// <param name="parent">생성될 인스턴스의 부모 Transform</param>
        /// <returns>생성된 게임오브젝트</returns>
        public async Task<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            // 참조 카운트 증가
            if (_referenceCount.ContainsKey(key))
            {
                _referenceCount[key]++;
            }
            else
            {
                _referenceCount[key] = 1;
            }

            var asyncOperationHandle = Addressables.InstantiateAsync(key, parent);
            
            await asyncOperationHandle.Task;

            if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var newObject = asyncOperationHandle.Result;
                
                // 생성된 오브젝트에 AutoReleaseComponent 추가
                var autoRelease = newObject.AddComponent<AutoReleaseComponent>();
                autoRelease.Initialize(this);
                RegisterAssetToComponent(key, newObject);
                
                return newObject;
            }
            else
            {
                Debug.LogError($"프리팹 인스턴스화 실패: {key}, 에러: {asyncOperationHandle.OperationException}");
                _referenceCount.Remove(key);
                return null;
            }
        }

        /// <summary>
        /// 로드된 에셋 또는 인스턴스화된 게임오브젝트를 해제합니다.
        /// </summary>
        /// <param name="key">해제할 에셋의 Addressable 주소</param>
        public void ReleaseAsset(string key)
        {
            if (_referenceCount.ContainsKey(key))
            {
                _referenceCount[key]--;
                
                if (_referenceCount[key] <= 0)
                {
                    // 참조 카운트가 0이 되면 실제 해제
                    if (_assetHandles.TryGetValue(key, out var handle))
                    {
                        if (handle.IsValid())
                        {
                            Addressables.Release(handle);
                        }
                        _assetHandles.Remove(key);
                    }
                    
                    _referenceCount.Remove(key);
                    Debug.Log($"에셋 해제됨: {key}");
                }
            }
            else
            {
                Debug.LogWarning($"해제할 에셋을 찾을 수 없음: {key}");
            }
        }
        
        /// <summary>
        /// 인스턴스화된 게임오브젝트를 파괴하고 Addressable 참조를 해제합니다.
        /// </summary>
        /// <param name="gameObjectToRelease">해제할 게임오브젝트</param>
        public void ReleaseInstance(GameObject gameObjectToRelease)
        {
            if (gameObjectToRelease == null)
            {
                Debug.LogWarning("해제할 게임오브젝트가 null입니다.");
                return;
            }
            
            if (!Addressables.ReleaseInstance(gameObjectToRelease))
            {
                // Addressable로 생성된 인스턴스가 아닐 경우 GameObject.Destroy 사용
                Destroy(gameObjectToRelease);
                Debug.LogWarning($"'{gameObjectToRelease.name}'은 Addressable로 생성된 인스턴스가 아니므로 Destroy()로 제거합니다.");
            }
        }


        /// <summary>
        /// 컴포넌트에 에셋 등록
        /// </summary>
        /// <param name="key">에셋 키</param>
        /// <param name="owner">소유 게임오브젝트</param>
        private void RegisterAssetToComponent(string key, GameObject owner)
        {
            var autoRelease = owner.GetComponent<AutoReleaseComponent>();
            if (autoRelease == null)
            {
                autoRelease = owner.AddComponent<AutoReleaseComponent>();
                autoRelease.Initialize(this);
            }

            if (!_componentAssets.ContainsKey(autoRelease))
            {
                _componentAssets[autoRelease] = new HashSet<string>();
            }

            _componentAssets[autoRelease].Add(key);
            autoRelease.RegisterAsset(key);
        }

        /// <summary>
        /// 컴포넌트가 파괴될 때 호출되는 메서드
        /// </summary>
        /// <param name="component">파괴된 컴포넌트</param>
        public void OnComponentDestroyed(AutoReleaseComponent component)
        {
            if (_componentAssets.TryGetValue(component, out var assets))
            {
                foreach (var key in assets)
                {
                    ReleaseAsset(key);
                }
                _componentAssets.Remove(component);
            }
        }

        private void OnDestroy()
        {
            foreach (var handle in _assetHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _assetHandles.Clear();
            _referenceCount.Clear();
            _componentAssets.Clear();
            
            _instance = null;
        }
    }
}
