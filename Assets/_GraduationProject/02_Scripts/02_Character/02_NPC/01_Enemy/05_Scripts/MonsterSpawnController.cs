using System.Collections.Generic;
using System.Threading.Tasks;
using BH_Lib.AssetManager;
using BH_Lib.DI;
using UnityEngine;

public class MonsterSpawnController:DIMonoBehaviour
{
    [Inject] private AssetManager _assetManager;

    public async Task SpawnEnemies(string Monster, int count, Transform transform = default)
    {
        Debug.Log(1);
        var spawnTasks = new List<Task<Enemy>>(); 
        Vector3 spawnCenterPosition = (transform != null) ? transform.position : Vector3.zero;
        Debug.Log(2);
        for (int i = 0; i < count; i++)
        {
        Debug.Log(3);

            Vector3 position = new Vector3(spawnCenterPosition.x + i * 5, spawnCenterPosition.y, spawnCenterPosition.z);
        Debug.Log(4);
            
            spawnTasks.Add(SpawnSingleEnemyAsync<Enemy>(Monster, position, null));
        }
        Debug.Log($"Spawned {count} of {Monster}");
        await Task.WhenAll(spawnTasks);
    }


    private async Task<T> SpawnSingleEnemyAsync<T>(string MonsterPrefabName, Vector3 position, Transform parent) where T : Enemy
    {
        var prefabObject = await _assetManager.InstantiateAsync(MonsterPrefabName, parent);

        T enemy = prefabObject.GetComponent<T>();
        if (enemy == null)
        {
            enemy = prefabObject.AddComponent<T>();
        }

        if (parent != null)
        {
            prefabObject.transform.SetParent(parent);
        }

        prefabObject.transform.localScale = Vector3.one * enemy.transform.localScale.x;

        prefabObject.transform.position = position;

        return enemy;
    }

}
