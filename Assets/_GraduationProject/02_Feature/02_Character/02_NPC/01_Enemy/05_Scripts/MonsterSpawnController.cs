using UnityEngine;

public class MonsterSpawnController
{

    void SpawnEnemy(Enemy.MonsterName Monster, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Enemy.Spawn<Enemy>(null, Monster);
        }
    }
}
