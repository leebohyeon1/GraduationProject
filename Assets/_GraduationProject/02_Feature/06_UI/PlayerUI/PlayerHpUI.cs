using BH_Lib.Log;
using UnityEngine;

public class PlayerHpUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private GameObject[] _hpImage;

    private void OnEnable()
    {
        _playerHealth.OnHealthChanged += UpdateHpUI;
    }

    private void OnDisable()
    {
        _playerHealth.OnHealthChanged -= UpdateHpUI;
    }   

    private void UpdateHpUI(int previousHp, int currentHp)
    {
        // Update the UI elements to reflect the current HP

        for(int i = 0; i < _hpImage.Length; i++)
        {
            if ((float)(i) / _hpImage.Length < (float)currentHp /_playerHealth.MaxHealth)
            {
                _hpImage[i].SetActive(true);
            }
            else
            {
                _hpImage[i].SetActive(false);
            }
        }
    }
}
