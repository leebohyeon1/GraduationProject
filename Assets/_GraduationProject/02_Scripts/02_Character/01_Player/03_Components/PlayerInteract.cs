using System;
using UnityEngine;

public class PlayerInteract : MonoBehaviour, IDisposable
{
    private PlayerEvents _events;
    private PlayerData _data;
    private InputReaderSO _inputReader;

    private IInteractable _interactable;
    public IInteractable Interactable => _interactable;

    public event Action Interacted;
    public event Action<IInteractable> InteractableChanged;


    private bool _isBattleState = false;

    public void Initialize(PlayerController player)
    {
        _events = player.Events;
        _data = player.RuntimeData;
        _inputReader = player.InputReader;

        _inputReader.InteractEvent += Interact;
        _events.BattleStateChanged += OnBattleStateChanged;
        player.RegisterDisposable(this);
    }

    public void Dispose()
    {
        _inputReader.InteractEvent -= Interact;
        _events.BattleStateChanged -= OnBattleStateChanged;

        Interacted = null;
        InteractableChanged = null;
    }

    public void Interact()
    {
        if (_interactable == null) { return; }

        if (Interactable != null)
        {
            Debug.Log("상호작용");

            Interactable.Interact();
            Interacted?.Invoke();

            SetInteractable(null);
        }
    }

    private void OnBattleStateChanged(bool isBattle)
    {
        _isBattleState = isBattle;
    }

    public void SetInteractable(IInteractable interactable)
    {
        _interactable = interactable;

        if (Interactable != null)
        {
            InteractableChanged?.Invoke(Interactable);
        }
        else
        {
            InteractableChanged?.Invoke(null);
        }
    }
}
