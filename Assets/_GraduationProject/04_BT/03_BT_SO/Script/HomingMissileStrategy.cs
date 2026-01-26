using UnityEngine;
using Pathfinding;

[CreateAssetMenu(fileName = "SpawnHomingMissile", menuName = "Enemy/Strategy/Spawn Homing Missile (Yuumi Q)")]
public class SpawnHomingStrategy : EnemyUseAnything
{
    [Header("Prefab Settings")]
    public GameObject projectilePrefab; // HomingProjectile 스크립트가 붙은 프리팹
    public Vector3 spawnOffset = new Vector3(0, 1.5f, 0.5f); // 적 머리 위 등

    [Header("Projectile Stats")]
    public float damage = 10f;
    public float HomingDuration = 5.0f;       
    public float HomingStartSpeed = 5.0f;     
    public float HomingAcceleration = 5.0f;   
    public float HomingMaxSpeed = 20.0f;      
    public float TurningForce = 120.0f;       
    public float StraightSpeed = 30.0f;       
    
    [Header("Settings")]
    public LayerMask obstacleMask;            
    public DamageData damageData;

    // Blackboard Key to keep track of the spawned missile
    private const string KEY_PROJECTILE_INSTANCE = "SpawnedHomingMissile";

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        // 1. 적 이동 정지 (채널링 시작)
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
        }
        runner.Movement.StopMovement();
        
        // 적이 플레이어를 바라보게 함
        Vector3 dirToPlayer = (enemy.player.transform.position - enemy.transform.position).normalized;
        dirToPlayer.y = 0;
        enemy.transform.rotation = Quaternion.LookRotation(dirToPlayer);

        // 2. 발사체 소환
        Vector3 spawnPos = enemy.transform.position + enemy.transform.TransformDirection(spawnOffset);
        GameObject projObj = Instantiate(projectilePrefab, spawnPos, enemy.transform.rotation);
        
        // 3. 발사체 데이터 주입
        HomingProjectile projectileScript = projObj.GetComponent<HomingProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(
                enemy.player.transform,
                damageData,
                obstacleMask,
                enemy,
                HomingDuration,
                HomingStartSpeed,
                HomingAcceleration,
                HomingMaxSpeed,
                TurningForce,
                StraightSpeed
            );
        }

        // 4. 블랙보드에 발사체 저장 (이게 null이 될 때까지 대기하기 위함)
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_PROJECTILE_INSTANCE, projObj);

        // (옵션) 발사 애니메이션/이펙트 실행
        // enemy.Animator.SetTrigger("CastSkill");

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        
        // 1. 발사체가 존재하는지 확인
        GameObject projectile = enemy._aiController._aiBrain.blackboard.GetValue<GameObject>(KEY_PROJECTILE_INSTANCE);

        // 2. 발사체가 파괴(null)되었다면 행동 종료
        if (projectile == null)
        {
            // 투사체가 벽에 박거나 플레이어를 맞춰서 사라짐 -> AI 행동 끝
            StopChanneling(enemy);
            return runner; 
        }

        // 3. 발사체가 날아가는 동안 본체(Enemy)의 행동
        // 예: 계속 플레이어를 바라보며 서있기 (Channeling)
        Vector3 dirToPlayer = (enemy.player.transform.position - enemy.transform.position).normalized;
        dirToPlayer.y = 0;
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(dirToPlayer), Time.deltaTime * 5f);

        // *주의* 여기서는 runner를 반환하여 계속 Running 상태를 유지해야 함. 
        // 외부 Task Node에서 이 함수가 계속 실행되도록 해야 합니다.
        
        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        StopChanneling(enemy);

        // (선택 사항) 만약 적이 스턴에 걸려서 강제로 취소(Abort)된 경우,
        // 이미 날아가고 있는 미사일을 파괴할지, 그냥 날아가게 둘지 결정
        // 여기서는 "적이 정신을 잃으면 조종이 끊겨서 미사일도 터짐"으로 구현하려면:
        /*
        GameObject projectile = enemy._aiController._aiBrain.blackboard.GetValue<GameObject>(KEY_PROJECTILE_INSTANCE);
        if (projectile != null) Destroy(projectile);
        */

        return runner;
    }

    private void StopChanneling(Enemy enemy)
    {
        if (enemy == null) return;

        enemy._aiController._aiBrain.blackboard.SetValue(KEY_PROJECTILE_INSTANCE, null);

        // AI 복구
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = true;
            ai.isStopped = false;
            if (ai is AIPath aiPath) aiPath.enableRotation = true;
        }
    }
}