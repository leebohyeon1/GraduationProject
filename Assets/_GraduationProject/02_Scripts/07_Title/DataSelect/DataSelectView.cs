using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataSelectView : TitleView
{
    [SerializeField] private GameObject _dataSelectPrefab;
    [SerializeField] private Transform _content;

    [SerializeField] private List<GameObject> _dataSelectButtonList = new List<GameObject>();

    [SerializeField] private GameObject _dataCheckBox;
    private int _selectedIndex = -1;

    private void OnEnable()
    {
        Initialize();

    }

    private void Initialize()
    {
        // 1. 버튼 갯수가 모자라면 추가 생성
        if (_dataSelectButtonList.Count < DataManager.Instance.DataList.Count)
        {
            int needCount = DataManager.Instance.DataList.Count - _dataSelectButtonList.Count;
            for (int i = 0; i < needCount; i++)
            {
                GameObject obj = Instantiate(_dataSelectPrefab, _content);
                _dataSelectButtonList.Add(obj);
            }
        }

        // 2. 존재하는 데이터만큼 버튼에 데이터 세팅
        for (int i = 0; i < DataManager.Instance.DataList.Count; i++)
        {
            GameData gameData = DataManager.Instance.DataList[i];
            _dataSelectButtonList[i].GetComponent<DataSelectButtonTrigger>().SetData(i, gameData);
        }

        // 3. 데이터가 없는 남은 버튼들은 비어있음(null) 처리
        if (DataManager.Instance.DataList.Count < _dataSelectButtonList.Count)
        {
            // 수정된 부분: 시작 인덱스를 DataList.Count로 맞추고, 리스트에서 꺼내오는 로직 제거
            for (int i = DataManager.Instance.DataList.Count; i < _dataSelectButtonList.Count; i++)
            {
                _dataSelectButtonList[i].GetComponent<DataSelectButtonTrigger>().SetData(i, null);
            }
        }

        // 4. 모든 버튼에 데이터가 꽉 찼으면 새 게임을 위한 빈 버튼 하나 추가
        if (DataManager.Instance.DataList.Count == _dataSelectButtonList.Count)
        {
            GameObject obj = Instantiate(_dataSelectPrefab, _content);
            // 이 새로 생성된 버튼의 SetData는 따로 안 해줘도 괜찮은지 확인이 필요할 수 있습니다.
            // 필요하다면 아래 코드를 추가하세요:
            // obj.GetComponent<DataSelectButtonTrigger>().SetData(_dataSelectButtonList.Count, null);
            _dataSelectButtonList.Add(obj);
        }
    }

    /// <summary>
    /// 이어하기 버튼 클릭
    /// </summary>
    public void OnContinueButton()
    {
        for (int i = 0; i < _dataSelectButtonList.Count; i++)
        {
            int index = i;
            Button btn = _dataSelectButtonList[index].gameObject.GetComponent<Button>();

            btn.onClick.RemoveAllListeners(); // 중복 등록 방지를 위해 기존 이벤트 지우기
            btn.onClick.AddListener(() =>
            {
                    if (_dataSelectButtonList[index].GetComponent<DataSelectButtonTrigger>().GameData != null)
                    {
                        DataManager.Instance.SelectSaveData(index);
                        SceneManager.LoadScene("Part3 1");
                    }
                });
        }
    }

    /// <summary>
    /// 새 게임 버튼 클릭
    /// </summary>
    public void OnNewGameButton()
    {
        for(int i = 0; i < _dataSelectButtonList.Count; i++)
        {
            int index = i;
            Button btn = _dataSelectButtonList[index].gameObject.GetComponent<Button>();

            btn.onClick.RemoveAllListeners(); // 중복 등록 방지를 위해 기존 이벤트 지우기
            btn.onClick.AddListener(() =>
            {
                    if (_dataSelectButtonList[index].GetComponent<DataSelectButtonTrigger>().GameData == null)
                    {
                        CreateNewGame();
                    }
                    else
                    {
                        CheckBoxOn(index);
                    }
                });
        }
    }

    public void CreateNewGame()
    {
        DataManager.Instance.CreateNewGame();
        SceneManager.LoadScene("Part3 1");
    }
    
    public void OverwriteData()
    {
        DataManager.Instance.CreateNewGame(_selectedIndex);
        SceneManager.LoadScene("Part3 1");
    }

    public void CheckBoxOn(int index)
    {
        _selectedIndex = index;
        _dataCheckBox.SetActive(true);
    }

    public void CheckBoxOff()
    {
        _dataCheckBox.SetActive(false);
    }
}
