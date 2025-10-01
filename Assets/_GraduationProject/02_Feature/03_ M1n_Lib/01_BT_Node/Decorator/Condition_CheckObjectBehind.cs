using UnityEngine;
using BehaviorTree;
public class Condition_CheckObjectBehind : Node
{
    [Tooltip("뒤를 체크할 거리입니다.")]
    public float checkDistance = 2.0f;

    [Tooltip("체크할 레이어 마스크입니다.")]
    public LayerMask layerMask;

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.checkDistance = this.checkDistance;
        node.layerMask = this.layerMask;
        return node;
    }
    
    public override void OnEnter()
    {

    }

    protected override NodeState OnUpdate()
    {
        // Raycast를 사용하여 뒤에 오브젝트가 있는지 확인합니다.
        Ray ray = new Ray(runner.transform.position, -runner.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, checkDistance, layerMask))
        {
            // 오브젝트가 감지되었을 때의 처리
            Debug.DrawLine(ray.origin, hit.point, Color.red); // 디버그용 라인 그리기
            Debug.Log("뒤에 오브젝트가 있습니다.");
            return NodeState.SUCCESS; // 오브젝트가 뒤에 있음을 나타냅니다.
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin - runner.transform.forward * checkDistance, Color.green); // 디버그용 라인 그리기
            return NodeState.FAILURE; // 오브젝트가 뒤에 없음을 나타냅니다.
        }
    }
}