using System;
using UnityEngine;

public class  StiffnessSystem : MonoBehaviour, IStiffness
{
    [Header("Stiffness Settings")]
    [Tooltip("캐릭터 최대 경직")]
    [SerializeField] protected int _stiffnessThreshold = 100;
    [Tooltip("경직 상태가 지속 시간")]
    [SerializeField] protected float _stiffnessDuration = 2f;
    [SerializeField] protected float _weakStiffnessDuration = 2f;
    protected int _currentStiffness = 0;

    public event Action<int, int> OnStiffnessChanged;

    #region Properties
    public int CurrentStiffness => _currentStiffness;

    public int StiffnessThreshold => _stiffnessThreshold;

    public float StiffnessDuration => _stiffnessDuration;
    #endregion

    /// <summary>
    /// 경직도를 게이지에 누적시킵니다.
    /// </summary>
    /// <param name="amount">추가할 경직도</param>
    /// <param name="isCounterAttack">카운터 공격 여부</param>
    public virtual void AddStiffness(int amount, bool isCounterAttack = false)
    {
        int previousStiffness = _currentStiffness;  
        _currentStiffness += amount;

        OnStiffnessChanged?.Invoke(previousStiffness, _currentStiffness);

        // 경직 게이지가 가득 찼는지 확인
        if (_currentStiffness >= _stiffnessThreshold)
        {
            previousStiffness = _currentStiffness;
            _currentStiffness = 0; // 게이지 초기화

            OnStiffnessChanged?.Invoke(previousStiffness, _currentStiffness);
            // 주인의 ApplyStun 함수를 호출하여 기절시킵니다.
            OnHeavyStagger();
        }
        else
        {
            if(!isCounterAttack)
                return;
            // 경직 게이지가 가득 차지 않았을 때의 피드백 처리
            OnLightStagger();
        }
    }

    /// <summary>
    /// 가벼운 경직 함수
    /// </summary>
    /// <param name="isCounterAttack">카운터 공격 여부</param>
    protected virtual void OnLightStagger() { }

    /// <summary>
    /// 무거운 경직 함수
    /// </summary>
    protected virtual void OnHeavyStagger() { }


}
