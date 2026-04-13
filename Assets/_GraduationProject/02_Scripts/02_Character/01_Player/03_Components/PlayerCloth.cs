using System.Collections.Generic;
using UnityEngine;

public class PlayerCloth : MonoBehaviour
{
    [SerializeField] private List<GameObject> _scarfList;
    [SerializeField] private GamePlayTagSO _wearScarf;

    [SerializeField] private List<GameObject> _weaponList;
    [SerializeField] private GamePlayTagSO _wearWeapon;

    private void Start()
    {
        UpdateCloth();
    }

    private void UpdateCloth()
    {
        bool hasScarf = GamePlayTagManager.Instance.HasTag(_wearScarf);
        bool hasWeapon = GamePlayTagManager.Instance.HasTag(_wearWeapon);
        foreach (var scarf in _scarfList)
        {
            if (scarf != null)
                scarf.SetActive(hasScarf);
        }
        foreach (var weapon in _weaponList)
        {
            if (weapon != null)
                weapon.SetActive(hasWeapon);
        }
    }
}
