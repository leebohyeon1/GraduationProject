using System.Collections.Generic;
using UnityEngine;

public class PlayerCloth : MonoBehaviour
{
    [SerializeField] private List<GameObject> _weaponList;
    [SerializeField] private GamePlayTagSO _wearWeapon;

    private void Start()
    {
        UpdateCloth();
    }

    private void UpdateCloth()
    {
        bool hasWeapon = GamePlayTagManager.Instance.HasTag(_wearWeapon.ID);

        foreach (var weapon in _weaponList)
        {
            if (weapon != null)
                weapon.SetActive(hasWeapon);
        }
    }
}
