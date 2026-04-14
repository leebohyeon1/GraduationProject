using Pathfinding;
using UnityEngine;

[CreateAssetMenu(fileName = "KSante", menuName = "Enemy/Strategy/KSante")]
public class KSante : EnemyUseAnything
{
    [Header("Rush Settings")]
    public float rushSpeed = 20f;       // 湲곕낯 ?뚯쭊 ?띾룄 (怨≪꽑??Y媛믪씠 1???뚯쓽 ?띾룄)
    public float hitRadius = 1.5f;      // ?뚮젅?댁뼱 ?묒큺 ?먯젙 踰붿쐞
    public float overshootDist = 3.0f;  // 紐⑺몴 ?ㅻ쾭?덊듃 嫄곕━
    public LayerMask obstacleMask;      // 踰??덉씠??

    [Header("Speed Curve Settings")]
    public float rushDuration = 1.0f;   // ?뚯쭊??吏?띾맆 珥??쒓컙 (珥?
    public float turnSpeed = 10f;      // ?뚯쟾 ?띾룄 (??珥?
    // 釉붾옓蹂대뱶 ??
     public float PushDistance = 5.0f;
    public DamageData AttackDataKnockback;
    private const string KEY_RUSH_DEST = "RushDestination";
    private const string KEY_RUSHBOOL = "RushBool";
    private const string KEY_RUSH_START_TIME = "RushStartTime"; // [異붽?] ?쒖옉 ?쒓컙 ??μ슜
    private const string KEY_HAS_HIT = "HasHitPlayer"; // [異붽?] 以묐났 異⑸룎 諛⑹???
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

