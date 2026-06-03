using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Image _sceneImage;
    [SerializeField] private SceneDataSO _defaultSceneDataSO;
    public SceneDataSO DefaultSceneDataSO => _defaultSceneDataSO;

    private bool _isDataSet = false;
    private string _loadSceneName = "";
    public GameData GameData { get; private set; } = null;

    public void SetData(int index, GameData data)
    {
        _indexText.text = (index + 1).ToString();

        if (data == null)
        {
            _saveDataPanel.SetActive(false); // 세이브 정보 패널 끄기
            GameData = null;                 // 데이터 초기화 (버튼 재사용 시 중요)

            _isDataSet = false;
            return;
        }

        _saveDataPanel.SetActive(true);
        GameData = data; 

        _saveTimeText.text = GameData.LastSaveTime;
        _moneyText.text = GameData.PlayerData.Money.ToString();

        SceneDataSO sceneData = null;
        if (SceneLoadingManager.Instance != null)
        {
            sceneData = SceneLoadingManager.Instance.GetSceneDataByName(GameData.LastMainScene);
        }

        if (sceneData != null)
        {
            _stageText.text = sceneData.StageName;
            _sceneImage.sprite = sceneData.StageImage;
        }
        else if (_defaultSceneDataSO != null)
        {
            _stageText.text = _defaultSceneDataSO.StageName;
            _sceneImage.sprite = _defaultSceneDataSO.StageImage;
        }
        else
        {
            _stageText.text = "Unknown";
        }

        _loadSceneName = GameData.LastMainScene;

        _isDataSet = true;
    }

    public void LoadScene()
    {
        if(!_isDataSet)
        {
            SceneLoadingManager.Instance.TeleportToSceneByName(_defaultSceneDataSO.SceneName, SceneLoadingManager.SpawnMode.Default);
        }
        else
        {
            SceneLoadingManager.Instance.TeleportToSceneByName(_loadSceneName, SceneLoadingManager.SpawnMode.LastPosition);
        }
    }
}
