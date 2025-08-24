using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 각 기능 컴포넌트들의 기반이 되는 클래스입니다.
/// Player 클래스와 PlayerStats에 대한 참조를 제공하여 중복 코드를 줄입니다.
/// </summary>
public class PlayerComponent : MonoBehaviour
{
    /// <summary>
    /// 이 컴포넌트가 속한 Player 객체입니다.
    /// </summary>
    protected Player p_player;
    
    /// <summary>
    /// 플레이어의 스탯(능력치) 데이터입니다.
    /// </summary>
    protected PlayerStats p_playerStats;

    /// <summary>
    /// 컴포넌트를 초기화합니다. Player 객체에 의해 호출됩니다.
    /// </summary>
    /// <param name="player">이 컴포넌트를 소유하는 Player 객체입니다.</param>
    public virtual void Initialize(Player player)
    {
        p_player = player;
        p_playerStats = player.PlayerStats;

        Log.Print($"{GetType().Name} initialized.");
    }
}