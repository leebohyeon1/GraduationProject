using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBaseDatasSO", menuName = "RefactorPlayer/PlayerBaseDatasSO")]
public class PlayerBaseDatasSO : ScriptableObject
{
    [Header("Stats")]
    public int MaxHealth = 100;
    public int MaxMana = 100;

    [Header("Movement")]
    public LayerMask GroundLayerMask = 1 << 3;
    public float MoveSpeed = 5f;
    public float RotateSpeed = 5f;
    public float Gravity = -9.81f;
    public float GroundCheckDistance = 0.1f;


    [Header("Combat")]
    public PlayerCombatData CombatData;
}
    



