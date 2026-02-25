using System;
using UnityEngine;

public class PlayerInteract : MonoBehaviour, IDisposable
{
    private PlayerEvents _events;
    private PlayerData _data;
    private InputReaderSO _inputReader;

    private IInteractable _interactable;
    public IInteractable Interactable => _interactable;

    public event Action OnInteract;


    public void Initialize(PlayerController player)
    {
        _events = player.Events;
        _data = player.RuntimeData;

        player.InputReader.InteractEvent += Interact;
        player.RegisterDisposable(this);
    }

    public void Dispose()
    {
        _inputReader.InteractEvent -= Interact;
    }

    public void Interact()
    {
        if (Interactable != null)
        {
            Debug.Log("상호작용");

            Interactable?.Interact();
            OnInteract?.Invoke();
        }
    }

    public void SetInteractable(IInteractable interactable)
    {
        _interactable = interactable;
    }
}
