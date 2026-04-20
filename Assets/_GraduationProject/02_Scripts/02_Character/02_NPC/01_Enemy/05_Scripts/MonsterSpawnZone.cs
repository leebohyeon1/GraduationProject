using UnityEngine;

/// <summary>
/// 몬스터 스폰 구역을 정의하는 컴포넌트입니다. (콜라이더 기반 - 구버전 호환용)
/// </summary>
public class MonsterSpawnZone : MonoBehaviour
{
    [Header("Group Settings")]
    public GroupAi targetGroupAi;

    [Header("Visualization")]
    public Color zoneColor = new Color(0, 1, 0, 0.2f);
    public bool showWireframe = true;

    private void Awake()
    {
        Debug.Log($"[MonsterSpawnZone] {gameObject.name} initialized with GroupAi: {(targetGroupAi != null ? targetGroupAi.GroupName : "None")}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = zoneColor;
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (collider is BoxCollider box)
            {
                if (showWireframe) Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (collider is SphereCollider sphere)
            {
                if (showWireframe) Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}
