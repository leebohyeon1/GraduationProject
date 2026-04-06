using UnityEngine;

public interface IState
{
    void OnEnter();

    void OnUpdate();

    void OnFixedUpdate();

    void OnExit();
}

public class State<T> : IState
{
    protected T p_context;
    protected StateMachine<T> p_stateMachine;

    public State(T state, StateMachine<T> machine)
    {
        p_context = state;
        p_stateMachine = machine;
    }

    public virtual void OnEnter() { }

    public virtual void OnExit() { }

    public virtual void OnFixedUpdate() { }

    public virtual void OnUpdate() { }
}
