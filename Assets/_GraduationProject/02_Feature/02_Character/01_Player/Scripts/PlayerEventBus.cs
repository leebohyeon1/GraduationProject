using System;

public class PlayerEventBus
{
    public event Action<int, int> OnHealthChanged;
    public void PublishHealthChanged(int previous, int current) => OnHealthChanged?.Invoke(previous, current);

    public event Action OnPlayerDied;
    public void PublishPlayerDied() => OnPlayerDied?.Invoke();

    public event Action OnAllowAttackInput;
    public void PublishAllowAttackInput() => OnAllowAttackInput?.Invoke();

    public event Action OnAttack;
    public void PublishAttack() => OnAttack?.Invoke();

    public event Action OnAttackFinished;
    public void PublishAttackFinished() => OnAttackFinished?.Invoke();

    public event Action OnFootstep;
    public void PublishFootstep() => OnFootstep?.Invoke();

    public event Action OnDodgeEnd;
    public void PublishDodgeEnd() => OnDodgeEnd?.Invoke();
}
