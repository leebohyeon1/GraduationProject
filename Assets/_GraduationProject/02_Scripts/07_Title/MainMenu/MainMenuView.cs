using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuView : TitleView
{
    [Header("Main Menu Settings")]
    [SerializeField] private GameObject _firstButton;
    [SerializeField] private MainMenuCursor _cursor;
    [SerializeField] private RectTransform _firstButtonRect;

    [SerializeField] private GameObject _continueButton;

    private void OnEnable()
    {
        SelectButton();

        if(DataManager.Instance.DataList.Count > 0 )
        {
            _continueButton.SetActive(true);
        }
        else
        {
            _continueButton.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // Update 문 삭제! (이제 감시할 필요 없음)
    private void SelectButton()
    {
        // EventSystem이 없으면 중단
        if (EventSystem.current == null)
        {
            return;
        }

        // 혹시 모를 잔여물 제거 (선택 초기화)
        EventSystem.current.SetSelectedGameObject(null);

        // 누구를 선택할지 결정
        GameObject targetButton = _firstButton;

        // 커서가 마지막 위치를 기억하고 있고, 그 버튼이 지금 켜져 있다면 그걸 타겟으로!
        if (_cursor != null && _cursor.LastSelectedButton != null && _cursor.LastSelectedButton.activeInHierarchy)
        {
            targetButton = _cursor.LastSelectedButton;
        }

        // [핵심] 강제 선택 명령
        EventSystem.current.SetSelectedGameObject(targetButton);

        // 커서 위치 동기화
        if (_cursor != null && targetButton != null)
        {
            var rect = targetButton.GetComponent<RectTransform>();
            _cursor.ForcePosition(rect, targetButton);
        }
    }
}