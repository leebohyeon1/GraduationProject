
using UnityEngine;
using UnityEditor;
using System;

#if UNITY_EDITOR
[RequireComponent(typeof(Enemy))]
public class EnemyGizmoDrawer : MonoBehaviour
{
    [Header("Debug Preview")]
    [Tooltip("런타임 아닐 때,SO넣어서 미리보기")]
    [SerializeField] private EnemyAttackData _editorPreviewData;
    private EnemyAttackData _runtimeAttackData;
    private Enemy _enemy;
    
    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }
    
    private void OnDrawGizmosSelected()
    {
        DrawDistanceRings();
        DrawAttackRange();
    }
    internal void SetRuntimeAttackData(EnemyAttackData data)
    {
        _runtimeAttackData = data;
    }
    
    private void DrawDistanceRings() { 
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        for (int i = 1; i <= 30; i++)
        {
            Handles.color = Color.Lerp(Color.green, Color.blue, i / 30f);

            Handles.DrawWireDisc(transform.position, Vector3.up, i);
            
            Vector3 textPosition = transform.position + transform.forward * i;
            Handles.Label(textPosition, $"{i}m", style);
        } 
        }
    private void DrawAttackRange() { 
        EnemyAttackData dataToDraw = null;

        if (Application.isPlaying)
        {
            dataToDraw = _runtimeAttackData;
        }
        else
        {
            // 게임 실행 중이 아닐 때는 인스펙터에 넣어둔 데이터를 그립니다.
            dataToDraw = _editorPreviewData;
        }
        if (dataToDraw == null) return;
        if (dataToDraw.shape == AttackShape.Sphere && dataToDraw.damageRadius <= 0) return;
        if (dataToDraw.shape == AttackShape.Box && dataToDraw.boxSize == Vector3.zero) return;
        Gizmos.color = Color.red;
        Vector3 attackOrigin = transform.position + transform.TransformDirection(dataToDraw.attackOffset);

        switch (dataToDraw.shape)
        {
            case AttackShape.Sphere:
                Gizmos.DrawWireSphere(attackOrigin, dataToDraw.damageRadius);
                break;

            case AttackShape.Box:
                // 박스는 회전이 필요하므로 Matrix 조작
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(attackOrigin, transform.rotation, Vector3.one);
                Gizmos.matrix = rotationMatrix;
                // OverlapBox는 HalfExtents를 쓰지만 DrawWireCube는 전체 Size를 씁니다.
                Gizmos.DrawWireCube(Vector3.zero, dataToDraw.boxSize);
                Gizmos.matrix = Matrix4x4.identity; // Matrix 복구
                break;

            case AttackShape.Fan:
                // 부채꼴 (Handles는 Editor에서만 작동)
                Handles.color = new Color(1f, 0f, 0f, 0.2f); // 반투명 빨강
                
                // 시작 각도 계산 (몬스터의 정면 기준)
                // 부채꼴의 왼쪽 끝 방향 벡터
                Vector3 startDir = Quaternion.Euler(0, -dataToDraw.fanAngle * 0.5f, 0) * transform.forward;

                // 부채꼴 그리기 (위치, 축, 시작방향, 각도, 반지름)
                Handles.DrawSolidArc(attackOrigin, Vector3.up, startDir, dataToDraw.fanAngle, dataToDraw.damageRadius);
                
                // 외곽선 진하게
                Handles.color = Color.red;
                Handles.DrawWireArc(attackOrigin, Vector3.up, startDir, dataToDraw.fanAngle, dataToDraw.damageRadius);

                break;
        }
    }


}
#endif