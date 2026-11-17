using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerInteractUI : PlayerUIBase
{
    private RectTransform _rectTransform;
    private IInteractable interactable;

    [SerializeField] private TMP_Text _interactText;
    [SerializeField] private string _npcInteractString;
    [SerializeField] private string _environmentInteractString;
    private Coroutine _pendingLabel;
    private bool _reportedFontDelay;

    public override void Initialize(PlayerController player)
    {
        bool samePlayer = p_player == player;
        ReleasePlayerSubscription();
        CancelPendingLabel();
        if (!samePlayer) base.Initialize(player);

        _rectTransform = GetComponent<RectTransform>();
        // 상호작용 UI 초기화 로직 추가 (예: 이벤트 구독)
        p_player.Interact.InteractableChanged += OnInteractableChanged;
        if (interactable != null) OnInteractableChanged(interactable);
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
        CancelPendingLabel();
        ReleasePlayerSubscription();
        interactable = null;
        p_player = null;
        base.Dispose();
    }

    private void ReleasePlayerSubscription()
    {
        if (p_player && p_player.Interact != null)
            p_player.Interact.InteractableChanged -= OnInteractableChanged;
    }

    private void OnInteractableChanged(IInteractable interactable)
    {
        CancelPendingLabel();
        if (interactable != null)
        {
            // Keep the authored Latin font from rendering a Korean source key
            // before LocalizedGameText's asynchronous font request completes.
            _interactText.enabled = false;
            _interactText.text = GetPrompt(interactable);
            var localized = _interactText.GetComponent<LocalizedGameText>();
            if (!localized) localized = _interactText.gameObject.AddComponent<LocalizedGameText>();
            gameObject.SetActive(true);
            this.interactable = interactable;
            if (localized.IsReady) _interactText.enabled = true;
            else _pendingLabel = StartCoroutine(ShowLabelWhenReady(localized));
        }
        else
        {
            this.interactable = null;
            gameObject.SetActive(false);
        }
    }

    private string GetPrompt(IInteractable target)
    {
        if (target is ResetTotem) return "초기화";
        if (target is SavePoint) return "저장";
        var component = target as Component;
        if (component && (component.GetComponent<TeleportInteractor>() || component.GetComponent<SceneTransitionInteractor>() || component.GetComponent<EnemyInteract>() || component.GetComponent<Chest>()))
            return "상호작용";
        if (target.InteractableType == InteractableType.NPC) return _npcInteractString;
        if (target.InteractableType == InteractableType.Environment) return _environmentInteractString;
        return _interactText.text;
    }

    private IEnumerator ShowLabelWhenReady(LocalizedGameText localized)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + 2;
        while (localized && !localized.IsReady && Time.realtimeSinceStartupAsDouble < deadline)
            yield return null;

        if (!_interactText || !isActiveAndEnabled) yield break;
        if (!localized || !localized.IsReady)
        {
            // A failed localization request must not leave the prompt hidden forever.
            // Reuse an already loaded font that contains the current source text;
            // LocalizedGameText will still apply the selected locale when it arrives.
            if (!_interactText.font || !_interactText.font.HasCharacters(_interactText.text))
            {
                foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
                {
                    if (!font || !font.HasCharacters(_interactText.text)) continue;
                    _interactText.font = font;
                    _interactText.fontSharedMaterial = font.material;
                    break;
                }
            }
            if (!_reportedFontDelay)
            {
                _reportedFontDelay = true;
                Debug.LogWarning("[PlayerInteractUI] Localized font is still pending after two seconds; showing the source prompt while localization continues.", this);
            }
        }
        _interactText.enabled = true;
        _pendingLabel = null;
    }

    private void CancelPendingLabel()
    {
        if (_pendingLabel != null) StopCoroutine(_pendingLabel);
        _pendingLabel = null;
    }

    private void OnDisable()
    {
        CancelPendingLabel();
        // The object is inactive, so restoring this flag cannot render a stale label.
        if (_interactText) _interactText.enabled = true;
    }

    private void OnDestroy() => Dispose();
}
