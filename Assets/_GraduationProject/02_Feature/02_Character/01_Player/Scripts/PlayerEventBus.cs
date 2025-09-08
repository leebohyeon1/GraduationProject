using System;
using Unity.Profiling;

/// <summary>
/// 플레이어 이벤트 버스 클래스
/// 플레이어의 애니메이션 이벤트와 게임 로직 간의 통신을 담당합니다.
/// Observer 패턴을 사용하여 느슨한 결합을 제공합니다.
/// </summary>
public class PlayerEventBus
{
    /// <summary>체력 변경 이벤트 (이전 체력, 현재 체력)</summary>
    public event Action<int, int> OnHealthChanged;
    public void PublishHealthChanged(int previous, int current) => OnHealthChanged?.Invoke(previous, current);

    /// <summary>플레이어 사망 이벤트</summary>
    public event Action OnPlayerDied;
    public void PublishPlayerDied() => OnPlayerDied?.Invoke();

    /// <summary>공격 입력 허용 이벤트 (애니메이션 이벤트에서 호출)</summary>
    public event Action OnAllowAttackInput;
    public void PublishAllowAttackInput() => OnAllowAttackInput?.Invoke();

    /// <summary>공격 실행 이벤트 (애니메이션 이벤트에서 호출)</summary>
    public event Action OnAttack;
    public void PublishAttack() => OnAttack?.Invoke();

    /// <summary>공격 완료 이벤트 (애니메이션 이벤트에서 호출)</summary>
    public event Action OnAttackFinished;
    public void PublishAttackFinished() => OnAttackFinished?.Invoke();

    /// <summary>발걸음 소리 이벤트 (애니메이션 이벤트에서 호출)</summary>
    public event Action OnFootstep;
    public void PublishFootstep() => OnFootstep?.Invoke();

    /// <summary>회피 애니메이션 종료 이벤트 (애니메이션 이벤트에서 호출)</summary>
    public event Action OnDodgeEnd;
    public void PublishDodgeEnd() => OnDodgeEnd?.Invoke();

    /// <summary>
    /// 패링 이벤트
    /// </summary>
    public event Action OnParry;
    public void PublishParry() => OnParry?.Invoke();

    /// <summary>원거리 공격 시작 이벤트</summary>
    public event Action OnRangedAttackStart;
    public void PublishRangedAttackStart() => OnRangedAttackStart?.Invoke();

    /// <summary>원거리 공격 종료 이벤트</summary>
    public event Action OnRangedAttackEnd;
    public void PublishRangedAttackEnd() => OnRangedAttackEnd?.Invoke();



    public void Dispose()
    {
        OnHealthChanged = null;
        OnPlayerDied = null;
        OnAllowAttackInput = null;
        OnAttack = null;
        OnAttackFinished = null;
        OnFootstep = null;
        OnDodgeEnd = null;
        OnParry = null;
        OnRangedAttackStart = null;
        OnRangedAttackEnd = null;
    }


}
