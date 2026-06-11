using UnityEngine;
using BehaviorTree;
using Pathfinding;

public class Task_FleeFromTarget : Node
{
    [Header("Settings")]
    public float fleeDistance = 5f;
    public float fleeSpeed = 5f;

    public override void OnEnter()
    {
        base.OnEnter();
        if(runner._animationBridge.IsAttacking) {
            return;
        }
        if (runner.player == null) return;

        Vector3 myPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;

        // 1. 1?쒖쐞: ?뚮젅?댁뼱 諛섎? 諛⑺뼢
        Vector3 fleeDir = (myPos - playerPos).normalized;
        fleeDir.y = 0;

        Vector3 finalDirection = fleeDir;
        float finalDistance = fleeDistance;

        // 2. Movement?먭쾶 "?ㅼそ 留됲삍??" 臾쇱뼱蹂닿린
        if (runner.Movement.IsPathBlocked(fleeDir, fleeDistance, out RaycastHit hit))
        {
            // [?꾩닠???먮떒] ?ㅺ? 留됲삍?쇰땲 醫???踰??湲? 諛⑺뼢??怨꾩궛?섏옄

            // 踰쎌쓽 踰뺤꽑(Normal)???댁슜??踰쎌쓣 ?怨??먮Ⅴ??諛⑺뼢(Tangent) 怨꾩궛
            Vector3 slideLeft = Vector3.Cross(hit.normal, Vector3.up).normalized;
            Vector3 slideRight = -slideLeft;

            // 3. 醫???以??대뵒媛 ?ル졇?붿? Movement?먭쾶 ?ㅼ떆 臾쇱뼱蹂닿린
            bool isLeftBlocked = runner.Movement.IsPathBlocked(slideLeft, fleeDistance, out RaycastHit leftHit);
            bool isRightBlocked = runner.Movement.IsPathBlocked(slideRight, fleeDistance, out RaycastHit rightHit);

            if (!isLeftBlocked && !isRightBlocked)
            {
                // ?????ル졇?쇰㈃ ?먮옒 ?꾨쭩媛?ㅻ뜕 諛⑺뼢怨???媛源뚯슫 履??댁쟻) ?좏깮
                float dotLeft = Vector3.Dot(fleeDir, slideLeft);
                float dotRight = Vector3.Dot(fleeDir, slideRight);
                finalDirection = (dotLeft > dotRight) ? slideLeft : slideRight;
            }
            else if (!isLeftBlocked)
            {
                finalDirection = slideLeft;
            }
            else if (!isRightBlocked)
            {
                finalDirection = slideRight;
            }
            else
            {
                // [援ъ꽍??紐곕┝] ?묒そ ??留됲삍?쇰㈃ 洹몃깷 踰?諛붾줈 ?욊퉴吏留?臾쇰윭??
                finalDirection = fleeDir; 
                finalDistance = Mathf.Max(0, hit.distance - 0.5f); // 踰?踰꾪띁 吏곸젒 ?곸슜 or Movement??留↔?
            }
        }

        // 4. 理쒖쥌 寃곗젙??醫뚰몴 怨꾩궛
        Vector3 finalDestination = myPos + (finalDirection * finalDistance);
        runner.aIPath.enableRotation = true;
        // 5. ?대룞 紐낅졊 ?섎떖
        runner.Movement.StartOrUpdateChase(finalDestination, EnemyStateController.EnemyState.Chase, fleeSpeed);
    }

    protected override NodeState OnUpdate()
    {
        if (runner.player == null) return NodeState.FAILURE;
        if(runner._animationBridge.IsAttacking) {
            return NodeState.FAILURE;
        }
        var ai = runner.GetComponent<IAstarAI>();
        if (ai == null) return NodeState.FAILURE;

        if (!ai.pathPending && (ai.reachedEndOfPath || ai.reachedDestination)) 
        {
            return NodeState.SUCCESS;
        }
        runner.aIPath.enableRotation = true;

        return NodeState.RUNNING;
    }
    public override void OnExit()
    {
        runner.Movement.StopMovement();
        if (runner.player == null) return;
        Vector3 playerDir = runner.player.transform.position - runner.transform.position;
        playerDir.y = 0;
        playerDir.Normalize();
        if (playerDir != Vector3.zero) runner.transform.rotation = Quaternion.LookRotation(playerDir);
    }
}
