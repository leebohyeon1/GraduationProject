using TMPro;
using UnityEngine;

/// <summary>
/// 데이터 선택 버튼
/// </summary>
public class DataSelectButtonTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _saveDataPanel;
    [SerializeField] private TMP_Text _indexText;   
    [SerializeField] private TMP_Text _saveTimeText;
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private TMP_Text _specialMoneyText;

    public GameData GameData { get; private set; } = null;

    public void SetData(int index, GameData data)
    {
        _indexText.text = (index + 1).ToString();

        if (data == null)
        {
            return;
        }

        _saveDataPanel.SetActive(true);
        GameData = data; 

        _saveTimeText.text = GameData.LastSaveTime;
        _stageText.text = GameData.StageName;
        _moneyText.text = GameData.PlayerData.Money.ToString();
        _specialMoneyText.text = GameData.PlayerData.Money.ToString();
    }
}
