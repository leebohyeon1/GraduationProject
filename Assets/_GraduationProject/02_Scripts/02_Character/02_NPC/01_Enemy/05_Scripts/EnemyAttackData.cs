using UnityEngine;
public enum AttackShape
{
    Sphere,
    Box,
    Fan
}
[CreateAssetMenu(fileName = "EnemyAttackData", menuName = "EnemyAttackData", order = 0)]
public class EnemyAttackData : ScriptableObject {
    public string AttackName;
    public DamageData damageData;
    public float Cooltime = 3f;
    [Header("Attack Shape Settings")]
    public AttackShape shape = AttackShape.Sphere;
    public float damageRadius = 2.0f;
    public Vector3 attackOffset;
    public int Phase = 0;
    [Header("Box Settings")]
    public Vector3 boxSize = Vector3.one;
    [Header("Fan Settings")]
    [Range(0, 360)]
    public float fanAngle = 90f;
    public float sizeY = 1.0f;
}