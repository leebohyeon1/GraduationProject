using UnityEngine;

public class ObstacleTotem : TotemBase
{
    protected override void Start()
    {
        base.Start();
        _type = TotemType.Obstacle;
    }

    // 장애물 토템은 내구도 감소나 승리 조건 체크 없음
    // 단순히 이동만 함
}
