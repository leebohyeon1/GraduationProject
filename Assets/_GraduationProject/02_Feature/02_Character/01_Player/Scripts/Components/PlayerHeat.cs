using UnityEngine;

public class PlayerHeat : HeatSystem
{
    /// <summary>플레이어 컨텍스트 참조</summary>
    private PlayerContext _context;

    /// <summary>
    /// 플레이어 체력 시스템 초기화
    /// </summary>
    /// <param name="context">플레이어 컨텍스트</param>
    public void Initialize(PlayerContext context)
    {
        _context = context;

    }
}
