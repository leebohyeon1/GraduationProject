using UnityEngine;

[CreateAssetMenu(fileName = "AttackRange", menuName = "Enemy/Strategy/Attack Range")]
public class AttackRange : EnemyUseAnything
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; 
    public float projectileSpeed = 15f;

    [Header("Spawn Settings")]
    public Vector3 spawnOffset = new Vector3(0, 1.0f, 0.5f); 
    public DamageData damageData; 

    private const string KEY_ATTACK_DIR = "AttackRange_Direction";

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        var blackboard = runner._aiController._aiBrain.blackboard;

        Vector3 playerPos = enemy.player.transform.position;
        Vector3 targetPos = playerPos + Vector3.up * 0.5f; 
        
        Vector3 spawnPos = enemy.transform.position + (enemy.transform.rotation * spawnOffset);
        
        Vector3 dir = (targetPos - spawnPos).normalized;
        dir.y = 0; 

        blackboard.SetValue(KEY_ATTACK_DIR, dir);
        enemy.transform.rotation = Quaternion.LookRotation(dir);

        return runner;
    }

    public bool Fire(Enemy runner)
    {

        var blackboard = runner._aiController._aiBrain.blackboard;

        if (!blackboard.HasKey(KEY_ATTACK_DIR)) return false;

        Vector3 dir = blackboard.GetValue<Vector3>(KEY_ATTACK_DIR);

        Vector3 spawnPos = runner.transform.position + (runner.transform.rotation * spawnOffset);

        if (projectilePrefab != null)
        {
            GameObject bulletObj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            EnemyProjectile projectileScript = bulletObj.GetComponent<EnemyProjectile>();
            
            if (projectileScript != null)
            {
                projectileScript.Setup(runner,dir, projectileSpeed, runner.gameObject, damageData);
            }
        }

        return true;
    }
    public override T OnUpdate<T>(T runner)
    {
        if (runner.animHandler.IsHitWindowOpen)
        {
            Fire(runner);
            runner.animHandler.CloseHitWindow();
        }
        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;
        blackboard.RemoveKey(KEY_ATTACK_DIR);
        
        return runner;
    }

    public override void Reset<T>(T runner)
    {
        
    }
}
