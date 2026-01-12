using System;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    protected Player p_player;

    public event Action<bool> OnPlayerScan;
    public event Action OnInteract;

    protected virtual void OnEnable()
    {
        if (p_player == null)
        {
            // p_player = DIContainer.Instance.Resolve<Player>();
        }
    }

    protected virtual void OnDisable()
    {
        if( p_player != null )
        {
            p_player.Interact.OnInteract -= Interact;
            OnPlayerScan?.Invoke(false);
        }
    }

    public virtual void Interact()
    {
        OnInteract?.Invoke();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (p_player != null && other.gameObject == p_player.gameObject)
        {
            p_player.Interact.OnInteract += Interact;
            OnPlayerScan?.Invoke(true);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if(p_player != null && other.gameObject == p_player.gameObject)
        {
            p_player.Interact.OnInteract -= Interact;
            OnPlayerScan?.Invoke(false);
        }
    }
}
