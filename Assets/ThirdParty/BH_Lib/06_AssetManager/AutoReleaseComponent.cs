using System.Collections.Generic;
using UnityEngine;

namespace BH_Lib.AssetManager
{
    /// <summary>
    /// 게임오브젝트가 파괴될 때 연결된 에셋들을 자동으로 해제하는 컴포넌트
    /// </summary>
    public class AutoReleaseComponent : MonoBehaviour
    {
        private AssetManager _assetManager;
        private readonly HashSet<string> _managedAssets = new HashSet<string>();

        /// <summary>
        /// AssetManager 참조 초기화
        /// </summary>
        /// <param name="assetManager">에셋 관리자</param>
        public void Initialize(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        /// <summary>
        /// 관리할 에셋 키 등록
        /// </summary>
        /// <param name="key">에셋 키</param>
        public void RegisterAsset(string key)
        {
            _managedAssets.Add(key);
        }

        /// <summary>
        /// 특정 에셋 해제 및 관리 목록에서 제거
        /// </summary>
        /// <param name="key">해제할 에셋 키</param>
        public void ReleaseAsset(string key)
        {
            if (_managedAssets.Contains(key) && _assetManager != null)
            {
                _assetManager.ReleaseAsset(key);
                _managedAssets.Remove(key);
            }
        }

        /// <summary>
        /// 관리 중인 모든 에셋 해제
        /// </summary>
        public void ReleaseAllAssets()
        {
            if (_assetManager != null)
            {
                foreach (var key in _managedAssets)
                {
                    _assetManager.ReleaseAsset(key);
                }
            }
            _managedAssets.Clear();
        }

        /// <summary>
        /// 컴포넌트가 파괴될 때 자동으로 모든 에셋 해제
        /// </summary>
        private void OnDestroy()
        {
            if (_assetManager != null)
            {
                _assetManager.OnComponentDestroyed(this);
            }
        }
    }
}