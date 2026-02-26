using System;
using UnityEngine;

public enum InteractableType
{
    None,
    NPC,
    Environment
}

public interface IInteractable
{
    public Transform InteractableUITransform {  get; }
    public InteractableType InteractableType { get; }
    public void Interact();
}
