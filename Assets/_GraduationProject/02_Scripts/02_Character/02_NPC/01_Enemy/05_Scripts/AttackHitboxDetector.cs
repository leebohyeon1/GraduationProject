using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 히트박스 감지 및 시각화 컴포넌트
/// 공격 범위를 구체(Sphere), 박스(Box), 부채꼴(Fan) 형태로 감지하고 씬 뷰에서 시각화합니다.
/// </summary>
public class AttackHitboxDetector : MonoBehaviour
{
    [Header("히트박스 설정 (Hitbox Settings)")]
    [Tooltip("공격 모양 (Attack Shape)")]
    public AttackShape shape = AttackShape.Sphere;

    [Tooltip("공격 반지름 (Attack Radius) - 구체/부채꼴 사용")]
    public float damageRadius = 2.0f;

    [Tooltip("공격 오프셋 (Attack Offset) - 공격자 위치로부터의 거리")]
    public Vector3 attackOffset;

    [Header("박스 설정 (Box Settings)")]
    [Tooltip("박스 크기 (Box Size)")]
    public Vector3 boxSize = Vector3.one;

    [Header("부채꼴 설정 (Fan Settings)")]
    [Range(0, 360)]
    [Tooltip("부채꼴 각도 (Fan Angle)")]
    public float fanAngle = 90f;

    [Header("디버그 설정 (Debug Settings)")]
    [Tooltip("씬 뷰에서 히트박스 시각화 여부")]
    public bool showGizmos = true;

    [Tooltip("히트박스 색상 (Hitbox Color)")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);

    [Tooltip("히트박스 테두리 색상 (Hitbox Border Color)")]
    public Color gizmoBorderColor = Color.red;

    /// <summary>
    /// 현재 히트박스 범위 내의 모든 콜라이더를 반환합니다.
    /// </summary>
    /// <returns>히트된 콜라이더 배열</returns>
    public Collider[] GetHitColliders()
    {
        Vector3 attackOrigin = transform.position + transform.TransformDirection(attackOffset);

        switch (shape)
        {
            case AttackShape.Sphere:
                return Physics.OverlapSphere(attackOrigin, damageRadius);

            case AttackShape.Box:
                return Physics.OverlapBox(attackOrigin, boxSize * 0.5f, transform.rotation);

            case AttackShape.Fan:
                List<Collider> validHits = new List<Collider>();
                Collider[] rawHits = Physics.OverlapSphere(attackOrigin, damageRadius);

                foreach (var col in rawHits)
                {
                    Vector3 directionToTarget = (col.transform.position - attackOrigin).normalized;
                    float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

                    if (angleToTarget <= fanAngle * 0.5f)
                    {
                        validHits.Add(col);
                    }
                }
                return validHits.ToArray();

            default:
                return new Collider[0];
        }
    }

    /// <summary>
    /// 씬 뷰에서 히트박스 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Vector3 attackOrigin = transform.position + transform.TransformDirection(attackOffset);

        Gizmos.color = gizmoColor;

        switch (shape)
        {
            case AttackShape.Sphere:
                Gizmos.DrawSphere(attackOrigin, damageRadius);
                Gizmos.color = gizmoBorderColor;
                Gizmos.DrawWireSphere(attackOrigin, damageRadius);
                break;

            case AttackShape.Box:
                Gizmos.matrix = Matrix4x4.TRS(attackOrigin, transform.rotation, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, boxSize);
                Gizmos.color = gizmoBorderColor;
                Gizmos.DrawWireCube(Vector3.zero, boxSize);
                Gizmos.matrix = Matrix4x4.identity;
                break;

            case AttackShape.Fan:
                DrawFanGizmo(attackOrigin);
                break;
        }

        // 오프셋 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, attackOrigin);
        Gizmos.DrawWireSphere(attackOrigin, 0.1f);
    }

    /// <summary>
    /// 부채꼴 형태 시각화
    /// </summary>
    private void DrawFanGizmo(Vector3 origin)
    {
        int segments = 30;
        float stepAngle = fanAngle / segments;
        Vector3 startDirection = Quaternion.Euler(0, -fanAngle * 0.5f, 0) * transform.forward;

        // 내부 채우기 (삼각형 메시)
        Vector3[] points = new Vector3[segments + 2];
        points[0] = origin;

        for (int i = 0; i <= segments; i++)
        {
            Vector3 direction = Quaternion.Euler(0, -fanAngle * 0.5f + stepAngle * i, 0) * transform.forward;
            points[i + 1] = origin + direction * damageRadius;
        }

        Gizmos.color = gizmoColor;
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(points[i + 1], points[i + 2]);
            Gizmos.DrawLine(points[0], points[i + 1]);
        }

        // 외곽선
        Gizmos.color = gizmoBorderColor;
        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, -fanAngle * 0.5f, 0) * transform.forward * damageRadius);
        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, fanAngle * 0.5f, 0) * transform.forward * damageRadius);

        // 호 그리기
        for (int i = 0; i < segments; i++)
        {
            Vector3 point1 = origin + Quaternion.Euler(0, -fanAngle * 0.5f + stepAngle * i, 0) * transform.forward * damageRadius;
            Vector3 point2 = origin + Quaternion.Euler(0, -fanAngle * 0.5f + stepAngle * (i + 1), 0) * transform.forward * damageRadius;
            Gizmos.DrawLine(point1, point2);
        }
    }

    /// <summary>
    /// EnemyAttackData로부터 설정값을 불러옵니다.
    /// </summary>
    /// <param name="attackData">복사할 공격 데이터</param>
    public void LoadFromAttackData(EnemyAttackData attackData)
    {
        if (attackData == null)
        {
            Debug.LogWarning("[AttackHitboxDetector] AttackData가 null입니다.");
            return;
        }

        shape = attackData.shape;
        damageRadius = attackData.damageRadius;
        attackOffset = attackData.attackOffset;
        boxSize = attackData.boxSize;
        fanAngle = attackData.fanAngle;
    }
}
