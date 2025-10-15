using BH_Lib.DI;
using System;
using System.Collections.Generic;
using UnityEngine;

[Register(LifetimeScope.Singleton)]
public class UIManager : MonoBehaviour
{
    [SerializeField] private UIInputHandler _input;
    [SerializeField] private EventListener _popUpOpenEventListener;
    private Stack<PopUpUI> _popUpUIStack;
    private PopUpUI _currentPopUpUI;

    public event Action OnOpenFirstPopUpUI;
    public event Action OnClearPopUpUI;

    private void Start()
    {
        if(_input == null)
        {
            _input = GetComponent<UIInputHandler>();   
        }

        if( _popUpOpenEventListener == null)
        {
            _popUpOpenEventListener = GetComponent<EventListener>();
        }

        _popUpOpenEventListener.EventMessage.AddListener((popUpObject) => { OpenPopUp(popUpObject.GetComponent<PopUpUI>()); });
        _popUpUIStack = new Stack<PopUpUI>();
    }

    private void Update()
    {
        HandlePopUp();
    }

    private void LateUpdate()
    {
        _input.LateTick();
    }

    private void OnDestroy()
    {
        _popUpOpenEventListener.EventMessage.RemoveAllListeners();
    }

    #region PopUp
    public void OpenPopUp(PopUpUI popUpUI)
    {
        if (_popUpUIStack.Count == 0)
        {
            OnOpenFirstPopUpUI?.Invoke();
        }

        _popUpUIStack.Push(popUpUI);
        _currentPopUpUI = _popUpUIStack.Peek();
    }

    public void CloseTopPopUp()
    {
        if(_popUpUIStack.Count > 0)
        {
            PopUpUI popUpUI = _popUpUIStack.Pop();
            if (popUpUI != null)
            {
                popUpUI.ClosePopUp();
            }

            if (_popUpUIStack.Count == 0)
            {
                OnClearPopUpUI?.Invoke();
                _currentPopUpUI = null;
            }
            else
            {
                _currentPopUpUI = _popUpUIStack.Peek();
            }
        }
    }

    public void HandlePopUp()
    {
        if (_currentPopUpUI == null)
        {
            return;
        }

        if (_input.CancelInput)
        {
            CloseTopPopUp();
            return;
        }

        switch (_currentPopUpUI.Type)
        {
            case PopUpType.SkillEnchant:

                break;
        }
    }
    #endregion
}
