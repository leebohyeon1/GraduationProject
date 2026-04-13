using UnityEngine;

public class ObstacleTotem : TotemBase
{
    protected override void Start()
    {
        base.Start();
        _type = TotemType.Obstacle;
    }

    // 인스펙터의 TotemBase Settings -> Is Movable 체크 해제하면 고정형 벽이 됨
}
