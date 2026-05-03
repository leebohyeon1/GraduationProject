using UnityEngine;
using Cinemachine;

/// <summary>
/// 몬스터 상태 이벤트를 수신하여 카메라 연출(줌인, 타겟 포커싱 등)을 처리하는 컨트롤러입니다.
/// </summary>
public class EnemyEventCameraController : MonoBehaviour, IEventListener<EnemyStateData>
{
    [Header("Event Channel")]
    [SerializeField] private EnemyStateEventSO _enemyStateEventChannel;

    [Header("Camera Settings")]
    [SerializeField] private CinemachineTargetGroup _targetGroup;
    [SerializeField] private CinemachineFreeLook _freeLookCamera;

    private void OnEnable()
    {
        if (_enemyStateEventChannel != null)
        {
            _enemyStateEventChannel.Subscribe(this);
        }
    }

    private void OnDisable()
    {
        if (_enemyStateEventChannel != null)
        {
            _enemyStateEventChannel.Unsubscribe(this);
        }
    }

    /// <summary>
    /// 몬스터 상태 이벤트가 발생했을 때 호출됩니다.
    /// </summary>
    /// <param name="data">이벤트 데이터 (Enemy, StateType)</param>
    public void OnEventTrigger(EnemyStateData data)
    {
        if (data.enemy == null) return;

        switch (data.stateType)
        {
            case EnemyStateType.Detected:
                HandleEnemyDetected(data.enemy);
                break;
            case EnemyStateType.Lost:
                HandleEnemyLost(data.enemy);
                break;
            case EnemyStateType.Dead:
                HandleEnemyDead(data.enemy);
                break;
        }
    }

    private void HandleEnemyDetected(Enemy enemy)
    {
        Debug.Log($"<color=cyan>[Camera] Enemy Detected: {enemy.name}</color>");
        
        // TODO: 카메라 타겟 그룹에 적 추가 또는 줌인 연출 로직 작성
        if (_targetGroup != null)
        {
            _targetGroup.AddMember(enemy.transform, 1f, 2f);
        }
    }

    private void HandleEnemyLost(Enemy enemy)
    {
        Debug.Log($"<color=orange>[Camera] Enemy Lost: {enemy.name}</color>");
        
        // TODO: 카메라 타겟 그룹에서 적 제거 또는 기본 시야 복구 로직 작성
        if (_targetGroup != null)
        {
            _targetGroup.RemoveMember(enemy.transform);
        }
    }

    private void HandleEnemyDead(Enemy enemy)
    {
        Debug.Log($"<color=red>[Camera] Enemy Dead: {enemy.name}</color>");
        
        // TODO: 사망 연출 (슬로우 모션, 줌 아웃 등) 로직 작성
        if (_targetGroup != null)
        {
            _targetGroup.RemoveMember(enemy.transform);
        }
    }
}
