using UnityEditor;
using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    [Header("Shield Settings")]
    public bool IsActive = true;            // 방패 활성화 여부
    [Tooltip("방어 가능한 전체 각도 (예: 120도면 좌우 60도씩 커버)")]
    [Range(0f, 360f)] 
    public float BlockAngle = 120f;         // 방어 각도 (Degree)

    [Tooltip("데미지 감소율 (예: 0.2 = 20% 데미지만 받음 / 0 = 완전 방어)")]
    [Range(0f, 1f)] 
    public float DamageReduction = 0.2f;

    public bool CheckBlock(Vector3 attackerPos)
    {
        if (!IsActive) return false;

        // 1. 공격자 방향 계산 (높이 무시)
        Vector3 dirToAttacker = (attackerPos - transform.position).normalized;
        dirToAttacker.y = 0;
        
        Vector3 myForward = transform.forward;
        myForward.y = 0;
        
        // 2. 각도 계산 (Vector3.Angle은 0~180도 사이의 값을 반환)
        float angle = Vector3.Angle(myForward, dirToAttacker);

        // 3. 내 각도 범위 안에 있는지 확인 (절반 각도와 비교)
        // 예: BlockAngle이 120도라면, 정면 기준 좌우 60도 이내여야 함
        return angle <= (BlockAngle * 0.5f);
    }
    #if UNITY_EDITOR
    // 씬 뷰에서 방어 범위를 그리는 함수
    private void OnDrawGizmosSelected()
    {
        if (!IsActive) return;

        // 반투명 초록색 부채꼴
        Handles.color = new Color(0f, 1f, 0f, 0.2f);
        Vector3 startDir = Quaternion.Euler(0, -BlockAngle * 0.5f, 0) * transform.forward;
        Handles.DrawSolidArc(transform.position, Vector3.up, startDir, BlockAngle, 2.0f);

        // 외곽선 (진한 초록색)
        Handles.color = Color.green;
        Handles.DrawWireArc(transform.position, Vector3.up, startDir, BlockAngle, 2.0f);
        
        // 중심선 (몬스터가 바라보는 방향)
        Handles.color = Color.yellow;
        Handles.DrawLine(transform.position, transform.position + transform.forward * 2.0f);
    }
#endif
}