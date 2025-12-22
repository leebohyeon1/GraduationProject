using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCardUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _selectButton;

    private AbilitySO _ability;

    public event Action<AbilitySO> OnCardSelected;

    private void Start()
    {
        _selectButton.onClick.AddListener(OnSelection);
    }

    private void OnDestroy()
    {
        _selectButton.onClick.RemoveListener(OnSelection);
    }

    /// <summary>
    /// 카드의 UI를 AbilitySO 데이터에 맞춰 설정합니다.
    /// </summary>
    public void SetAbility(AbilitySO ability)
    {
        _ability = ability;

        if (_iconImage != null) _iconImage.sprite = ability.Icon;
        if (_nameText != null) _nameText.text = ability.AbilityName;
        if (_descriptionText != null) _descriptionText.text = ability.Description;
    }

    private void OnSelection()
    {
        OnCardSelected?.Invoke(_ability);
    }
}
