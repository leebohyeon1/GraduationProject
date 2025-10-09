using BH_Lib.DI;
using UnityEngine;

public class PopUpUI : MonoBehaviour
{
    protected UIManager p_uiManager;

    protected virtual void Start()
    {
        p_uiManager = DIContainer.Instance.Resolve<UIManager>();
    }

    protected virtual void CloseUI()
    {

    }
}
