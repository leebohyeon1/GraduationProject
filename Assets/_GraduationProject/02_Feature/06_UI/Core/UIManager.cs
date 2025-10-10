using UnityEngine;
using BH_Lib.DI;
using System.Collections.Generic;
using System;

[Register(LifetimeScope.Singleton)]
public class UIManager : MonoBehaviour
{
    [SerializeField] private UIInputHandler _input;
    private Stack<PopUpUI> _popUpUIStack;

    public event Action OnOpenPopUpUI;
    public event Action OnClearPopUpUI;

    private void Start()
    {
        if(_input == null)
        {
            _input = GetComponent<UIInputHandler>();   
        }

        _popUpUIStack = new Stack<PopUpUI>();
    }

    private void Update()
    {
        if (_input.CancelInput)
        {
            CloseTopUI();
        }
    }

    private void LateUpdate()
    {
        _input.LateTick();
    }

    public void OpenUI(PopUpUI popUpUI)
    {
        if (_popUpUIStack.Count == 0)
        {
            OnOpenPopUpUI?.Invoke();
        }

        _popUpUIStack.Push(popUpUI);
    }

    public void CloseTopUI()
    {
        if(_popUpUIStack.Count > 0)
        {
            PopUpUI popUpUI = _popUpUIStack.Pop();
            if (popUpUI != null)
            {
                popUpUI.CloseUI();
            }

            if(_popUpUIStack.Count == 0)
            {
                OnClearPopUpUI?.Invoke();
            }
        }
    }
}
