using System;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    private Player _player;

    public event Action<bool> OnPlayerScan;
    public event Action OnInteract;

    private void OnDisable()
    {
        if( _player != null )
        {
            _player.Interact.OnInteract -= Interact;
            OnPlayerScan?.Invoke(false);
        }
    }

    public virtual void Interact()
    {
        OnInteract?.Invoke();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Player>(out var player))
        {
            _player = player;

            _player.Interact.OnInteract += Interact;
            OnPlayerScan?.Invoke(true);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if(_player != null && other.gameObject == _player.gameObject)
        {
            _player.Interact.OnInteract -= Interact;
            OnPlayerScan?.Invoke(false);
        }
    }
}
