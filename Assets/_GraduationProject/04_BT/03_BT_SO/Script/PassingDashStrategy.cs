using UnityEngine;
using Pathfinding;

[CreateAssetMenu(fileName = "PassingDashStrategy", menuName = "Enemy/Strategy/Passing Dash (Debug)")]
public class PassingDashStrategy : EnemyUseAnything
{
    [Header("Data Inspector")]
    public float DashSpeed = 15.0f;
    public float ExtraDist = 10.0f;     // 愿?????대룞 嫄곕━

    [Header("Settings")]
    public LayerMask obstacleMask;      // 踰??덉씠??
    public float arrivalThreshold = 0.5f;

    // 釉붾옓蹂대뱶 ??
    private const string KEY_DASH_TARGET_POS = "DashTargetPos";
    private const string KEY_DASH_START_POS = "DashStartPos"; // ?붾쾭源낆슜 ?쒖옉 ?꾩튂

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        // 1. AI 諛?臾쇰━ ?뺤?
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null) { ai.canMove = false; ai.isStopped = true; }
        
        runner.Movement.StopMovement();
        runner.aIPath.enableRotation = false;

        // 2. [?듭떖] 諛⑺뼢 諛?紐⑺몴 吏??怨꾩궛 (????踰덈쭔 ?ㅽ뻾!)
        Vector3 startPos = enemy.transform.position;
        Vector3 playerPos = enemy.player.transform.position;

        // Y異??믪씠 蹂댁젙 (?곸쓽 ?믪씠 湲곗?)
        float fixedY = startPos.y; 
        
        Vector3 direction = (playerPos - startPos);
        direction.y = 0; // ?됰㈃ 諛⑺뼢留??ъ슜

        // 嫄곕━媛 ?덈Т 媛源뚯슦硫??곸쓽 ?뺣㈃???ъ슜
        if (direction.sqrMagnitude < 0.1f) direction = enemy.transform.forward;
        else direction.Normalize();

        // **紐⑺몴 吏??= ?뚮젅?댁뼱 ?꾩튂 + (諛⑺뼢 * 異붽? 嫄곕━)**
        // ???꾩튂(startPos) 湲곗????꾨땲?? ?뚮젅?댁뼱(playerPos) 湲곗??쇰줈 ?ㅻ줈 ??媛????
        Vector3 finalTarget = playerPos + (direction * ExtraDist);
        finalTarget.y = fixedY; // ?믪씠 怨좎젙

        // 釉붾옓蹂대뱶 ???
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_DASH_TARGET_POS, finalTarget);
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_DASH_START_POS, startPos);

        // ?곸쓣 紐⑺몴 諛⑺뼢?쇰줈 ?뚯쟾
        enemy.transform.rotation = Quaternion.LookRotation(direction);

        // [?붾쾭洹? 紐⑺몴 吏??濡쒓렇 李띻린

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null) return runner;
        if(enemy._aiController._aiBrain.blackboard.HasKey(KEY_DASH_TARGET_POS) == false) return runner;

        // 1. 紐⑺몴 吏??媛?몄삤湲?
        Vector3 targetPos = enemy._aiController._aiBrain.blackboard.GetValue<Vector3>(KEY_DASH_TARGET_POS);
        Vector3 startPos = enemy._aiController._aiBrain.blackboard.GetValue<Vector3>(KEY_DASH_START_POS);
        Vector3 currentPos = enemy.transform.position;

        // ---------------------------------------------------------
        // [?쒓컖???붾쾭源? Scene 酉곗뿉???뺤씤?섏꽭??
        // 鍮④컙 ?? ?쒖옉??-> 紐⑺몴??(?꾩껜 寃쎈줈)
        Debug.DrawLine(startPos, targetPos, Color.red);
        // 珥덈줉 ?? ???꾩튂 -> 紐⑺몴??(?⑥? 寃쎈줈)
        Debug.DrawLine(currentPos, targetPos, Color.green);
        // ---------------------------------------------------------

        // 2. ?대룞 諛⑺뼢 踰≫꽣 (紐⑺몴 吏??- ???꾩튂)
        Vector3 moveDir = (targetPos - currentPos);
        moveDir.y = 0; // ?믪씠 臾댁떆
        float distToTarget = moveDir.magnitude;

        // 3. ?꾩갑 泥댄겕
        if (distToTarget <= arrivalThreshold)
        {
            // 紐⑺몴 ?꾨떖
            enemy.transform.position = targetPos; // 源붾걫?섍쾶 ?꾩튂 蹂댁젙
            StopDash(enemy);
            return runner;
        }

        // 4. ?대룞 ?ㅽ뻾
        moveDir.Normalize();
        float moveDistance = DashSpeed * Time.deltaTime;

        // 踰?泥댄겕 (紐몄껜 ?믪씠 1.0f 媛??
        if (Physics.Raycast(currentPos + Vector3.up * 1.0f, moveDir, moveDistance, obstacleMask))
        {
            StopDash(enemy);
            return runner;
        }

        // ?ㅼ젣 ?대룞
        enemy.transform.position += moveDir * moveDistance;

        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        // StopDash(runner as Enemy);
        return runner;
    }

    private void StopDash(Enemy enemy)
    {
        if (enemy == null) return;

        // 臾쇰━ 珥덇린??
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;
        enemy._aiController._aiBrain.blackboard.RemoveKey(KEY_DASH_TARGET_POS);
                    enemy._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                    enemy._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.LastAttackSuccessTime, Time.time);

        // AI 蹂듦뎄
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.Teleport(enemy.transform.position); // ?꾩옱 ?꾩튂瑜?AI?먭쾶 ?뚮┝
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = enemy.Movement._normalSpeed;
            if (ai is AIPath aiPath) aiPath.enableRotation = true;
        }
    }

    public override void Reset<T>(T runner)
    {
        
    }
}
