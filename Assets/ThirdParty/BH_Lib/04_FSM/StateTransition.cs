using System;
using UnityEngine;

namespace BH_Lib.FSM
{
    /// <summary>
    /// 상태 전환 규칙을 정의하는 클래스
    /// </summary>
    public class StateTransition
    {
        /// <summary>전환 목표 상태의 타입</summary>
        public Type TargetState { get; private set; }
        
        /// <summary>전환 조건을 판단하는 함수</summary>
        public Func<bool> Condition { get; private set; }
        
        /// <summary>
        /// StateTransition 생성자
        /// </summary>
        /// <param name="targetState">전환할 목표 상태의 타입</param>
        /// <param name="condition">전환 조건을 판단하는 함수</param>
        public StateTransition(Type targetState, Func<bool> condition)
        {
            TargetState = targetState;
            Condition = condition;
        }
    }
    
    /// <summary>
    /// 제네릭 타입 안전성을 제공하는 상태 전환 클래스
    /// </summary>
    /// <typeparam name="TState">전환할 상태의 타입</typeparam>
    public class StateTransition<TState> : StateTransition where TState : IState
    {
        /// <summary>
        /// 제네릭 StateTransition 생성자
        /// </summary>
        /// <param name="condition">전환 조건을 판단하는 함수</param>
        public StateTransition(Func<bool> condition) 
            : base(typeof(TState), condition) { }
    }
}
