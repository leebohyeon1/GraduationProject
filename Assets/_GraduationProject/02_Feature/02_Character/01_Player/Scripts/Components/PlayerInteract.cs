using System;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public event Action OnInteract;

    public void Interact()
    {
        OnInteract?.Invoke();
    }
}
