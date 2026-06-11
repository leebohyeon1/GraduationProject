using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 몬스터 스폰을 제어하는 컨트롤러입니다. 오브젝트 풀을 활용하여 최적화된 스폰을 수행합니다.
/// </summary>
public class MonsterSpawnController : MonoBehaviour
{
    /// <summary>
    /// 지정된 개수만큼 몬스터를 비동기적으로 스폰합니다.
    /// </summary>
    /// <param name="monsterPrefabName">스폰할 몬스터의 Addressables 이름</param>
    /// <param name="count">스폰할 마릿수</param>
    /// <param name="spawnTransform">스폰 기준 트랜스폼 (null일 경우 zero)</param>
    /// <returns>Task</returns>
    public async Task SpawnEnemies(string monsterPrefabName, int count, Transform spawnTransform = default)
    {
        var spawnTasks = new List<Task<Enemy>>(); 
        Vector3 spawnCenterPosition = (spawnTransform != null) ? spawnTransform.position : Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            Vector3 position = new Vector3(spawnCenterPosition.x + i * 5, spawnCenterPosition.y, spawnCenterPosition.z);
            spawnTasks.Add(SpawnFromPoolAsync(monsterPrefabName, position, Quaternion.identity));
        }
        
        Debug.Log($"Requested {count} of {monsterPrefabName} from pool.");
        await Task.WhenAll(spawnTasks);
    }

    private async Task<Enemy> SpawnFromPoolAsync(string monsterPrefabName, Vector3 position, Quaternion rotation)
    {
        var enemy = await MonsterPoolManager.Instance.GetMonsterAsync<Enemy>(monsterPrefabName, position, rotation);
        enemy.MonsterPrefabName = monsterPrefabName;
        return enemy;
    }

    /// <summary>
    /// 살아있는 몬스터를 오브젝트 풀로 반납합니다.
    /// </summary>
    /// <param name="enemy">반납할 몬스터 인스턴스</param>
    public void ReturnMonster(Enemy enemy)
    {
        if (string.IsNullOrEmpty(enemy.MonsterPrefabName))
        {
            Debug.LogWarning($"Enemy {enemy.name} has no PoolKey. Destroying instead of returning to pool.");
            Destroy(enemy.gameObject);
            return;
        }
        MonsterPoolManager.Instance.ReleaseMonster(enemy.MonsterPrefabName, enemy);
    }
}
