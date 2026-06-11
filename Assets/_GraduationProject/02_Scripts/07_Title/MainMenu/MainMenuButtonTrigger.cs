using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonTrigger : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] private MainMenuCursor _cursorController;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 닿으면 "내가 타겟이야" 라고 알림
        // eventData.selectedObject = gameObject; // (필요하면 주석 해제)
        UpdateCursor();
    }

    public void OnSelect(BaseEventData eventData)
    {
        // 키보드/패드로 선택되면 "내가 타겟이야" 라고 알림
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        if (_cursorController != null)
        {
            // [수정] RectTransform과 함께 gameObject(나 자신)도 보냄
            _cursorController.SetTarget(_rectTransform, gameObject);
        }
    }
}