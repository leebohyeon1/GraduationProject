using UnityEngine;

/// <summary>Restores the unmodified bone pose before Ragdoll Animator (-1), physics and Animator evaluate it.</summary>
[DefaultExecutionOrder(-10001)]
public sealed class EnemyIntroPoseRestore : MonoBehaviour
{
    [System.NonSerialized] public EnemyIntroVisualRise owner;
    private void Update() { if (owner) owner.RemoveOffset(); }
    private void FixedUpdate() { if (owner) owner.RemoveOffset(); }
    private void OnDisable() { if (owner) owner.RemoveOffset(); }
}
