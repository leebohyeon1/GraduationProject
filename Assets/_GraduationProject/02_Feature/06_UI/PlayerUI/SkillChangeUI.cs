using BH_Lib.DI;
using UnityEngine;

public class SkillChangeUI : MonoBehaviour, IEventListener<bool>
{
    private SkillType _selectSkill = SkillType.None;

    [SerializeField] private EventSO<bool> _onOpenSkillChangeUI;
    [SerializeField] private EventSO<SkillType> _onSelectSkill;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private InputReader _inputReader;

    [SerializeField] private GameObject _skillChangeImage;
    private RectTransform _skillChangeImageRectTransform;

    private bool _isGamepadActive;

    private void OnEnable()
    {
        if (_playerTransform == null)
        {
            _playerTransform = FindFirstObjectByType<Player>().transform;
        }

        _onOpenSkillChangeUI.Subscribe(this);
        _skillChangeImageRectTransform = _skillChangeImage.GetComponent<RectTransform>();

        _inputReader.LookEvent += HandleLook;
        _inputReader.MousePositionEvent += HandleMousePosition;
        _inputReader.InputDeviceChangedEvent += HandleDeviceChange;
    }

    private void OnDisable()
    {
        _onOpenSkillChangeUI.Unsubscribe(this);

        _inputReader.LookEvent -= HandleLook;
        _inputReader.MousePositionEvent -= HandleMousePosition;
        _inputReader.InputDeviceChangedEvent -= HandleDeviceChange;
    }

    private void Start()
    {
        _skillChangeImage.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (_skillChangeImage.activeSelf)
        {
            _skillChangeImageRectTransform.position = Camera.main.WorldToScreenPoint(_playerTransform.position + Vector3.up * 2);
        }
    }

    public void OnEventTrigger(bool value)
    {
        if (value)
        {
            _skillChangeImageRectTransform.position = Camera.main.WorldToScreenPoint(_playerTransform.position + Vector3.up * 2);
            _skillChangeImage.SetActive(true);
        }
        else
        {
            if(_selectSkill != SkillType.None)
            {
                _onSelectSkill.Publish(_selectSkill);
            }
            _skillChangeImage.SetActive(false);
            _selectSkill = SkillType.None;
        }
    }

    private void HandleDeviceChange(InputDeviceType deviceType)
    {
        _isGamepadActive = deviceType == InputDeviceType.Gamepad;
    }

    private void HandleLook(Vector2 direction)
    {
        if (!_isGamepadActive || !_skillChangeImage.activeSelf) return;
        UpdateSelectedSkill(direction);
    }

    private void HandleMousePosition(Vector2 mousePosition)
    {
        if (_isGamepadActive || !_skillChangeImage.activeSelf) return;

        Vector2 playerScreenPosition = Camera.main.WorldToScreenPoint(_playerTransform.position + Vector3.up);
        Vector2 direction = mousePosition - playerScreenPosition;
        UpdateSelectedSkill(direction.normalized);
    }

    private void UpdateSelectedSkill(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.2f)
        {
            _selectSkill = SkillType.None;
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle > 30f && angle <= 150f)
        {
            _selectSkill = SkillType.Flash;
        }
        else if (angle > 150f || angle <= -90f)
        {
            _selectSkill = SkillType.Boost;
        }
        else
        {
            _selectSkill = SkillType.TimeStop;
        }
    }
}