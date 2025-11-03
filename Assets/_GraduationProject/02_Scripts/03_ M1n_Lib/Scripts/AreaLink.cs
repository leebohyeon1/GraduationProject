using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;

public class AreaLink : MonoBehaviour {
    public Transform areaA; // GridGraph 영역
    public Transform areaB; // RecastGraph 영역
    public float radius = 5f;

    void OnEnable() {
        AstarPath.active.AddWorkItem(ctx => {
            var nodesA = GetNodesInArea(areaA.position, radius, graphMask: 1<<0); // Grid
            var nodesB = GetNodesInArea(areaB.position, radius, graphMask: 1<<1); // Recast
            
            foreach (var na in nodesA) {
                foreach (var nb in nodesB) {
                    // 노드 연결
                    var cost = (uint)(Vector3.Distance((Vector3)na.position, (Vector3)nb.position) * 1000);
                    na.AddConnection(nb, cost);
                    nb.AddConnection(na, cost);
                }
            }
        });
    }

    // 범위 내 노드 찾기
    NNConstraint constraint = NNConstraint.None;
    private List<GraphNode> GetNodesInArea(Vector3 pos, float radius, int graphMask) {
        var nodes = new List<GraphNode>();
        var constraint = NNConstraint.None;
        constraint.graphMask = graphMask;
        // AstarPath.active.GetN(node => {
        //     if ((node.position - (Int3)pos).sqrMagnitude < radius * radius) {
        //         nodes.Add(node);
        //     }
        //     return true;
        // });
        return nodes;
    }
}
