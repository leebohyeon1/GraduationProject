using BH_Lib.Log;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
    protected Player p_player;
    protected PlayerStats p_playerStats;

    public virtual void Initialize(Player player)
    {
        p_player = player;
        p_playerStats = player.PlayerStats;

        Log.Print("PlayerComponent initialized.");
    }
}
