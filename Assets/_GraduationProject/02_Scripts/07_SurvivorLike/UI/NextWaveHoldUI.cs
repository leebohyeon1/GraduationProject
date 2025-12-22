using UnityEngine;
using UnityEngine.UI;

public class NextWaveHoldUI : MonoBehaviour, IEventListener<float>
{
    [SerializeField] private UpdateNextWaveHoldTimeEventSO _updateNextWaveHoldTimeEvent;

    [SerializeField] private GameObject _panel;
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
        if (value == -1f)
        {
            _panel.SetActive(false);
        }
        else if (!_panel.activeSelf)
        {
            _panel.SetActive(true);
        }

        _image.fillAmount = value;
    }
}
