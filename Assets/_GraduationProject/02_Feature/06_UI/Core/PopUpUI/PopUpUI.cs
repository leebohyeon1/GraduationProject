using BH_Lib.AssetManager;
using BH_Lib.DI;
using System;
using UnityEngine;

public enum PopUpType
{
    SkillEnchant
}

public class PopUpUI : MonoBehaviour
{
    [SerializeField] protected PopUpType p_type;
    [SerializeField] protected EventSO<PopUpUI> p_openPopUP;

    public PopUpType Type => p_type;

    protected virtual async void  Start()
    {
        if(p_openPopUP == null)
        {
            AssetManager assetManager = DIContainer.Instance.Resolve<AssetManager>();
            p_openPopUP = await assetManager.LoadAssetAsync<EventSO<PopUpUI>>("OnOpenPopUp", gameObject);
        }
    }

    public virtual void OpenPopUp()
    {
        p_openPopUP.Publish(this);
    }

    public virtual void ClosePopUp() 
    {
    }
}
