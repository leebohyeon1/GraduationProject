using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackData", menuName = "EnemyAttackData", order = 0)]
public class EnemyAttackData : ScriptableObject {
    public string AttackName;
    public float damageRadius;
    public Vector3 attackOffset;
    public DamageData damageData;

}