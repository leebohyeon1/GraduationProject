using BH_Lib.DI;
using System;
using System.Collections.Generic;
using UnityEngine;

[Register(LifetimeScope.Singleton)]
public class UIManager : MonoBehaviour
{
    [SerializeField] private UIInputHandler _input;
    [SerializeField] private EventListener _listener;
    private Stack<PopUpUI> _popUpUIStack;


    public event Action OnOpenFirstPopUpUI;
    public event Action OnClearPopUpUI;

    private void Start()
    {
        if(_input == null)
        {
            _input = GetComponent<UIInputHandler>();   
        }

        if( _listener == null)
        {
            _listener = GetComponent<EventListener>();
        }

        _listener.EventMessage.AddListener((popUpObject) => { OpenPopUp(popUpObject.GetComponent<PopUpUI>()); });
        _popUpUIStack = new Stack<PopUpUI>();
    }

    private void Update()
    {
        if (_input.CancelInput)
        {
            CloseTopPopUp();
        }
    }

    private void LateUpdate()
    {
        _input.LateTick();
    
    }

    private void OnDestroy()
    {
        _listener.EventMessage.RemoveAllListeners();
    }


    public void OpenPopUp(PopUpUI popUpUI)
    {
        if (_popUpUIStack.Count == 0)
        {
            OnOpenFirstPopUpUI?.Invoke();
        }

        _popUpUIStack.Push(popUpUI);
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

            if(_popUpUIStack.Count == 0)
            {
                OnClearPopUpUI?.Invoke();
            }
        }
    }
}
