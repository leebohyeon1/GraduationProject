// 파일 경로: 01_BT_Node/Action/BT/OnlyBackChase.cs

using UnityEngine;
using Pathfinding;
using System.Collections.Generic;
using BehaviorTree;

[CreateAssetMenu(fileName = "OnlyBackChase", menuName = "BehaviorTree/Action/OnlyBackChase")]
public class OnlyBackChase : Node
{
    [Header("타겟 설정")]
    [Tooltip("플레이어와 유지할 등 뒤 거리입니다.")]
    public float desiredBehindDistance = 4f;

    [Header("회피 설정")]
    [Tooltip("플레이어 주변에 생성할 회피 영역의 반지름입니다.")]
    public float avoidanceRadius = 3f;
    [Tooltip("A* 에디터에서 설정한 패널티 태그 번호")]
    public int penaltyTag = 1;
    [Tooltip("이 AI가 회피 태그에 부여할 패널티 값")]
    public int avoidancePenalty = 20000;

    // --- BT 내부 변수 ---
    // IAstarAI는 이제 직접 사용하지 않으므로 제거해도 됩니다.
    private Seeker _seeker;
    private List<GraphNode> _affectedNodes = new List<GraphNode>();
    private float _timer = 0f;
    private const float LOGIC_UPDATE_INTERVAL = 0.25f;

    public override void OnEnter()
    {

        // Seeker는 패널티 설정에 필요하므로 유지합니다.
        _seeker = runner.GetComponent<Seeker>();

        if (_seeker == null)
        {
            return;
        }

        ConfigureTagPenalties();
        _timer = 0f;
        runner.SetState(Enemy.EnemyState.Chase);
    }

    protected override NodeState OnUpdate()
    {
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        
        if (typeof(PlayerAttackBaseState).IsAssignableFrom(runner.player.CurrentPlayerState))
        {
            return NodeState.SUCCESS;
        }
        else if (runner.player.CurrentPlayerState == typeof(PlayerDodgeState))
        {
            return NodeState.SUCCESS;
        }
        if (runner.player == null)
        {
            return NodeState.FAILURE;
        }
        
        _timer += Time.deltaTime;

        if (_timer >= LOGIC_UPDATE_INTERVAL)
        {
            _timer = 0f; 
            

            UpdateAvoidanceZone();

            Transform target = runner.player.transform;
            Vector3 behindPosition = target.position - (target.forward * desiredBehindDistance);
            
            Debug.DrawLine(runner.transform.position, behindPosition, Color.green, LOGIC_UPDATE_INTERVAL);

            runner.Movement.StartOrUpdateChase(behindPosition, runner.NormalSpeed * 1.5f); // runner의 공식 이동 함수를 사용합니다.
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {

        ClearAvoidanceZone();

        if (runner.Movement != null)
        {
            runner.Movement.StopMovement();
        }
        
        if (runner.CurrentState == Enemy.EnemyState.Chase)
        {
            runner.SetState(Enemy.EnemyState.Idle);
        }
    }

    public override Node Clone()
    {
        OnlyBackChase node = Instantiate(this);
        node.desiredBehindDistance = this.desiredBehindDistance;
        node.avoidanceRadius = this.avoidanceRadius;
        node.penaltyTag = this.penaltyTag;
        node.avoidancePenalty = this.avoidancePenalty;
        return node;
    }

    // --- 핵심 로직 함수들 (이하 동일) ---

    private void ConfigureTagPenalties()
    {
        if (_seeker.tagPenalties == null || _seeker.tagPenalties.Length <= penaltyTag)
        {
            int[] newPenalties = new int[penaltyTag + 1];
            if (_seeker.tagPenalties != null) _seeker.tagPenalties.CopyTo(newPenalties, 0);
            _seeker.tagPenalties = newPenalties;
        }
        _seeker.tagPenalties[penaltyTag] = avoidancePenalty;
    }

    private void UpdateAvoidanceZone()
    {
        if (AstarPath.active == null) return;
        
        ClearAvoidanceZone();

        var gridGraph = AstarPath.active.data.gridGraph;
        if (gridGraph == null) return;

        Bounds bounds = new Bounds(runner.player.transform.position, new Vector3(avoidanceRadius * 2, avoidanceRadius * 2, avoidanceRadius * 2));
        IntRect rect = gridGraph.GetRectFromBounds(bounds);
        List<GraphNode> nodesInBounds = gridGraph.GetNodesInRegion(rect);

        uint uPenaltyTag = (uint)penaltyTag;
        foreach (var node in nodesInBounds)
        {
            node.Tag = uPenaltyTag;
            _affectedNodes.Add(node);
        }
    }

    private void ClearAvoidanceZone()
    {
        if (_affectedNodes.Count > 0)
        {
            foreach (var node in _affectedNodes)
            {
                if (node != null && !node.Destroyed) node.Tag = 0;
            }
            _affectedNodes.Clear();
        }
    }
}