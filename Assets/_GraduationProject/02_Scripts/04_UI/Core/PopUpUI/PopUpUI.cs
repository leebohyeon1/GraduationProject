using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
            var handle = Addressables.LoadAssetAsync<EventSO<PopUpUI>>("OnOpenPopUp");
            p_openPopUP = await handle.Task;
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
