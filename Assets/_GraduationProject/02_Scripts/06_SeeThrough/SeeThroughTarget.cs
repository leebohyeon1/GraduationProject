using UnityEngine;
using UnityEngine.AddressableAssets;

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
            var handle = Addressables.LoadAssetAsync<OnRegisterSeeThroughTargetSO>("OnRegisterCutOutTarget");
            _onRegisterCutOutTargetSO = await handle.Task;
        }

        _onRegisterCutOutTargetSO.Publish(new SeeThroughTargetTransform(transform), 0.1f);
    }

    private void OnDisable()
    {
        _onRegisterCutOutTargetSO.Publish(new SeeThroughTargetTransform(transform, false));
    }
}
