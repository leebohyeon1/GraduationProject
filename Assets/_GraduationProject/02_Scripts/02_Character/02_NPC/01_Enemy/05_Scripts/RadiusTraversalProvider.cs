using UnityEngine;
using Pathfinding;

/// <summary>
/// 에이전트의 크기(Radius)를 고려하여 좁은 길을 벽으로 간주하는 경로 필터입니다.
/// 유니티 메인 스레드가 아닌 곳에서도 작동하도록 물리 체크 대신 그래프 노드 체크를 사용합니다.
/// </summary>
public class RadiusTraversalProvider : ITraversalProvider
{
    private readonly float agentRadius;
    private readonly float nodeSize;
    private readonly int nodeRadius;

    public RadiusTraversalProvider(float radius, float gridNodeSize, LayerMask mask)
    {
        this.agentRadius = radius;
        this.nodeSize = gridNodeSize;
        
        // 반지름을 노드 단위로 변환
        // 예: 반지름이 0.7이고 노드 크기가 1.0이면 nodeRadius는 1 (주변 1칸씩 더 검사)
        this.nodeRadius = Mathf.CeilToInt(agentRadius / nodeSize);
    }

    /// <summary>
    /// A* 엔진이 특정 노드를 지나갈 수 있는지 판단할 때 호출됩니다. (멀티스레드 환경에서 실행됨)
    /// </summary>
    public bool CanTraverse(Path path, GraphNode node)
    {
        // 1. 기본적으로 갈 수 없는 노드(벽)면 탈락
        if (!node.Walkable) return false;

        // 2. 그리드 노드가 아니면 기본 판정 반환
        if (!(node is GridNodeBase gridNode)) return true;

        // 3. [핵심] 물리 체크 대신 인접 노드들의 Walkable 여부를 확인 (Thread-Safe)
        // 내 몸집(nodeRadius) 범위 내에 갈 수 없는 노드가 하나라도 있다면 좁은 길로 간주
        GridGraph graph = gridNode.Graph as GridGraph;
        if (graph == null) return true;

        int x0 = gridNode.XCoordinateInGrid;
        int z0 = gridNode.ZCoordinateInGrid;

        for (int x = x0 - nodeRadius; x <= x0 + nodeRadius; x++)
        {
            for (int z = z0 - nodeRadius; z <= z0 + nodeRadius; z++)
            {
                // 범위를 벗어나거나(맵 밖) 갈 수 없는 노드가 있으면 통과 불가
                GridNodeBase other = graph.GetNode(x, z);
                if (other == null || !other.Walkable) return false;
            }
        }

        return true;
    }

    public uint GetTraversalCost(Path path, GraphNode node)
    {
        return DefaultITraversalProvider.GetTraversalCost(path, node);
    }
}
