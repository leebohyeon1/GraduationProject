using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySelectUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private AbilityCardUI[] _cards;

    // SurvivorLikeManager가 직접 할당해줄 프로퍼티
    public SurvivorLikeManager Manager { get; set; }
    [SerializeField] private AbilitySelectedSO _abilitySelectedSO;

    private void Awake()
    {
        if (_panel == null) _panel = gameObject;

        foreach (var card in _cards)
        {
            card.OnCardSelected += OnCardSelection;
        }

        _panel.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (var card in _cards)
        {
            card.OnCardSelected -= OnCardSelection;
        }
    }

    /// <summary>
    /// 능력 선택창을 보여줍니다.
    /// </summary>
    /// <param name="abilities">보여줄 능력 데이터 리스트</param>
    public void Show(List<AbilitySO> abilities)
    {
        if (abilities.Count != _cards.Length)
        {
            Debug.LogError($"능력 데이터 개수({abilities.Count})가 카드 UI 개수({_cards.Length})와 맞지 않습니다.");
            return;
        }

        for (int i = 0; i < _cards.Length; i++)
        {
            _cards[i].SetAbility(abilities[i]);
        }

        _panel.SetActive(true);
    }
    
    /// <summary>
    /// 능력 선택창을 숨깁니다.
    /// </summary>
    public void Hide()
    {
        _panel.SetActive(false);
    }

    /// <summary>
    /// 카드 중 하나가 선택되었을 때 호출됩니다.
    /// </summary>
    private void OnCardSelection(AbilitySO chosenAbility)
    {
        _abilitySelectedSO.Publish(chosenAbility);
        Hide();
    }
}

