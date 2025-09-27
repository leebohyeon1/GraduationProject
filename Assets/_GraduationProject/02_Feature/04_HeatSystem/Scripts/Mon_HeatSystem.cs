using UnityEngine;
public class Monster_HeatSystem : HeatSystem
{
    private Enemy _enemy;
    [SerializeField] private int _overheatTime = 2;
    protected override void OverHeat()
    {
        SetHeat(0);
        _enemy.ApplyStun(_overheatTime);
        _enemy.TakeDamage(_enemy.Maxhealth / 10); // 과열 시 10 데미지 3초 유지);
        Debug.Log($"과열 시 데미지: {_enemy.Maxhealth / 10}");
    }
    public override void Init(ActorType actorType)
    {
        base.Init(actorType);
        _enemy = GetComponent<Enemy>();
    }
}