using UnityEngine;

public class PlayerContext
{
    public IPlayerMovement Movement { get; private set; }
    public IPlayerAttack Attack { get; private set; }
    public IPlayerHealth Health { get; private set; }
    public IPlayerController Controller { get; private set; }
    public PlayerStatsSO Stats { get; private set; }
    public Animator Animator { get; private set; }
    public PlayerEventBus EventBus { get; private set; }
    public IInputDeviceDetector InputDeviceDetector { get; private set; }


    public PlayerContext(IPlayerMovement movement, IPlayerAttack attack, IPlayerHealth health, IPlayerController controller, PlayerStatsSO stats, Animator animator, IInputDeviceDetector inputDeviceDetector)
    {
        Movement = movement;
        Attack = attack;
        Health = health;
        Controller = controller;
        Stats = stats;
        Animator = animator;
        InputDeviceDetector = inputDeviceDetector;
        
        EventBus = new PlayerEventBus();
    }
}
