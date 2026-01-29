using UnityEngine;

[CreateAssetMenu(fileName = "AttackRange", menuName = "Enemy/Strategy/Attack Range")]
public class AttackRange : EnemyUseAnything
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // 발사할 총알 프리팹 (EnemyProjectile 스크립트가 붙어있어야 함)
    public float projectileSpeed = 15f; // 총알 속도

    [Header("Spawn Settings")]
    public Vector3 spawnOffset = new Vector3(0, 1.0f, 0.5f); // 적의 중심에서 총알이 생성될 위치 오프셋
    public DamageData damageData; 
    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;


        Vector3 playerPos = enemy.player.transform.position;
        
        Vector3 targetPos = playerPos + Vector3.up * 0.5f; 
        
        Vector3 spawnPos = enemy.transform.position + (enemy.transform.rotation * spawnOffset);
        Vector3 dir = (targetPos - spawnPos).normalized;
        dir.y = 0;
        GameObject bulletObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        EnemyProjectile projectileScript = bulletObj.GetComponent<EnemyProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Setup(dir, projectileSpeed, enemy, damageData);
        }

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        return runner;
    }

    public override void Reset<T>(T runner)
    {
        
    }
}