using Pathfinding;
using UnityEngine;

[CreateAssetMenu(fileName = "TristanaJump", menuName = "Enemy/Strategy/TristanaJump")]
public class TristanaJump : EnemyUseAnything
{
    [Header("Jump Settings")]
    public float jumpRange = 8.0f;          // 理쒕? ?먰봽 嫄곕━
    public float jumpDuration = 0.8f;       // ?먰봽 泥닿났 ?쒓컙 (怨좎젙 ?쒓컙)
    public float jumpHeight = 5.0f;         // ?먰봽 理쒕? ?믪씠 (Y異?
    
    [Header("Landing Settings")]
    public float impactRadius = 2.5f;       // 李⑹? ???곕?吏 踰붿쐞
    public DamageData impactDamage;         // 李⑹? ?곕?吏 ?곗씠??
    
    [Header("Trajectory")]
    // X異? 0~1 (?쒓컙), Y異? 0~1 (?믪씠 鍮꾩쑉). 
    // 紐⑥뼇??(0,0) -> (0.5, 1) -> (1,0) ?쇰줈 ?ㅼ젙?섏뿬 ?щЪ?좎쓣 留뚮뱶?몄슂.
    public AnimationCurve heightCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

    // 釉붾옓蹂대뱶 ??
    private const string KEY_JUMP_START_POS = "JumpStartPos";
    private const string KEY_JUMP_END_POS = "JumpEndPos";
    private const string KEY_JUMP_START_TIME = "JumpStartTime";
    private const string KEY_IS_JUMPING = "IsJumping";

    public override T OnEnter<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;

        // ?대? ?먰봽 以묒씠硫?由ы꽩
        if (blackboard.GetValueOrDefault<bool>(KEY_IS_JUMPING, false))
        {
            return runner;
        }

        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        // 1. A* 諛?臾쇰━ ?뺤? (怨듭쨷 ?대룞???꾪빐 吏곸젒 ?쒖뼱)
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
        }
        enemy.Movement.StopMovement();

        // 2. 紐⑺몴 吏??怨꾩궛
        Vector3 startPos = enemy.transform.position;
        Vector3 playerPos = enemy.player.transform.position;
        
        // ?뚮젅?댁뼱 諛⑺뼢?쇰줈 理쒕? ?ш굅由щ쭔??怨꾩궛
        Vector3 direction = (playerPos - startPos);
        direction.y = 0; // ?믪씠 臾댁떆
        float distance = direction.magnitude;
        direction.Normalize();

        // ?ш굅由щ? 踰쀬뼱?섎㈃ 理쒕? ?ш굅由щ줈 ?쒗븳
        float jumpDist = Mathf.Min(distance, jumpRange);
        Vector3 targetPos = startPos + (direction * jumpDist);

        // [以묒슂] 紐⑺몴 吏?먯씠 ?대룞 媛?ν븳 怨녹씤吏 ?뺤씤 (A* NavMesh 湲곗?)
        // 踰??띿쑝濡??ㅼ뼱媛??寃껋쓣 諛⑹??섍린 ?꾪빐 媛??媛源뚯슫 ?몃뱶濡?蹂댁젙
        NNInfo info = AstarPath.active.GetNearest(targetPos, NNConstraint.Default);
        if (info.node != null)
        {
            targetPos = info.position;
        }

        // 3. 釉붾옓蹂대뱶 ?곗씠???ㅼ젙
        blackboard.SetValue(KEY_JUMP_START_POS, startPos);
        blackboard.SetValue(KEY_JUMP_END_POS, targetPos);
        blackboard.SetValue(KEY_JUMP_START_TIME, Time.time);
        blackboard.SetValue(KEY_IS_JUMPING, true);

        // 4. ?먰봽 ?쒖옉 ?좊땲硫붿씠???몃━嫄?(?꾩슂??
        // enemy.animHandler.Play("JumpStart");

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;
        if (!blackboard.GetValueOrDefault<bool>(KEY_IS_JUMPING, false)) return runner;

        Enemy enemy = runner as Enemy;
        
        // 1. ?쒓컙 怨꾩궛
        float startTime = blackboard.GetValue<float>(KEY_JUMP_START_TIME);
        float elapsedTime = Time.time - startTime;
        float normalizedTime = elapsedTime / jumpDuration; // 0.0 ~ 1.0

        // 2. ?대룞 濡쒖쭅 (Parabolic Movement)
        if (normalizedTime < 1.0f)
        {
            Vector3 startPos = blackboard.GetValue<Vector3>(KEY_JUMP_START_POS);
            Vector3 endPos = blackboard.GetValue<Vector3>(KEY_JUMP_END_POS);

            // A. ?섑룊 ?대룞 (Lerp: ?좏삎 蹂닿컙)
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, normalizedTime);

            // B. ?섏쭅 ?대룞 (Animation Curve ?쒖슜)
            // 而ㅻ툕 媛?0~1) * 理쒕? ?믪씠
            float height = heightCurve.Evaluate(normalizedTime) * jumpHeight;
            currentPos.y += height;

            // ?꾩튂 ?곸슜
            enemy.transform.position = currentPos;
            
            // (?좏깮) 吏꾪뻾 諛⑺뼢 諛붾씪蹂닿린
            Vector3 lookDir = (endPos - startPos).normalized;
            if(lookDir != Vector3.zero) 
                enemy.transform.rotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            // ?쒓컙 醫낅즺 -> 李⑹?
            Landing(enemy);
        }

        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            // 媛뺤젣 醫낅즺 ???덉쟾?섍쾶 李⑹? 泥섎━
            if (runner._aiController._aiBrain.blackboard.GetValueOrDefault<bool>(KEY_IS_JUMPING, false))
            {
                Landing(enemy);
            }
        }
        return runner;
    }

    private void Landing(Enemy enemy)
    {
        var blackboard = enemy._aiController._aiBrain.blackboard;
        
        // 1. ?곹깭 ?댁젣
        blackboard.SetValue(KEY_IS_JUMPING, false);

        // 2. ?꾩튂 蹂댁젙 (理쒖쥌 紐⑺몴 吏?먯쑝濡?媛뺤젣 ?대룞 諛??믪씠 珥덇린??
        Vector3 landPos = blackboard.GetValue<Vector3>(KEY_JUMP_END_POS);
        // ?뱀떆 怨듭쨷???좎엳?????덉쑝誘濡?y媛믪쓣 NavMesh ?믪씠濡?留욎땄
        landPos.y = AstarPath.active.GetNearest(landPos).position.y;
        enemy.transform.position = landPos;

        // 3. 李⑹? ?곕?吏 諛??댄럺??(愿묒뿭 ?곕?吏)
        Collider[] hitColliders = Physics.OverlapSphere(landPos, impactRadius, LayerMask.GetMask("Player"));
        foreach (var hitCollider in hitColliders)
        {
            // ?뚮젅?댁뼱 ?곕?吏 泥섎━
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>(); // ?뱀? ?곸젅??而댄룷?뚰듃
            if (playerHealth != null)
            {
                impactDamage.AttackerTransform = enemy.transform;
                playerHealth.TakeDamage(impactDamage);
                
                // (?좏깮) ?щ줈???④낵 異붽? 媛??
            }
        }
        enemy.animator.SetBool("IsRushing" , true);
        // 4. A* 諛?臾쇰━ 蹂듦뎄
       
    }
    
    public override void Reset<T>(T runner)
    {
        // ?꾩슂 ??珥덇린??濡쒖쭅
    }
}
