using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 스테이트 머신
/// </summary>
/// <typeparam name="T">스테이트 머신 주인</typeparam>
public class StateMachine<T>
{
    private T _context;

    private Dictionary<Type, IState> _states;

    private IState _currentState;
    private IState _previousState;

    public Type CurrentState => _currentState?.GetType();
    public Type PreviousState => _previousState?.GetType();

    public StateMachine(T context)
    {
        _context = context;
        _states = new Dictionary<Type, IState>();
    }

    public void Update()
    {
        _currentState?.OnUpdate();   
    }
    
    public void FixedUpdate()
    {
        _currentState?.OnFixedUpdate();
    }


    public void AddState(IState state)
    {
        Type stateType = state.GetType();
        if (!_states.ContainsKey(stateType))
        {
            _states.Add(stateType, state);
        }
    }

    public void ChangeState<TState>() where TState : IState
    {
        Type changeState = typeof(TState);
        ChangeState(changeState);
    }

    public void ChangeState(Type stateType)
    {
        if (!_states.ContainsKey(stateType))
        {
            return;
        }

        if(CurrentState != null)
        {
            _currentState?.OnExit();
            _previousState = _states[CurrentState.GetType()];
        }

        _currentState = _states[stateType];

        _currentState.OnEnter();
    }

    public T GetContext()
    {
        return _context;
    }
}
