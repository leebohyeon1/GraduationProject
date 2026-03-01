using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataSelectView : TitleView
{
    private enum SelectMode
    {
        NewGame,
        Continue
    }

    [Header("References")]
    [SerializeField] private GameObject _dataSelectPrefab;
    [SerializeField] private InputReaderSO _inputReader;

    [Header("UI")]
    [SerializeField] private Transform _content;
    private List<DataSelectButtonTrigger> _dataSelectButtonList = new List<DataSelectButtonTrigger>();

    [SerializeField] private GameObject _dataCheckBox;
    private int _selectedIndex = -1;
    private SelectMode _currentMode = SelectMode.NewGame;

    [Header("Event")]
    public UnityEvent OnCancelEvent;

    private void OnEnable()
    {
        _inputReader.CancelEvent += OnCancel;
    }

    private void Start()
    {
        Initialize();
    }

    private void OnDisable()
    {
        _inputReader.CancelEvent -= OnCancel;
    }

    private void Initialize()
    {
        // 기존 버튼 리스트 가져오기 (이미 씬에 있는 경우)
        _dataSelectButtonList = _content.GetComponentsInChildren<DataSelectButtonTrigger>().ToList();

        // 1. 데이터 개수에 맞춰 버튼 생성 및 리스트 확보
        int totalDataCount = DataManager.Instance.DataList.Count;
        
        // 버튼 갯수가 모자라면 추가 생성 (데이터 개수 + 새 게임용 빈 칸 1개)
        int requiredButtonCount = totalDataCount + 1;
        
        if (_dataSelectButtonList.Count < requiredButtonCount)
        {
            int needCount = requiredButtonCount - _dataSelectButtonList.Count;
            for (int i = 0; i < needCount; i++)
            {
                GameObject obj = Instantiate(_dataSelectPrefab, _content);
                _dataSelectButtonList.Add(obj.GetComponent<DataSelectButtonTrigger>());
            }
        }

        // 2. 모든 버튼 초기화 및 클릭 이벤트 등록
        for (int i = 0; i < _dataSelectButtonList.Count; i++)
        {
            int index = i;
            DataSelectButtonTrigger trigger = _dataSelectButtonList[i];
            
            // 데이터 설정 (범위 밖이면 null)
            GameData gameData = (i < totalDataCount) ? DataManager.Instance.DataList[i] : null;
            trigger.SetData(i, gameData);

            // 클릭 이벤트 등록
            Button btn = trigger.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnDataButtonClick(index));
            }
        }
    }

    /// <summary>
    /// 개별 데이터 버튼 클릭 시 실행될 공통 로직
    /// </summary>
    private void OnDataButtonClick(int index)
    {
        _selectedIndex = index;
        DataSelectButtonTrigger selectedTrigger = _dataSelectButtonList[index];

        if (_currentMode == SelectMode.Continue)
        {
            // 이어하기 모드: 데이터가 있을 때만 선택 가능
            if (selectedTrigger.GameData != null)
            {
                DataManager.Instance.SelectSaveData(index);
                selectedTrigger.LoadScene(); // 씬 로드 추가 (필요 시)
            }
        }
        else // NewGame 모드
        {
            // 새 게임 모드: 데이터가 없으면 즉시 생성, 있으면 덮어쓰기 확인창
            if (selectedTrigger.GameData == null)
            {
                CreateNewGame();
            }
            else
            {
                CheckBoxOn();
            }
        }
    }

    /// <summary>
    /// 이어하기 메뉴 진입 시 호출 (외부에서 연결)
    /// </summary>
    public void OnContinueButton()
    {
        _currentMode = SelectMode.Continue;
        Initialize(); // 버튼 상태 업데이트를 위해 다시 호출
    }

    /// <summary>
    /// 새 게임 메뉴 진입 시 호출 (외부에서 연결)
    /// </summary>
    public void OnNewGameButton()
    {
        _currentMode = SelectMode.NewGame;
        Initialize(); // 버튼 상태 업데이트를 위해 다시 호출
    }

    public void CreateNewGame()
    {
        DataManager.Instance.CreateNewGame();
        _dataSelectButtonList[_selectedIndex].LoadScene();
    }
    
    public void OverwriteData()
    {
        DataManager.Instance.CreateNewGame(_selectedIndex);
        _dataSelectButtonList[_selectedIndex].SetData(_selectedIndex, null);
        _dataSelectButtonList[_selectedIndex].LoadScene();
        CheckBoxOff();
    }

    public void CheckBoxOn()
    {
        _dataCheckBox.SetActive(true);
    }

    public void CheckBoxOff()
    {
        _dataCheckBox.SetActive(false);
    }

    private void OnCancel()
    {
        if(_dataCheckBox.activeSelf)
        {
            CheckBoxOff();
        }
        else
        {
            OnCancelEvent?.Invoke();
        }
    }
}
