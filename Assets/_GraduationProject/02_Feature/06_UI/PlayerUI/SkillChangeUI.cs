using UnityEngine;

public class SkillChangeUI : MonoBehaviour, IEventListener<bool>
{
    private SkillType _selectSkill;

    [SerializeField] private EventSO<bool> _onOpenSkillChangeUI;
    [SerializeField] private EventSO<SkillType> _onSelectSkill;
    [SerializeField] private Transform _playerTransform;

    [SerializeField] private GameObject _skillChangeImage;
    private RectTransform _skillChangeImageRectTransform;

    private void Start()
    {
        _onOpenSkillChangeUI.Subscribe(this);
        _skillChangeImage.SetActive(false);
        _skillChangeImageRectTransform = _skillChangeImage.GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        if(_skillChangeImage.activeSelf)
        {
            _skillChangeImageRectTransform.position = Camera.main.WorldToScreenPoint(_playerTransform.position + Vector3.up * 2);
        }
    }

    private void OnDestroy()
    {
        _onOpenSkillChangeUI.Unsubscribe(this);
    }


    public void OnEventTrigger(bool value)
    {
        if(value)
        {
            _skillChangeImageRectTransform.position = Camera.main.WorldToScreenPoint(_playerTransform.position + Vector3.up * 2);  
            _skillChangeImage.SetActive(true);
        }
        else
        {
            _skillChangeImage.SetActive(false);
        }   
    }
}
