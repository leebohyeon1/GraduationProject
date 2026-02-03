using UnityEngine;

[CreateAssetMenu(fileName = "AttackRange", menuName = "Enemy/Strategy/Attack Range")]
public class AttackRange : EnemyUseAnything
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // 발사할 총알 프리팹
    public float projectileSpeed = 15f; // 총알 속도

    [Header("Spawn Settings")]
    public Vector3 spawnOffset = new Vector3(0, 1.0f, 0.5f); // 적의 중심에서 총알이 생성될 위치 오프셋
    public DamageData damageData; 

    // 블랙보드 키: 조준 방향을 저장하기 위함
    private const string KEY_ATTACK_DIR = "AttackRange_Direction";

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        var blackboard = runner._aiController._aiBrain.blackboard;

        // 1. [조준 단계] 플레이어 위치 확인 및 발사 방향 계산
        Vector3 playerPos = enemy.player.transform.position;
        Vector3 targetPos = playerPos + Vector3.up * 0.5f; 
        
        // 발사 시작 위치 (현재 기준)
        Vector3 spawnPos = enemy.transform.position + (enemy.transform.rotation * spawnOffset);
        
        // 방향 계산 (목표지점 - 시작지점)
        Vector3 dir = (targetPos - spawnPos).normalized;
        dir.y = 0; // 수평 발사 가정 (필요 시 제거)

        // 2. [저장] 계산된 방향을 블랙보드에 저장 (쏘지 않음)
        blackboard.SetValue(KEY_ATTACK_DIR, dir);
        Debug.Log("저장된 발사 방향: " + dir);
        // (선택) 조준하는 순간 적이 플레이어를 바라보게 하고 싶다면:
        enemy.transform.rotation = Quaternion.LookRotation(dir);

        return runner;
    }

    public bool Fire(Enemy runner)
    {

        var blackboard = runner._aiController._aiBrain.blackboard;

        // 저장된 조준 방향이 없으면 발사 불가
        if (!blackboard.HasKey(KEY_ATTACK_DIR)) return false;

        // 저장된 방향 가져오기
        Vector3 dir = blackboard.GetValue<Vector3>(KEY_ATTACK_DIR);

        // 현재 위치 기준으로 생성 위치 재계산 (애니메이션 중 적이 밀려났을 수 있으므로)
        Vector3 spawnPos = runner.transform.position + (runner.transform.rotation * spawnOffset);

        // 총알 생성
        if (projectilePrefab != null)
        {
            GameObject bulletObj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            EnemyProjectile projectileScript = bulletObj.GetComponent<EnemyProjectile>();
            
            if (projectileScript != null)
            {
                projectileScript.Setup(dir, projectileSpeed, runner.gameObject, damageData);
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
        // 상태 종료 시 데이터 정리
        var blackboard = runner._aiController._aiBrain.blackboard;
        blackboard.RemoveKey(KEY_ATTACK_DIR);
        
        return runner;
    }

    public override void Reset<T>(T runner)
    {
        
    }
}