        runner.Movement.StopMovement();

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
        blackboard.SetValue(KEY_HAS_HIT, false); // [異붽?] 異⑸룎 ?곹깭 珥덇린??
        blackboard.SetValue(KEY_RUSH_START_TIME, Time.time);
        
        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            StopRush(enemy);
        }
        var board = runner._aiController._aiBrain.blackboard;
        if(board.GetValue<bool>(KEY_HAS_HIT) )
        {
            enemy.player.transform.parent = null;
            AttackDataKnockback.AttackerTransform = enemy.transform;
            enemy.player.GetComponent<IDamageable>().TakeDamage(AttackDataKnockback);
            board.SetValue(KEY_HAS_HIT, false);

            enemy.player.Movement.Step(enemy.transform.forward, 
                new StepData()
                {
                    StepDistance = AttackDataKnockback.KnockbackForce * AttackDataKnockback.KnockbackDuration,
                    StepDuration = AttackDataKnockback.KnockbackDuration,
                    StepCurve = AttackDataKnockback.KnockbackCurve,
                    StepRotateSpeed = 0f
                }, 
                this, false, () => 
            {
                enemy.player.GetComponent<IDragable>().Drop();
            });

        }
        return runner;
    }

    public override T OnUpdate<T>(T runner)
{
    var board = runner._aiController._aiBrain.blackboard;
    
    // 1. ?대? Rush媛 ?앸궗?붿? 泥댄겕
    if(board.GetValue<bool>(KEY_RUSHBOOL))
    {
        return runner; 
    }

    Enemy enemy = runner as Enemy;
    if (enemy == null || enemy.player == null) return runner;

    // 2. 紐⑺몴 吏??媛?몄삤湲?(?대쾲 ?꾨젅?꾩쓽 紐⑺몴)
    if (!board.GetValue<Vector3>(KEY_RUSH_DEST, out Vector3 targetPos))
    {
        return runner; 
    }

    // [?앸왂?덈뜕 遺遺? ?좊땲硫붿씠???곹깭 泥댄겕
    if(enemy.animHandler.IsActionSO)
    {
    }

    // [?앸왂?덈뜕 遺遺? ?쒓컙 寃쎄낵???곕Ⅸ ?띾룄 怨꾩궛 諛?醫낅즺 泥댄겕
    float startTime = board.GetValue<float>(KEY_RUSH_START_TIME);
    float elapsedTime = Time.time - startTime;      // 寃쎄낵 ?쒓컙
    float normalizedTime = elapsedTime / rushDuration; // 0.0 ~ 1.0 ?ъ씠 媛믪쑝濡??뺢퇋??
    // 3. ?대룞 怨꾩궛
    float step = rushSpeed * Time.deltaTime;
    Vector3 currentPos = enemy.transform.position;
    
    // 紐⑺몴 諛⑺뼢?쇰줈 ?대룞
    Vector3 nextPos = Vector3.MoveTowards(currentPos, targetPos, step);
    
    Vector3 moveDir = (nextPos - currentPos).normalized;
    moveDir.y = 0; // ?믪씠 李⑥씠 臾댁떆 (?됱? ?대룞 ??

    float moveDist = Vector3.Distance(currentPos, nextPos);

    // 4. ?대룞 以?踰?泥댄겕 (?대룞?섎젮??嫄곕━媛 ?꾩＜ ?묒쑝硫??앸왂)
    if (moveDist > 0.0001f)
    {
        // 二쇱쓽: obstacleMask???뚮젅?댁뼱媛 ?ы븿?섏뼱 ?덉쑝硫????⑸땲??
        if (!Physics.Raycast(currentPos + Vector3.up * 0.5f, moveDir, moveDist + 2, obstacleMask))
        {
            enemy.transform.position = nextPos;
        }
        else
        {
            StopRush(enemy);
            return runner;
        }
    }
    bool hashit = board.GetValue<bool>(KEY_HAS_HIT);
    if (!hashit)
    {
        float distToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.transform.position);
        if (distToPlayer <= hitRadius)
        {
            PlayerTORush(enemy);
            
            return runner; 
        }
    }
    // ?쒓컙?????섎㈃ 醫낅즺
    if (normalizedTime >= 1.0f)
    {
        StopRush(enemy);
        return runner;
    }


    

    if (Vector3.Distance(enemy.transform.position, targetPos) < 0.1f)
    {
        StopRush(enemy);
    }

    return runner;
}
    private void PlayerTORush(Enemy enemy)
    {
        var board = enemy._aiController._aiBrain.blackboard;

        // 1. 以묐났 ?몄텧 諛⑹? ?뚮옒洹??ㅼ젙
        board.SetValue(KEY_HAS_HIT, true);
        enemy.player.GetComponent<IDragable>().Drag();

        // 2. ?덈줈??紐⑺몴 吏??怨꾩궛: ?꾩옱 ?꾩튂?먯꽌 諛붾씪蹂대뒗 諛⑺뼢(Forward)?쇰줈 5m
        Vector3 currentPos = enemy.transform.position;
        Vector3 pushDir = enemy.transform.forward; // ?뱀? (playerPos - myPos).normalized
        Vector3 newDestination = currentPos + (pushDir * PushDistance);
        enemy.player.transform.parent = enemy.transform;

        Vector3 rayOrigin = currentPos + Vector3.up * 0.5f;

        RaycastHit hit;
        // maxPushDistance 留뚰겮 ?욎쓣 ?뺤씤
        if (Physics.Raycast(rayOrigin, pushDir, out hit, PushDistance, obstacleMask))
        {
            // [踰?諛쒓껄]
            // 踰??꾩튂(hit.point)?먯꽌 wallBuffer留뚰겮 ?ㅻ줈 類 ?꾩튂瑜?紐⑺몴濡??ㅼ젙
            float distanceToWall = hit.distance;
            
            // 踰쎌씠 ?덈Т 媛源뚯슦硫?buffer蹂대떎 媛源뚯슦硫? ?쒖옄由??뱀? ?꾩＜ 議곌툑留??대룞
            float targetDist = Mathf.Max(0, distanceToWall - 3);
            
            newDestination = currentPos + (pushDir * targetDist);
            
           
        }
        else
        {
            // [踰??놁쓬] 理쒕? 嫄곕━濡??대룞
            newDestination = currentPos + (pushDir * PushDistance);
        }

        // 3. 釉붾옓蹂대뱶 紐⑺몴 ?낅뜲?댄듃
        board.SetValue(KEY_RUSH_DEST, newDestination);

        // 4. [以묒슂] ?뚯쭊 ?쒓컙 由ъ뀑 (?덈줈??5m瑜??대룞???쒓컙??踰뚯뼱以?
        // ?쒓컙??由ъ뀑?섎㈃ curve(0)遺???ㅼ떆 ?쒖옉?섎?濡?硫덉무?????덉뒿?덈떎.
        // ?먯뿰?ㅻ읇寃??댁뼱吏湲??먰븳?ㅻ㈃ 蹂꾨룄??'PushDuration' 蹂?섎? ?곌굅??濡쒖쭅 議곗젙???꾩슂?섏?留?
        // 媛??媛꾨떒??諛⑸쾿? ?쒓컙??由ъ뀑?섎릺 curve ?쒖옉?먯씠 0???꾨땲?꾨줉 ?섍굅??洹몃깷 ?ㅼ떆 媛?랁븯??寃껋엯?덈떎.
        board.SetValue(KEY_RUSH_START_TIME, Time.time); 

        // (?좏깮) 諛怨??섍컝 ?뚮뒗 議곌툑 ???ㅻ옒 諛怨??띕떎硫?rushDuration???ш린???섎젮以섎룄 ?⑸땲??
        // rushDuration = 1.5f; 

        // (?좏깮) ?뚮젅?댁뼱?먭쾶 異⑷꺽/?됰갚??二쇨퀬 ?띕떎硫??ш린???뚮젅?댁뼱 ?ㅽ겕由쏀듃 ?몄텧
        // enemy.player.GetComponent<Rigidbody>().AddForce(pushDir * 10f, ForceMode.Impulse);
        
    }
    private void StopRush(Enemy enemy)
    {
        var board = enemy._aiController._aiBrain.blackboard;
        board.SetValue(KEY_RUSHBOOL, true);
                // 3. 釉붾옓蹂대뱶 ?곗씠???ㅼ젙
        board.SetValue(KEY_RUSH_DEST, enemy.transform.position);
        
        // [異붽?] ?쒖옉 ?쒓컙 湲곕줉 (怨≪꽑 怨꾩궛???꾪빐 ?꾩슂)
        board.SetValue(KEY_RUSH_START_TIME, null);
        
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.Teleport(enemy.transform.position);
            ai.canMove = true;      
            ai.isStopped = false;    
            ai.maxSpeed = enemy.Movement._normalSpeed; 
            ai.destination = enemy.transform.position;
            if (ai is AIPath aiPath) aiPath.enableRotation = true;
        }
        var Rvo = enemy.GetComponent<Pathfinding.RVO.RVOController>();
        if (Rvo != null)
        {
            Rvo.locked = false;
            Rvo.lockWhenNotMoving = true;
            Rvo.velocity = Vector3.zero;
        }
    }

    public override void Reset<T>(T runner)
    {
        
    }
}
