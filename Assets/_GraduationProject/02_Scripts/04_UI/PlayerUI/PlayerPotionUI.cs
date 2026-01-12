using UnityEngine;

public class PlayerPotionUI : MonoBehaviour
{
    [SerializeField] private PlayerPotion _playerPotion;
    [SerializeField] private GameObject[] potionUIObjects;

    private void OnEnable()
    {
        if(_playerPotion == null)
        {
            _playerPotion = GameObject.FindFirstObjectByType<PlayerPotion>();
        }

        _playerPotion.OnPotionChange += OnPotionChange;

        OnPotionChange(_playerPotion.CurrentPotion);
    }

    private void OnDisable()
    {
        _playerPotion.OnPotionChange -= OnPotionChange;
    }

    private void OnPotionChange(int currentPotion)
    {
        for (int i = 0; i < potionUIObjects.Length; i++)
        {
            if(i < currentPotion)
            {
                potionUIObjects[i].SetActive(true);
            }
            else
            {
                potionUIObjects[i].SetActive(false);
            }
        }
    }
}
