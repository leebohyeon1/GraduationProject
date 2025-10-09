using UnityEngine;
using BH_Lib.DI;
using System.Collections.Generic;

[Register(LifetimeScope.Singleton)]
public class UIManager : MonoBehaviour
{
    private Stack<PopUpUI> _popUpUIStack;

    private void Start()
    {
        _popUpUIStack = new Stack<PopUpUI>();
    }

    public void RegisterUI(PopUpUI popUpUI)
    {
        _popUpUIStack.Push(popUpUI);
    }

}
