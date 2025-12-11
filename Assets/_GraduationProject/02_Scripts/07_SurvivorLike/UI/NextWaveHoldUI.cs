using UnityEngine;
using UnityEngine.UI;

public class NextWaveHoldUI : MonoBehaviour, IEventListener<float>
{
    [SerializeField] private UpdateNextWaveHoldTimeEventSO _updateNextWaveHoldTimeEvent;

    [SerializeField] private Image _image;


    private void Start()
    {
        _updateNextWaveHoldTimeEvent.Subscribe(this);
    }

    private void OnDestroy()
    {
        _updateNextWaveHoldTimeEvent.Unsubscribe(this);
    }

    public void OnEventTrigger(float value)
    {
        _image.fillAmount = value;
    }
}
