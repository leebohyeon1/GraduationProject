using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public enum TitleState
{
    None = -1,
    TitlePrompt = 0,
    MainMenu = 1,
    SelectData = 2,
    Setting = 3
}

/// <summary>
/// 타이틀을 관리하는 클래스
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<CinemachineCamera> _cameraList;
    [SerializeField] private InputReaderSO _inputReader;

    [Header("State")]
    [SerializeField] private TitleState _currentState = TitleState.None;
    public event UnityAction<TitleState> TitleStateChanged;  // 타이틀 상태 변경 이벤트

    private void Start()
    {
        Initialize();
        SetState(TitleState.TitlePrompt);
    }

    private void OnDestroy()
    {
        _inputReader.AnyKeyEvent -= OnAnyKeyEvent;
    }


    private void Initialize()
    {
        _inputReader.AnyKeyEvent += OnAnyKeyEvent;

        // UI 전용으로 바꿈
        _inputReader.SetInputMode(InputReaderSO.InputMode.UI);
    }

    /// <summary>
    /// 카메라 설정
    /// </summary>
    /// <param name="state">현재 상태</param>
    private void SetState(TitleState state)
    {
        // 같은 상태면 리턴
        if(_currentState == state)
        {
            return;
        }

        _currentState = state;
        UpdateCamera(); // 카메라 업데이트

        TitleStateChanged?.Invoke(_currentState);
    }

    /// <summary>
    /// 카메라 업데이트
    /// </summary>
    private void UpdateCamera()
    {
        //for (int i = 0; i < _cameraList.Count; i++)
        //{
        //    if (i == (int)_currentState)
        //    {
        //        _cameraList[i].Priority = 99;
        //    }
        //    else
        //    {
        //        _cameraList[i].Priority = 0;
        //    }
        //}
    }

    /// <summary>
    /// 아무키나 입력했을 때 이벤트
    /// </summary>
    private void OnAnyKeyEvent()
    {
        if(_currentState == TitleState.TitlePrompt)
        {
            SetState(TitleState.MainMenu);
        }
        
    }

    /// <summary>
    /// 메인 메뉴로 가는 함수
    /// </summary>
    public void GotoManiMenu()
    {
        if (_currentState == TitleState.SelectData || _currentState == TitleState.Setting)
        {
            SetState(TitleState.MainMenu);
        }
    }

    //====================================================================================================================
    // Event Handler =====================================================================================================
    //====================================================================================================================

    /// <summary>
    /// 새 게임 버튼 눌렀을 때 
    /// </summary>
    public void OnNewGameButton()
    {
        if (_currentState == TitleState.MainMenu)
        {
            SetState(TitleState.SelectData);
        }
    }

    /// <summary>
    /// 이어하기 버튼 눌렀을 때
    /// </summary>
    public void OnContinueButton()
    {
        if (_currentState == TitleState.MainMenu)
        {
            SetState(TitleState.SelectData);
        }
    }

    /// <summary>
    /// 게임 종료 버튼 눌렀을 때
    /// </summary>
    public void OnQuitGameButton()
    {
        if (_currentState == TitleState.MainMenu)
        {
            Application.Quit();
        }
    }

    public void OnSettingButton()
    {
        if (_currentState == TitleState.MainMenu)
        {
            SetState(TitleState.Setting);
        }
    }
}
