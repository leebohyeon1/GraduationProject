using TMPro;
using UnityEngine;

/// <summary>
/// 데이터 선택 버튼
/// </summary>
public class DataSelectButtonTrigger : MonoBehaviour
{
    [SerializeField] private TMP_Text _indexText;   
    [SerializeField] private TMP_Text _saveTimeText;
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private TMP_Text _moneyText;

    public GameData GameData { get; private set; } = null;

    public void SetData(int index, GameData data)
    {
        _indexText.text = (index + 1).ToString();

        GameData = data; 

        _saveTimeText.text = GameData.LastSaveTime;
        _stageText.text = GameData.StageName;
        _moneyText.text = GameData.PlayerData.Money.ToString();
    }
}
