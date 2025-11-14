using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "SelfDestructAttack", menuName = "SelfDestructAttack")]
public class SelfDestructAttack : EnemyUseAnything
{
    public List<Explosion> explosions;
    public int Stage;

    public override T OnEnter<T>(T enemy)
    {
        _isExplosionStarted = false;
        
        return enemy;
    }
    private bool _isExplosionStarted = false;
    public override T OnUpdate<T>(T enemy)
    {
        if (!_isExplosionStarted)
        {
            _isExplosionStarted = true;
            // _ = '는 반환된 Task를 무시하여 컴파일러 경고를 없애줍니다.
            _ = ExplosionTask(enemy);
        }
        return enemy;
    }
    private async Task ExplosionTask(Enemy enemy)
    {
        for (int i = 0; i < explosions.Count; i++)
        {
            if (Stage == explosions[i].Stage)
            {
                explosions[i].damageData.AttackerTransform = enemy.transform;
                // Task.Delay는 밀리초라서 1000을 곱해야함
                await Task.Delay((int)(explosions[i].ExplosionDelay * 1000));

                Vector3 explosionOrigin = enemy.transform.position;
                Collider[] hitColliders = Physics.OverlapSphere(explosionOrigin, explosions[i].ExplosionRadius);

                foreach (var col in hitColliders)
                {
                    if (col.gameObject == enemy.gameObject) continue;
                    if (col.TryGetComponent<IDamageable>(out IDamageable character))
                    {
                        character.TakeDamage(explosions[i].damageData);
                    }
                }
                if(explosions[i].ExplosionFeedback != null)
                {
                    explosions[i].ExplosionFeedback.transform.position = enemy.transform.position;
                    explosions[i].ExplosionFeedback.PlayFeedbacks();
                }
                if (!string.IsNullOrEmpty(explosions[i].MonsterName))
                {
                    DamageData damageData = explosions[i].damageData;
                    damageData.DamageAmount = enemy.EnemyHealth.Maxhealth;
                    enemy.EnemyHealth.TakeDamage(damageData);
                    await SpawnEnemies(explosions[i].MonsterName, 1, enemy.transform);
                }
            }
        }
    }
    public async Task SpawnEnemies(string monsterName, int count, Transform transform = default)
    {
        var spawnController = GameObject.FindObjectOfType<MonsterSpawnController>();
        if (spawnController == null)
        {
            spawnController = new GameObject().AddComponent<MonsterSpawnController>();
        }
        await spawnController.SpawnEnemies(monsterName, count, transform);
    }
}
[System.Serializable]
public class Explosion
{
    public int Stage;
    public float ExplosionDelay = 2f;
    public float ExplosionRadius = 5f;
    public DamageData damageData;
    public string MonsterName;
    public MMF_Player ExplosionFeedback;
}