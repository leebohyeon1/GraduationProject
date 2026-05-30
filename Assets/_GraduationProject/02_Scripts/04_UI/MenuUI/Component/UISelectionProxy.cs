using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 요소의 선택(Select) 및 선택 해제(Deselect) 이벤트를 가로채서 다른 동작을 수행하게 해주는 프록시 컴포넌트입니다.
/// 패드 조작 시 UI 이벤트 충돌을 방지하고 부모에게 명확하게 이벤트를 전달하기 위해 사용합니다.
/// </summary>
public class UISelectionProxy : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Action OnSelectAction;
    public Action OnDeselectAction;

    public void OnSelect(BaseEventData eventData)
    {
        OnSelectAction?.Invoke();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        OnDeselectAction?.Invoke();
    }
}
