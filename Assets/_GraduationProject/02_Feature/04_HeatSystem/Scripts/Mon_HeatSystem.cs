using UnityEngine;
public class Monster_HeatSystem : HeatSystem
{
    private Enemy _enemy;
    protected override void OverHeat()
    {
        _currentHeat = 0;
        _enemy.ApplyStun(3);
        _enemy.TakeDamage(10, 3, true); // 과열 시 10 데미지 3초 유지);
    }
    public override void Init(ActorType actorType)
    {
        base.Init(actorType);
        _enemy = GetComponent<Enemy>();
    }
}