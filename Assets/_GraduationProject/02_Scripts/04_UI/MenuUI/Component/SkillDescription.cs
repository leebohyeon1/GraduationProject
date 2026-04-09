using TMPro;
using UnityEngine;

public class SkillDescription : MonoBehaviour
{
    [SerializeField] private TMP_Text _skillName;
    [SerializeField] private TMP_Text _skillDescription;
    [SerializeField] private TMP_Text _moneyAmount;
    [SerializeField] private TMP_Text _specialmoneyAmount;

    public void SetDescription(string skillName, string skillDescription, string moneyAmount, string specialMoneyAmount)
    {
        _skillName.text = skillName;
        _skillDescription.text = skillDescription;
        _moneyAmount.text = moneyAmount;
        _specialmoneyAmount.text = specialMoneyAmount;
    }
}
