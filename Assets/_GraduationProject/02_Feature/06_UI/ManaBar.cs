using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    [SerializeField] private List<Image> _manaLists;
    [SerializeField] private PlayerMana _playerMana;

    private void OnEnable()
    {
        if(_playerMana == null)
        {
            _playerMana = FindFirstObjectByType<PlayerMana>();
        }


        _playerMana.OnManaChange += UpdateUI;
    }

    private void OnDisable()
    {
        _playerMana.OnManaChange -= UpdateUI;
    }

    private void UpdateUI(int currentMana, int maxMana)
    {
        for (int i = 0; i < _manaLists.Count; i++)
        {
            if(i < currentMana)
            {
                _manaLists[i].gameObject.SetActive(true);
            }
            else
            {
                _manaLists[i].gameObject.SetActive(false);
            }

        }
    }
}
