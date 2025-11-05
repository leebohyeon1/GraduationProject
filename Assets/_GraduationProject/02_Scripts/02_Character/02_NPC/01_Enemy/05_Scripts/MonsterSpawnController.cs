using System.Collections.Generic;
using System.Threading.Tasks;
using BH_Lib.AssetManager;
using BH_Lib.DI;
using UnityEngine;

public class MonsterSpawnController:DIMonoBehaviour
{
    [Inject] private AssetManager _assetManager;

    public async Task SpawnEnemies(string Monster, int count)
    {
        var spawnTasks = new List<Task<Enemy>>(); 

        for (int i = 0; i < count; i++)
        {
            Vector3 position = new Vector3(i * 5, 0.5f, 5);
            
            spawnTasks.Add(SpawnSingleEnemyAsync<Enemy>(Monster, position, null));
        }

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
