using UnityEngine;

[CreateAssetMenu(fileName = "AttackRange", menuName = "Enemy/Strategy/Attack Range")]
public class AttackRange : EnemyUseAnything
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // 諛쒖궗??珥앹븣 ?꾨━??
    public float projectileSpeed = 15f; // 珥앹븣 ?띾룄

    [Header("Spawn Settings")]
    public Vector3 spawnOffset = new Vector3(0, 1.0f, 0.5f); // ?곸쓽 以묒떖?먯꽌 珥앹븣???앹꽦???꾩튂 ?ㅽ봽??
    public DamageData damageData; 

    // 釉붾옓蹂대뱶 ?? 議곗? 諛⑺뼢????ν븯湲??꾪븿
    private const string KEY_ATTACK_DIR = "AttackRange_Direction";

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        var blackboard = runner._aiController._aiBrain.blackboard;

        // 1. [議곗? ?④퀎] ?뚮젅?댁뼱 ?꾩튂 ?뺤씤 諛?諛쒖궗 諛⑺뼢 怨꾩궛
        Vector3 playerPos = enemy.player.transform.position;
        Vector3 targetPos = playerPos + Vector3.up * 0.5f; 
        
        // 諛쒖궗 ?쒖옉 ?꾩튂 (?꾩옱 湲곗?)
        Vector3 spawnPos = enemy.transform.position + (enemy.transform.rotation * spawnOffset);
        
        // 諛⑺뼢 怨꾩궛 (紐⑺몴吏??- ?쒖옉吏??
        Vector3 dir = (targetPos - spawnPos).normalized;
        dir.y = 0; // ?섑룊 諛쒖궗 媛??(?꾩슂 ???쒓굅)

        // 2. [??? 怨꾩궛??諛⑺뼢??釉붾옓蹂대뱶?????(?섏? ?딆쓬)
        blackboard.SetValue(KEY_ATTACK_DIR, dir);
        // (?좏깮) 議곗??섎뒗 ?쒓컙 ?곸씠 ?뚮젅?댁뼱瑜?諛붾씪蹂닿쾶 ?섍퀬 ?띕떎硫?
        enemy.transform.rotation = Quaternion.LookRotation(dir);

        return runner;
    }

    public bool Fire(Enemy runner)
    {

        var blackboard = runner._aiController._aiBrain.blackboard;

        // ??λ맂 議곗? 諛⑺뼢???놁쑝硫?諛쒖궗 遺덇?
        if (!blackboard.HasKey(KEY_ATTACK_DIR)) return false;

        // ??λ맂 諛⑺뼢 媛?몄삤湲?
        Vector3 dir = blackboard.GetValue<Vector3>(KEY_ATTACK_DIR);

        // ?꾩옱 ?꾩튂 湲곗??쇰줈 ?앹꽦 ?꾩튂 ?ш퀎??(?좊땲硫붿씠??以??곸씠 諛?ㅻ궗?????덉쑝誘濡?
        Vector3 spawnPos = runner.transform.position + (runner.transform.rotation * spawnOffset);

        // 珥앹븣 ?앹꽦
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
        // ?곹깭 醫낅즺 ???곗씠???뺣━
        var blackboard = runner._aiController._aiBrain.blackboard;
        blackboard.RemoveKey(KEY_ATTACK_DIR);
        
        return runner;
    }

    public override void Reset<T>(T runner)
    {
        
    }
}
