using UnityEngine;

namespace BH_Lib.FSM
{
    /// <summary>
    /// 상태의 기본 추상 클래스
    /// 컨텍스트와 상태 머신에 대한 참조를 제공합니다.
    /// </summary>
    /// <typeparam name="T">상태가 작동할 컨텍스트 타입</typeparam>
    public abstract class BaseState<T> : IState where T : class
    {
        /// <summary>상태가 작동하는 컨텍스트 객체</summary>
        protected T p_context;
        
        /// <summary>이 상태를 관리하는 상태 머신</summary>
        protected StateMachine<T> p_stateMachine;
        
        /// <summary>
        /// BaseState 생성자
        /// </summary>
        /// <param name="context">상태가 작동할 컨텍스트</param>
        /// <param name="stateMachine">상태를 관리하는 상태 머신</param>
        public BaseState(T context, StateMachine<T> stateMachine)
        {
            this.p_context = context;
            this.p_stateMachine = stateMachine;
        }
        
        /// <summary>상태 진입 시 호출 (필요시 오버라이드)</summary>
        public virtual void OnEnter() { }
        
        /// <summary>매 프레임 업데이트 시 호출 (필요시 오버라이드)</summary>
        public virtual void OnUpdate() { }
        
        /// <summary>고정 시간 간격 업데이트 시 호출 (필요시 오버라이드)</summary>
        public virtual void OnFixedUpdate() { }
        
        /// <summary>상태 종료 시 호출 (필요시 오버라이드)</summary>
        public virtual void OnExit() { }
    }
}