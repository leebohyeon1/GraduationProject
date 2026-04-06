using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Addressables와 UnityEngine.Pool을 결합하여 몬스터 오브젝트 풀링을 관리하는 싱글톤 매니저입니다.
/// </summary>
public class MonsterPoolManager : MonoBehaviour
{
    private static MonsterPoolManager _instance;
    
    /// <summary>
    /// MonsterPoolManager의 전역 싱글톤 인스턴스입니다.
    /// </summary>
    public static MonsterPoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("MonsterPoolManager");
                _instance = go.AddComponent<MonsterPoolManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private Dictionary<string, ObjectPool<Enemy>> _pools = new Dictionary<string, ObjectPool<Enemy>>();
    private Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();

    /// <summary>
    /// 지정된 프리팹 이름으로 오브젝트 풀에서 몬스터를 비동기적으로 가져옵니다.
    /// </summary>
    /// <typeparam name="T">Enemy를 상속받는 컴포넌트 타입</typeparam>
    /// <param name="monsterPrefabName">Addressables 어드레스 이름</param>
    /// <param name="position">생성 위치</param>
    /// <param name="rotation">생성 회전</param>
    /// <param name="parent">부모 트랜스폼</param>
    /// <returns>가져온 몬스터 컴포넌트 인스턴스</returns>
    public async Task<T> GetMonsterAsync<T>(string monsterPrefabName, Vector3 position, Quaternion rotation, Transform parent = null) where T : Enemy
    {
        if (!_pools.ContainsKey(monsterPrefabName))
        {
            await CreatePoolAsync(monsterPrefabName);
        }

        var enemy = _pools[monsterPrefabName].Get();
        enemy.transform.SetParent(parent);
        enemy.transform.position = position;
        enemy.transform.rotation = rotation;
        
        // 풀에서 꺼낼 때 상태 초기화
        enemy.Init(); 
        
        return enemy as T;
    }

    private async Task CreatePoolAsync(string monsterPrefabName)
    {
        if (_prefabHandles.ContainsKey(monsterPrefabName)) return;

        var handle = Addressables.LoadAssetAsync<GameObject>(monsterPrefabName);
        _prefabHandles[monsterPrefabName] = handle;
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load monster prefab: {monsterPrefabName}");
            return;
        }

        GameObject prefab = handle.Result;

        var pool = new ObjectPool<Enemy>(
            createFunc: () => {
                GameObject obj = Instantiate(prefab);
                Enemy enemy = obj.GetComponent<Enemy>();
                if (enemy == null) enemy = obj.AddComponent<Enemy>();
                return enemy;
            },
            actionOnGet: (enemy) => {
                enemy.gameObject.SetActive(true);
            },
            actionOnRelease: (enemy) => {
                enemy.gameObject.SetActive(false);
            },
            actionOnDestroy: (enemy) => {
                Destroy(enemy.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
        );

        _pools[monsterPrefabName] = pool;
    }

    /// <summary>
    /// 사용이 끝난 몬스터를 해당 오브젝트 풀로 반납합니다.
    /// </summary>
    /// <param name="monsterPrefabName">Addressables 어드레스 이름</param>
    /// <param name="enemy">반납할 몬스터 인스턴스</param>
    public void ReleaseMonster(string monsterPrefabName, Enemy enemy)
    {
        if (_pools.TryGetValue(monsterPrefabName, out var pool))
        {
            pool.Release(enemy);
        }
        else
        {
            Destroy(enemy.gameObject);
        }
    }

    private void OnDestroy()
    {
        foreach (var handle in _prefabHandles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
    }
}
