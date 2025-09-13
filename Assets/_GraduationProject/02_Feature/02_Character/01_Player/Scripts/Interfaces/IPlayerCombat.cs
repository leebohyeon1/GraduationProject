using UnityEngine;

/// <summary>
/// 플레이어의 기본 공격을 제외한 전투 인터페이스
/// </summary>
public interface IPlayerCombat
{
    public void TryParry();

    public void TryCounterAttack();
    
}
