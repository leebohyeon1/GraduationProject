using BH_Lib.AssetManager;
using BH_Lib.DI;
using UnityEngine;

public class CutOutTarget : MonoBehaviour
{
    [SerializeField] private OnRegisterCutOutTargetSO _onRegisterCutOutTargetSO;

    private async void OnEnable()
    {
        if( _onRegisterCutOutTargetSO == null )
        {
            _onRegisterCutOutTargetSO = await DIContainer.Instance.Resolve<AssetManager>()
                .LoadAssetAsync<OnRegisterCutOutTargetSO>("OnRegisterCutOutTarget", this.gameObject);
        }

        _onRegisterCutOutTargetSO.Publish(new CutOutTargetTransform(transform), 0.1f);
    }

    void Update()
    {
        
    }

    private void OnDisable()
    {
        _onRegisterCutOutTargetSO.Publish(new CutOutTargetTransform(transform, false));
    }
}
