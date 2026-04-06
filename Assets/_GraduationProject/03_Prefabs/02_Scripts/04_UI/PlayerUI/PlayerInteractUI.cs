using TMPro;
using UnityEngine;

public class PlayerInteractUI : PlayerUIBase
{
    private RectTransform _rectTransform;
    private IInteractable interactable;

    [SerializeField] private TMP_Text _interactText;
    [SerializeField] private string _npcInteractString;
    [SerializeField] private string _environmentInteractString;

    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        _rectTransform = GetComponent<RectTransform>();
        // 상호작용 UI 초기화 로직 추가 (예: 이벤트 구독)
        p_player.Interact.InteractableChanged += OnInteractableChanged;
    }

    private void Update()
    {
        if (interactable != null)
        {
            // 상호작용 UI를 상호작용 가능한 오브젝트의 위치에 맞게 업데이트
            Vector3 screenPos = Camera.main.WorldToScreenPoint(interactable.InteractableUITransform.position);
            _rectTransform.position = screenPos;
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        p_player.Interact.InteractableChanged -= OnInteractableChanged;
    }

    private void OnInteractableChanged(IInteractable interactable)
    {
        if (interactable != null)
        {
            // 상호작용 가능한 오브젝트가 있을 때 UI 활성화
            gameObject.SetActive(true);

            this.interactable = interactable;

            switch(interactable.InteractableType)
            {
                case InteractableType.NPC:
                    _interactText.text = _npcInteractString;
                    break;
                case InteractableType.Environment:
                    _interactText.text = _environmentInteractString;
                    break;
            }
        }
        else
        {
            // 상호작용 가능한 오브젝트가 없을 때 UI 비활성화
            gameObject.SetActive(false);
        }
    }
}
