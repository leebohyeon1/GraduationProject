using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace BH_Lib.AssetManager
{
    /// <summary>
    /// 게임오브젝트가 파괴될 때 연결된 에셋과 씬을 자동으로 해제하는 컴포넌트
    /// </summary>
    public class AutoReleaseComponent : MonoBehaviour
    {
        private AssetManager _assetManager;
        
        // Owner 방식으로 관리되는 에셋 키 (참조 카운팅)
        private readonly HashSet<string> _managedAssetKeys = new HashSet<string>();
        // Handle 방식으로 관리되는 인스턴스 (InstantiateAsync)
        private readonly HashSet<AsyncOperationHandle> _managedInstanceHandles = new HashSet<AsyncOperationHandle>();
        // Handle 방식으로 관리되는 씬
        private readonly HashSet<AsyncOperationHandle<SceneInstance>> _managedSceneHandles = new HashSet<AsyncOperationHandle<SceneInstance>>();

        internal void Initialize(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        // Owner 방식: 참조 카운팅 에셋 등록
        internal void RegisterAssetKey(string key)
        {
            _managedAssetKeys.Add(key);
        }

        // Handle 방식: 인스턴스 핸들 등록
        internal void RegisterInstanceHandle(AsyncOperationHandle handle)
        {
            _managedInstanceHandles.Add(handle);
        }

        // Handle 방식: 씬 핸들 등록
        internal void RegisterSceneHandle(AsyncOperationHandle<SceneInstance> sceneHandle)
        {
            _managedSceneHandles.Add(sceneHandle);
        }

        private void OnDestroy()
        {
            if (_assetManager != null)
            {
                // 자신의 파괴를 AssetManager에게 알려 리소스 해제를 위임
                _assetManager.OnComponentDestroyed(_managedAssetKeys, _managedInstanceHandles, _managedSceneHandles);
            }
        }
    }
}
