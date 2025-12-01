using BH_Lib.AssetManager;
using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 투명화 컨트롤러가 탐지해야할 대상
/// </summary>
public class SeeThroughTarget : MonoBehaviour
{
    [SerializeField] private OnRegisterSeeThroughTargetSO _onRegisterCutOutTargetSO;

    private async void OnEnable()
    {
        if( _onRegisterCutOutTargetSO == null )
        {
            _onRegisterCutOutTargetSO = await DIContainer.Instance.Resolve<AssetManager>()
                .LoadAssetAsync<OnRegisterSeeThroughTargetSO>("OnRegisterCutOutTarget", this.gameObject);
        }

        _onRegisterCutOutTargetSO.Publish(new SeeThroughTargetTransform(transform), 0.1f);
    }

    private void OnDisable()
    {
        _onRegisterCutOutTargetSO.Publish(new SeeThroughTargetTransform(transform, false));
    }
}
