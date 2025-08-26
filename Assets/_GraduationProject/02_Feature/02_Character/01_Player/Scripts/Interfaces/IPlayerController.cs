using UnityEngine;

public interface IPlayerController
{
    Vector2 MoveInput { get; }
    bool AttackInput { get; }
    bool DodgeInput { get; }
    Vector2 LookInput { get; }
    Vector2 MousePosition { get; }
    void LateTick();
}
