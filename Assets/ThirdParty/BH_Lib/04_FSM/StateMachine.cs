using System;
using System.Collections.Generic;
using UnityEngine;

namespace BH_Lib.FSM
{
    using Log;

    /// <summary>
    /// 제네릭 상태 머신 클래스
    /// 상태들을 관리하고 전환을 처리합니다.
    /// </summary>
    /// <typeparam name="T">상태 머신이 관리할 컨텍스트 타입</typeparam>
    public class StateMachine<T> where T : class
    {
        /// <summary>등록된 모든 상태들을 저장하는 딕셔너리</summary>
        private Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

        /// <summary>현재 활성 상태</summary>
        private IState _currentState;

        /// <summary>상태 머신이 작동하는 컨텍스트</summary>
        private T _context;

        /// <summary>현재 상태 반환</summary>
        public IState CurrentState => _currentState;

        /// <summary>현재 상태의 타입 반환</summary>
        public Type CurrentStateType => _currentState?.GetType();

        /// <summary>상태 전환 규칙들을 저장하는 딕셔너리</summary>
        private Dictionary<Type, List<StateTransition>> _transitions = new Dictionary<Type, List<StateTransition>>();

        /// <summary>현재 전환 중인지 확인하는 플래그</summary>
        private bool _isTransitioning = false;

        /// <summary>상태 변경 시 발생하는 이벤트</summary>
        public event Action<Type, Type> OnStateChanged;

        /// <summary>
        /// StateMachine 생성자
        /// </summary>
        /// <param name="context">상태 머신이 작동할 컨텍스트</param>
        public StateMachine(T context)
        {
            this._context = context;
        }

        /// <summary>
        /// 새로운 상태를 상태 머신에 추가
        /// </summary>
        /// <param name="state">추가할 상태</param>
        public void AddState(IState state)
        {
            Type stateType = state.GetType();
            if (!_states.ContainsKey(stateType))
            {
                _states.Add(stateType, state);
            }
        }

        /// <summary>
        /// 제네릭 타입으로 상태 전환
        /// </summary>
        /// <typeparam name="TState">전환할 상태 타입</typeparam>
        public void ChangeState<TState>() where TState : IState
        {
            Type stateType = typeof(TState);
            ChangeState(stateType);
        }

        /// <summary>
        /// 타입으로 상태 전환
        /// </summary>
        /// <param name="stateType">전환할 상태의 타입</param>
        public void ChangeState(Type stateType)
        {
            // 요청된 상태가 등록되어 있는지 확인
            if (!_states.ContainsKey(stateType))
            {
                Debug.Log($"State {stateType} not found in state machine!");
                return;
            }

            // 현재 상태가 같은 상태면 무시
            if (_currentState != null && _currentState.GetType() == stateType)
            {
                return;
            }

            // 현재 상태를 히스토리에 저장
            if (CurrentState != null)
            {
                stateHistory.Push(CurrentState.GetType());
                if (stateHistory.Count > maxHistorySize)
                {
                    // Stack을 List로 변환 후 다시 Stack으로 (크기 제한)
                    var list = new List<Type>(stateHistory);
                    list.RemoveAt(list.Count - 1);
                    stateHistory = new Stack<Type>(list);
                }
            }

            // 이전 상태 종료
            _currentState?.OnExit();

            // 새 상태로 전환
            _currentState = _states[stateType];
            _currentState.OnEnter();
        }

        /// <summary>
        /// 매 프레임 상태 업데이트 호출
        /// MonoBehaviour의 Update에서 호출해야 합니다.
        /// </summary>
        public void Update()
        {
            _currentState?.OnUpdate();

            if (!_isTransitioning && CurrentState != null)
            {
                CheckTransitions();
            }
        }

        /// <summary>
        /// 고정 시간 간격 상태 업데이트 호출
        /// MonoBehaviour의 FixedUpdate에서 호출해야 합니다.
        /// </summary>
        public void FixedUpdate()
        {
            _currentState?.OnFixedUpdate();
        }

        /// <summary>
        /// 현재 상태가 특정 타입인지 확인
        /// </summary>
        /// <typeparam name="TState">확인할 상태 타입</typeparam>
        /// <returns>현재 상태가 해당 타입이면 true</returns>
        public bool IsInState<TState>() where TState : IState
        {
            return _currentState != null && _currentState.GetType() == typeof(TState);
        }

        /// <summary>
        /// 상태 머신의 컨텍스트 반환
        /// </summary>
        /// <returns>컨텍스트 객체</returns>
        public T GetContext()
        {
            return _context;
        }

        /// <summary>
        /// 특정 상태에서 다른 상태로의 조건부 전환 규칙 추가
        /// </summary>
        /// <typeparam name="TFrom">전환 시작 상태</typeparam>
        /// <typeparam name="TTo">전환 목표 상태</typeparam>
        /// <param name="condition">전환 조건을 판단하는 함수</param>
        public void AddTransition<TFrom, TTo>(Func<bool> condition)
            where TFrom : IState
            where TTo : IState
        {
            Type fromState = typeof(TFrom);

            if (!_transitions.ContainsKey(fromState))
            {
                _transitions[fromState] = new List<StateTransition>();
            }

            _transitions[fromState].Add(new StateTransition<TTo>(condition));
        }

        /// <summary>
        /// 모든 상태에서 특정 상태로의 조건부 전환 규칙 추가
        /// </summary>
        /// <typeparam name="TTo">전환 목표 상태</typeparam>
        /// <param name="condition">전환 조건을 판단하는 함수</param>
        public void AddAnyTransition<TTo>(Func<bool> condition) where TTo : IState
        {
            foreach (var state in _states.Keys)
            {
                if (!_transitions.ContainsKey(state))
                {
                    _transitions[state] = new List<StateTransition>();
                }

                _transitions[state].Add(new StateTransition<TTo>(condition));
            }
        }

        /// <summary>
        /// 현재 상태에서 설정된 전환 조건들을 확인하고 자동 전환 처리
        /// </summary>
        private void CheckTransitions()
        {
            Type currentStateType = CurrentState.GetType();

            if (_transitions.ContainsKey(currentStateType))
            {
                foreach (var transition in _transitions[currentStateType])
                {
                    if (transition.Condition())
                    {
                        _isTransitioning = true;
                        Type previousState = currentStateType;
                        ChangeState(transition.TargetState);
                        OnStateChanged?.Invoke(previousState, transition.TargetState);
                        _isTransitioning = false;
                        break;
                    }
                }
            }
        }

        /// <summary>상태 히스토리를 저장하는 스택</summary>
        private Stack<Type> stateHistory = new Stack<Type>();

        /// <summary>히스토리 최대 저장 개수</summary>
        private int maxHistorySize = 10;

        /// <summary>
        /// 이전 상태로 복귀
        /// </summary>
        public void RevertToPreviousState()
        {
            if (stateHistory.Count > 0)
            {
                Type previousStateType = stateHistory.Pop();
                ChangeState(previousStateType);
            }
        }
        
        public Type GetPreviousState()
        {
            if (stateHistory.Count > 0)
            {
                return stateHistory.Peek();
            }
            return null;
        }
        
    }
}