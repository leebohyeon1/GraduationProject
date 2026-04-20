using UnityEngine;
using Pathfinding;

[CreateAssetMenu(fileName = "RushAttackStrategy", menuName = "Enemy/Strategy/Rush Attack")]
public class RushAttackStrategy : EnemyUseAnything
{
    // [?ㅼ젙媛? ?대윴 嫄?SO????ν빐???⑸땲?? (紐⑤뱺 紐ъ뒪??怨듯넻)
    public float rushSpeed = 15f;    
    public float stopDistance = 1.0f;
    bool endStrategy = false;
    public float speed = 6;

    // [?곹깭媛? ?뚯쭊??硫덉톬?붿? ?щ? ?깆? '?몄뒪?댁뒪'媛 ?꾩슂?섏?留? 
    // 媛꾨떒?섍쾶 ?섍린 ?꾪빐 ?ш린?쒕뒗 runner瑜??듯빐 ?쒖뼱?섍굅?? 
    // 蹂듭옟?섎㈃ Node?먯꽌 愿由ы빐???⑸땲??
    // ?쇰떒 ?ш린?쒕뒗 濡쒖쭅留?泥섎━?⑸땲??



    public override T OnEnter<T>(T runner)
    {
         // runner瑜??듯빐 ?ъ뿉 ?덈뒗 而댄룷?뚰듃???묎렐?⑸땲??
        var aiPath = runner.GetComponent<AIPath>(); 
        if (aiPath != null)
        {
            aiPath.maxSpeed = rushSpeed;
            aiPath.enableRotation = false;
            endStrategy = false;
        }
        return runner;
    }


    public override T OnUpdate<T>(T runner)
    {
        // ?ъ뿉 ?덈뒗 ?뚮젅?댁뼱 李얘린: runner.player
        if (runner.player == null) return null;

        // 濡쒖쭅 ?섑뻾
        float dist = Vector3.Distance(runner.transform.position, runner.player.transform.position);
        
        if (dist > stopDistance && !runner.animHandler.IsHitWindowOpen && !endStrategy)
        {
             runner.Movement.StartRush(runner.player.transform.position, rushSpeed);
             // ?뚯쟾 濡쒖쭅 ??..
        }
        else
        {
             runner.Movement.StopMovement();
             endStrategy = true;
        }
        return runner;
    }

    public override T OnExit<T>(T runner) // <--- 醫낅즺 ???뺣━
    {
        // runner瑜??먮옒?濡??뚮젮?볤린
        var aiPath = runner.GetComponent<AIPath>();
        if (aiPath != null)
        {
            aiPath.maxSpeed = runner.Movement._normalSpeed; // ?먮옒 ?띾룄濡?蹂듦뎄 (?뱀? Enemy ?ㅽ꺈 李몄“)
            aiPath.enableRotation = true;
        }
        return runner;
    }

    public override void Reset<T>(T runner)
    {
        
    }
}
