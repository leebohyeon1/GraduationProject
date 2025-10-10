using BH_Lib.AssetManager;
using BH_Lib.DI;
using System;
using UnityEngine;

public class PopUpUI : MonoBehaviour
{
    [SerializeField] protected EventSO p_openPopUP;

    protected virtual async void  Start()
    {
        if(p_openPopUP == null)
        {
            AssetManager assetManager = DIContainer.Instance.Resolve<AssetManager>();
            p_openPopUP = await assetManager.LoadAssetAsync<EventSO>("OnOpenPopUp", gameObject);
        }
    }

    public virtual void OpenPopUp()
    {
        p_openPopUP.Publish(gameObject);
    }

    public virtual void ClosePopUp() 
    {
    }
}
