using UnityEngine;

/// <summary>
/// 씬의 특정 구역(Trigger Collider)에 부착하여 구역 ID를 부여합니다.
/// </summary>
public class ZoneArea : MonoBehaviour
{
    [Tooltip("구역 고유 번호 (예: 1, 2, 3, 5 등)")]
    public int zoneId;
    [Tooltip("우선순위 (값이 높을수록 우선적으로 판단됩니다. 예: 근거리=10, 원거리=1)")]
    public int priority = 0;

    private void Awake()
    {
        // 트리거 설정 확인
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }
}
