using System;
using UnityEngine;

public interface IInteractable
{
    public void Interact();

    public event Action<bool> OnPlayerScan;
    public event Action OnInteract;
}
