using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataSelectView : TitleView
{
    [SerializeField] private GameObject _dataSelectPrefab;
    [SerializeField] private Transform _content;

    [SerializeField] private List<GameObject> _dataSelectButtonList = new List<GameObject>();

    [SerializeField] private GameObject _dataCheckBox;

    private void OnEnable()
    {
        Initialize();

    }

    private void Initialize()
    {
        for (int i = 0; i < DataManager.Instance.DataList.Count; i++)
        {
            // 저장된 데이터량이 리스트량보다 많으면 오브젝트 생성
            if (i > _dataSelectButtonList.Count)
            {
                GameObject obj = Instantiate(_dataSelectPrefab, _content);
                _dataSelectButtonList.Add(obj);
            }

            GameData gameData = DataManager.Instance.DataList[i];
            _dataSelectButtonList[i].GetComponent<DataSelectButtonTrigger>().SetData(i, gameData);
        }

        // 모든 버튼에 데이터가 있으면 데이터 추가
        if(DataManager.Instance.DataList.Count == _dataSelectButtonList.Count)
        {
            GameObject obj = Instantiate(_dataSelectPrefab, _content);
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
            _dataSelectButtonList[index].gameObject.GetComponent<Button>()
                .onClick.AddListener(() => 
                {
                    if (_dataSelectButtonList[index].GetComponent<DataSelectButtonTrigger>().GameData != null)
                    {
                        DataManager.Instance.SelectSaveData(index);
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

            _dataSelectButtonList[index].gameObject.GetComponent<Button>()
                .onClick.AddListener(() =>
                {
                    if (_dataSelectButtonList[index].GetComponent<DataSelectButtonTrigger>().GameData == null)
                    {
                        CreateNewGame();
                    }
                    else
                    {
                        CheckBoxOn();
                    }
                });
        }
    }

    public void CreateNewGame()
    {
        DataManager.Instance.CreateNewGame();
    }

    public void CheckBoxOn()
    {
        _dataCheckBox.SetActive(true);
    }

    public void CheckBoxOff()
    {
        _dataCheckBox.SetActive(false);
    }
}
