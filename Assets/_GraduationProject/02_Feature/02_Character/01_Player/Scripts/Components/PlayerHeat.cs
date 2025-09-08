using UnityEngine;

/// <summary>
/// 플레이어 열량 시스템 클래스
/// HeatSystem을 상속받아 플레이어의 열량 관리를 담당합니다.
/// 공격, 스킬 사용 등에 따라 열량이 축적되고, 티어별로 스탯 변화가 적용됩니다.
/// </summary>
public class PlayerHeat : HeatSystem
{
    /// <summary>플레이어 컨텍스트 참조</summary>
    private PlayerContext _context;

    /// <summary>
    /// 플레이어 열량 시스템 초기화
    /// </summary>
    /// <param name="context">플레이어 컨텍스트</param>
    public void Initialize(PlayerContext context)
    {
        _context = context;

        // TODO: 열량 시스템 이벤트 구독
        // TODO: 플레이어 ID로 열량 데이터 초기화

        _context.EventBus.OnAttack += AddHeatOnMeleeAttack;
    }


    private void AddHeatOnMeleeAttack(Collider[] targets)
    {
        foreach (Collider target in targets)
        {
            IHeatable heatable = target.GetComponent<IHeatable>();
            if (heatable != null)
            {
                SourceMap sourceMap = p_heatDataBase.GetSourceMap("OnMeleeHit", heatable.ActorType, CurrentTier);
                int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
                heatable.ChangeHeat(deltaHeat);
            }
        }
    }

    public void Oestroy()
    {
        if (_context?.EventBus != null)
        {
            _context.EventBus.OnAttack -= AddHeatOnMeleeAttack;
        }
    }
}
