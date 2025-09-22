using UnityEngine;
public class Monster_HeatSystem : HeatSystem
{
    private Enemy _enemy;
    protected override void OverHeat()
    {
        _enemy.TakeDamage(10, 3); // 과열 시 10 데미지 3초 유지);
        _enemy.ApplyStun();
    }
    public override void Init(ActorType actorType)
    {
        base.Init(actorType);
        _enemy = GetComponent<Enemy>();
    }
}