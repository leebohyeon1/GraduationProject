using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders.Simulation;

public class StiffnessSystem : MonoBehaviour
{
    [Header("Stiffness Settings")]
    [Tooltip("캐릭터 최대 경직")]
    [SerializeField] protected int _stiffnessThreshold = 100;
    [Tooltip("경직 상태가 지속 시간")]
    [SerializeField] protected float _stiffnessDuration = 2f;
    private int _currentStiffness = 0;

    /// <summary>
    /// 경직도를 게이지에 누적시킵니다.
    /// </summary>
    /// <param name="amount">추가할 경직도</param>
    public void AddStiffness(int amount)
    {
        _currentStiffness += amount;
        
        // 경직 게이지가 가득 찼는지 확인
        if (_currentStiffness >= _stiffnessThreshold)
        {
            _currentStiffness = 0; // 게이지 초기화
           
            // 주인의 ApplyStun 함수를 호출하여 기절시킵니다.
            OnHeavyStagger();
        }
        else
        {
            // 경직 게이지가 가득 차지 않았을 때의 피드백 처리
            OnLightStagger();
        }
    }
    protected virtual void OnLightStagger() { }
    protected virtual void OnHeavyStagger() { }
}
