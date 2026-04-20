using UnityEngine;
using Pathfinding;
using UnityEngine.XR;

[CreateAssetMenu(fileName = "RushToFixedLocation", menuName = "Enemy/Strategy/Rush To Fixed Location")]
public class RushToFixedLocationStrategy : EnemyUseAnything
{
    [Header("Rush Settings")]
    public float rushSpeed = 20f;       // 湲곕낯 ?뚯쭊 ?띾룄 (怨≪꽑??Y媛믪씠 1???뚯쓽 ?띾룄)
    public float hitRadius = 1.5f;      // ?뚮젅?댁뼱 ?묒큺 ?먯젙 踰붿쐞
    public float overshootDist = 3.0f;  // 紐⑺몴 ?ㅻ쾭?덊듃 嫄곕━
    public LayerMask obstacleMask;      // 踰??덉씠??

    [Header("Speed Curve Settings")]
    public float rushDuration = 1.0f;   // ?뚯쭊??吏?띾맆 珥??쒓컙 (珥?
    // X異? 0~1 (?쒓컙 鍮꾩쑉), Y異? ?띾룄 諛곗쑉 (?? 0?먯꽌 ?쒖옉?댁꽌 1濡?媛붾떎媛 0?쇰줈 ?⑥뼱吏?
    public AnimationCurve rushCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.5f), new Keyframe(1, 0)); 
    public float turnSpeed = 10f;      // ?뚯쟾 ?띾룄 (??珥?
    // 釉붾옓蹂대뱶 ??
    private const string KEY_RUSH_DEST = "RushDestination";
    private const string KEY_RUSHBOOL = "RushBool";
    private const string KEY_RUSH_START_TIME = "RushStartTime"; // [異붽?] ?쒖옉 ?쒓컙 ??μ슜

    public override T OnEnter<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;
        if(!blackboard.GetValueOrDefault<bool>(KEY_RUSHBOOL, true))
        {
            return runner; 
        }
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        IAstarAI ai = enemy.GetComponent<IAstarAI>();

        // 1. A* ?ㅻ퉬寃뚯씠???꾧린
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true; 
        }

        // 2. 紐⑺몴 吏??怨꾩궛
        Vector3 playerPos = enemy.player.transform.position;
        Vector3 myPos = enemy.transform.position;

        Vector3 dir = (playerPos - myPos);
        dir.y = 0; 
        if (dir == Vector3.zero) dir = enemy.transform.forward;
        dir.Normalize();

        Vector3 offset = Quaternion.Euler(0, Random.Range(0, 360), 0) * new Vector3(0.5f, 0, 0);
        Vector3 finalDestination = playerPos + (dir * overshootDist) + offset;

        // 3. 釉붾옓蹂대뱶 ?곗씠???ㅼ젙
        blackboard.SetValue(KEY_RUSH_DEST, finalDestination);
        blackboard.SetValue(KEY_RUSHBOOL, false);
        
        // [異붽?] ?쒖옉 ?쒓컙 湲곕줉 (怨≪꽑 怨꾩궛???꾪빐 ?꾩슂)
        blackboard.SetValue(KEY_RUSH_START_TIME, Time.time);
        
        runner.aIPath.enableRotation = false;

        runner.Movement.StopMovement();
        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        if(runner._aiController._aiBrain.blackboard.GetValue<bool>(KEY_RUSHBOOL))
        {
            return runner; 
        }
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;
        if (!enemy._aiController._aiBrain.blackboard.GetValue<Vector3>(KEY_RUSH_DEST, out Vector3 targetPos))
        {
            return runner; 
        }
        // [異붽?] ?쒓컙 寃쎄낵???곕Ⅸ ?띾룄 怨꾩궛
        float startTime = enemy._aiController._aiBrain.blackboard.GetValue<float>(KEY_RUSH_START_TIME);
        float elapsedTime = Time.time - startTime;      // 寃쎄낵 ?쒓컙
        float normalizedTime = elapsedTime / rushDuration; // 0.0 ~ 1.0 ?ъ씠 媛믪쑝濡??뺢퇋??

        // ?쒓컙?????섎㈃ 醫낅즺
        if (normalizedTime >= 1.0f)
        {
            StopRush(enemy);
            return runner;
        }

        // AnimationCurve?먯꽌 ?꾩옱 ?쒓컙???띾룄 諛곗쑉??媛?몄샂
        float speedMultiplier = rushCurve.Evaluate(normalizedTime);
        float currentSpeed = rushSpeed * speedMultiplier; // 理쒖쥌 ?띾룄 = 湲곕낯 ?띾룄 * 諛곗쑉

        // 1. [吏곸젒 ?대룞] 媛蹂 ?띾룄 ?곸슜
        float step = currentSpeed * Time.deltaTime;
        Vector3 currentPos = enemy.transform.position;
        
        // 紐⑺몴 諛⑺뼢?쇰줈 ?대룞
        Vector3 nextPos = Vector3.MoveTowards(currentPos, targetPos, step);
        
        // [踰?泥댄겕] (湲곗〈 濡쒖쭅 ?좎?)
        Vector3 moveDir = (nextPos - currentPos).normalized;
        moveDir.y = 0; // ?믪씠 李⑥씠 臾댁떆 (?됱? ?대룞 ??

        // if (moveDir != Vector3.zero)
        // {
        //     Quaternion targetRot = Quaternion.LookRotation(moveDir);
        //     // ?뚯쭊 以묒뿉??議곌툑 ??鍮좊Ⅴ寃??뚯쟾?댁꽌 諛⑺뼢???〓룄濡?蹂댁젙 (turnSpeed * 2f ??議곗젅 媛??
        //     enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRot, turnSpeed * Time.deltaTime * 5f);
        // }
        float moveDist = Vector3.Distance(currentPos, nextPos);

        // ?대룞 嫄곕━媛 ?꾩＜ ?묒쑝硫??띾룄媛 0??援ш컙 ?? ?덉씠罹먯뒪???앸왂 媛??
        if (moveDist > 0.0001f)
        {
            if (!Physics.Raycast(currentPos + Vector3.up * 0.5f, moveDir, moveDist, obstacleMask))
            {
                enemy.transform.position = nextPos;
            }
            else
            {
                StopRush(enemy);
                return runner;
            }
        }

        // 2. [?묒큺 泥댄겕]
        float distToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.transform.position);
        if (distToPlayer <= hitRadius)
        {
            StopRush(enemy);
            return runner;
        }

        // 3. [?꾩갑 泥댄겕]
        if (Vector3.Distance(enemy.transform.position, targetPos) < 0.1f)
        {
            StopRush(enemy);
        }

        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            StopRush(enemy);
        }
        return runner;
    }

    private void StopRush(Enemy enemy)
    {
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSHBOOL, true);
                // 3. 釉붾옓蹂대뱶 ?곗씠???ㅼ젙
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSH_DEST, enemy.transform.position);
        
        // [異붽?] ?쒖옉 ?쒓컙 湲곕줉 (怨≪꽑 怨꾩궛???꾪빐 ?꾩슂)
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSH_START_TIME, null);
        
    }

    public override void Reset<T>(T runner)
    {
        runner._aiController._aiBrain.blackboard.RemoveKey(KEY_RUSHBOOL);
        
    }
}
