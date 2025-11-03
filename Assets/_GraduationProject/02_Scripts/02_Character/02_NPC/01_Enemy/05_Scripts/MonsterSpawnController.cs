using UnityEngine;

public class MonsterSpawnController
{

    public void SpawnEnemy(Enemy.MonsterName Monster, int count)
    {
        
        for (int i = 0; i < count; i++)
        {
            Enemy.Spawn<Enemy>(null, Monster, new Vector3(i * 5, 0.5f, 5));
        }
    }
}
