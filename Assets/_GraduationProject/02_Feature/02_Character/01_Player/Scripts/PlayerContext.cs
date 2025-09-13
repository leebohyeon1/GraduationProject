using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 모든 컴포넌트와 시스템에 대한 참조를 관리하는 컨텍스트 클래스
/// 각 상태 클래스에서 플레이어의 기능에 접근할 때 사용됩니다.
/// </summary>
public class PlayerContext
{
    /// <summary>
    /// 플레이어 게임 오브젝트의 MonoBehaviour 컴포넌트
    /// </summary>
    public MonoBehaviour Owner { get; private set; }

    /// <summary>
    /// 플레이어 이동 인터페이스
    /// </summary>
    public IPlayerMovement Movement { get; private set; }

    /// <summary>
    /// 플레이어 공격 인터페이스
    /// </summary>
    public IPlayerMeleeAttack MeleeAttack { get; private set; }

    /// <summary>
    /// 플레이어 원거리 공격 인터페이스
    /// </summary>
    public IPlayerRangedAttack RangedAttack { get; private set; }

    /// <summary>
    /// 플레이어 전투 인터페이스
    /// </summary>
    public IPlayerCombat Combat { get; private set; }

    /// <summary>
    /// 플레이어 체력 인터페이스
    /// </summary>
    public IPlayerHealth Health { get; private set; }

    /// <summary>
    /// 플레이어 입력 컨트롤러 인터페이스
    /// </summary>
    public IPlayerController Controller { get; private set; }

    /// <summary>
    /// 플레이어 열량 시스템 인터페이스
    /// </summary>
    public IHeatable Heat { get; private set; }

    /// <summary>
    /// 플레이어 스탯 데이터 (ScriptableObject)
    /// </summary>
    public PlayerStatsSO Stats { get; private set; }

    /// <summary>
    /// 플레이어 애니메이터 컴포넌트
    /// </summary>
    public Animator Animator { get; private set; }

    /// <summary>
    /// 플레이어 이벤트
    /// </summary>
    public PlayerEventChannel Event { get; private set; }

    /// <summary>
    /// 입력 기기 감지기 (키보드/마우스, 게임패드 구분)
    /// </summary>
    public IInputDeviceDetector InputDeviceDetector { get; private set; }

    /// <summary>
    /// 플레이어 컨텍스트 생성자
    /// </summary>
    /// <param name="owner">플레이어 게임 오브젝트</param>
    /// <param name="movement">이동 시스템</param>
    /// <param name="meleeAttack">공격 시스템</param>
    /// <param name="rangeedAttack">원거리 공격 시스템</param>
    /// <param name="health">체력 시스템</param>
    /// <param name="controller">입력 컨트롤러</param>
    /// <param name="stats">플레이어 스탯 데이터</param>
    /// <param name="animator">애니메이터</param>
    /// <param name="inputDeviceDetector">입력 기기 감지기</param>
    public PlayerContext(MonoBehaviour owner, IPlayerMovement movement, IPlayerMeleeAttack meleeAttack,
    IPlayerRangedAttack rangeedAttack, IPlayerCombat combat,IPlayerHealth health, IPlayerController controller,
    IHeatable heat, PlayerStatsSO stats, Animator animator, IInputDeviceDetector inputDeviceDetector)
    {
        Owner = owner;
        Movement = movement;
        MeleeAttack = meleeAttack;
        RangedAttack = rangeedAttack;
        Combat = combat;
        Health = health;
        Controller = controller;
        Stats = stats;
        Animator = animator;
        InputDeviceDetector = inputDeviceDetector;
        Heat = heat;
        Event = new PlayerEventChannel();
    }

    /// <summary>
    /// 코루틴 시작 (Owner의 StartCoroutine 호출)
    /// </summary>
    public Coroutine StartCoroutine(IEnumerator routine) => Owner.StartCoroutine(routine);

    /// <summary>
    /// 코루틴 중지 (Owner의 StopCoroutine 호출)
    /// </summary>
    public void StopCoroutine(Coroutine routine) => Owner.StopCoroutine(routine);
    
}
