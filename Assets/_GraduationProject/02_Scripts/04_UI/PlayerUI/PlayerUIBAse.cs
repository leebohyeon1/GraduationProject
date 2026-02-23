using System;
using UnityEngine;

public class PlayerUIBase : MonoBehaviour, IDisposable
{
    protected PlayerController p_player;

    public virtual void Initialize(PlayerController player)
    {
        p_player = player;

        player.RegisterDisposable(this);
    }

    public virtual void Dispose()
    {

    }
}
