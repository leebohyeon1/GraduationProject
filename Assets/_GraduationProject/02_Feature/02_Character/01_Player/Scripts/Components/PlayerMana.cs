using BH_Lib.Log;
using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour, IDisposable
{
    private PlayerEvents _events;
    private PlayerStats _stats;

    private bool _canChange = false;


    public int CurrentMana => _stats.CurrentMana;
    public int MaxMana => _stats.MaxMana;   

    public event Action<int, int> OnManaChange;

    public void Initialize(PlayerStats stats, PlayerEvents events)
    {
        _stats = stats;
        _events = events;

        _events.OnAttackStart += HandleAttackStart;
        _events.OnAttackAffect += HandleAttackAffect;
        _events.OnAttackFinish += HandleAttackFinish;
        _events.OnFlashStart += HandleFlashStart;

        OnManaChange?.Invoke(CurrentMana, MaxMana);
    }

    public void Dispose()
    {
        _events.OnAttackStart -= HandleAttackStart;
        _events.OnAttackAffect -= HandleAttackAffect;
        _events.OnAttackFinish -= HandleAttackFinish;
        _events.OnFlashStart -= HandleFlashStart;
    }

    private void ChangeMana(int amount)
    {
        if (!_canChange)
        {
            return;
        }

        _stats.CurrentMana = Mathf.Clamp(_stats.CurrentMana + amount, 0, _stats.MaxMana);
        OnManaChange?.Invoke(_stats.CurrentMana, _stats.MaxMana);
    }

    private void SetCanChange(bool canChange)
    {
        if (_canChange == canChange)
        {
            return ;
        }

        _canChange = canChange;
    }

    #region EventHandler

    private void HandleAttackStart()
    {
        Log.Print("시작");
        SetCanChange(true);
    }

    private void HandleAttackAffect(Collider collider)
    {
        Log.Print("적용");
        ChangeMana(1);
    }

    private void HandleAttackFinish()
    {
        Log.Print("종료");
        SetCanChange(false);
    }

    private void HandleFlashStart(Vector2 input)
    {
        SetCanChange(true);
        ChangeMana(-1);
        SetCanChange(false);
    }
    #endregion
}
