using UnityEngine;
using BehaviorTree;
using Pathfinding;

public class BackMoving : Node
{
    [Tooltip("?좎??섍퀬 ?띠? 紐⑺몴 嫄곕━?낅땲??")]
    public float targetDistance = 5.0f;

    [Tooltip("?대룞??硫덉텧 紐⑺몴 吏?먭낵???덉슜 ?ㅼ감 嫄곕━?낅땲??")]
    public float acceptanceRadius = 0.5f;

    // ?대? 蹂??
    private Transform playerTransform;
    private AIPath aiPath;

    public float timeout = 1.8f;
    float startTime;

    public override void OnEnter()
    {
        playerTransform = runner.player.transform;
        aiPath = runner.GetComponent<AIPath>();

        if (aiPath == null)
        {
            Debug.LogError("AIPath 而댄룷?뚰듃瑜?李얠쓣 ???놁뒿?덈떎!", runner);
            return;
        }

        runner.SetState(EnemyStateController.EnemyState.RunAway);
        aiPath.enableRotation = false; // ?뚯쟾? ?섎룞?쇰줈 ?쒖뼱
        
        startTime = Time.time;
    }

    protected override NodeState OnUpdate()
    {
        if (playerTransform == null || aiPath == null)
        {
            return NodeState.FAILURE;
        }

        Vector3 playerFacingDir = playerTransform.forward;
        playerFacingDir.y = 0; // ?믩궙??臾댁떆
        playerFacingDir.Normalize();

        Vector3 currentTargetPosition = runner.transform.position + (playerFacingDir * targetDistance);

        runner.Movement.StartOrUpdateChase(currentTargetPosition);


        Vector3 currentDirectionToPlayer = (playerTransform.position - runner.transform.position);
        currentDirectionToPlayer.y = 0;

        if (currentDirectionToPlayer != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.LookRotation(currentDirectionToPlayer);
        }

        RaycastHit hit;
        bool isHit = Physics.Raycast(
            runner.transform.position,       
            -runner.transform.forward,      
            out hit,                         
            1f,                              
            LayerMask.GetMask("Wall")        
        );

        if (isHit)
        {
            return NodeState.SUCCESS;
        }

        // --- 醫낅즺 議곌굔 ---
        if (Time.time - startTime > timeout)
        {
            return NodeState.SUCCESS;
        }

        if (aiPath.reachedDestination)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    private void Cleanup()
    {
        if (runner != null && aiPath != null)
        {
            runner.Movement.StopMovement(); 
            runner.SetState(EnemyStateController.EnemyState.Idle);
        }
    }

    public override void OnExit() { Cleanup(); }
    public override void Abort() { Cleanup(); }

    public override Node Clone()
    {
        BackMoving newNode = Instantiate(this);
        newNode.targetDistance = this.targetDistance;
        newNode.acceptanceRadius = this.acceptanceRadius;
        return newNode;
    }
}
