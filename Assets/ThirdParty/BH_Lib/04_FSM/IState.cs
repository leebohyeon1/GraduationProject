using UnityEngine;

namespace BH_Lib.FSM
{
    /// <summary>
    /// 상태 머신의 기본 상태 인터페이스
    /// 모든 상태는 이 인터페이스를 구현해야 합니다.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 상태 진입 시 호출되는 메서드
        /// </summary>
        void OnEnter();
        
        /// <summary>
        /// 매 프레임 업데이트 시 호출되는 메서드
        /// </summary>
        void OnUpdate();
        
        /// <summary>
        /// 고정 시간 간격으로 호출되는 메서드 (물리 연산용)
        /// </summary>
        void OnFixedUpdate();
        
        /// <summary>
        /// 상태 종료 시 호출되는 메서드
        /// </summary>
        void OnExit();
    }
}