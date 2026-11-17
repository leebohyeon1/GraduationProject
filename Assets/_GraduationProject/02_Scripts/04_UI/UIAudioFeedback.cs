using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using MoreMountains.Tools;

public class UIAudioFeedback : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private static float _lastSound;
    public static void Play(int kind)
    {
        if ((kind < 3 && Time.unscaledTime - _lastSound < 0.075f) || !MMSoundManager.HasInstance) return;
        var config = Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings");
        if (!config) return;
        var clip = kind == 6 ? config.skillLearned : kind == 7 ? config.insufficientMoney : kind == 3 ? config.uiWarning : kind == 4 ? config.puzzleClear : kind == 5 ? config.stoneHit : kind == 1 ? config.uiConfirm : kind == 2 ? config.uiCancel : config.uiNavigate;
        if (!clip) return;
        _lastSound = Time.unscaledTime;
        MMSoundManager.Instance.PlaySound(clip, kind >= 4 ? MMSoundManager.MMSoundManagerTracks.Sfx : MMSoundManager.MMSoundManagerTracks.UI, Vector3.zero, volume:0.4f, pitch:kind==2?0.85f:1f);
    }
    public void OnPointerEnter(PointerEventData data) { if(InputReaderSO.ActiveInputMode == InputReaderSO.InputMode.UI && GetComponent<UnityEngine.UI.Selectable>().IsInteractable()) Play(0); }
    public void OnPointerClick(PointerEventData data) { if(InputReaderSO.ActiveInputMode == InputReaderSO.InputMode.UI && GetComponent<UnityEngine.UI.Selectable>().IsInteractable()) Play(1); }
    public static void OnActionChange(object obj, InputActionChange change)
    {
        if (InputReaderSO.ActiveInputMode != InputReaderSO.InputMode.UI) return;
        if (change != InputActionChange.ActionPerformed || !(obj is InputAction action)) return;
        if (action.actionMap?.name != "UI" && !(action.actionMap?.name == "Share" && action.name == "Escape")) return;
        if (action.name == "Point" || action.name == "Click" || action.name == "ScrollWheel" || action.name == "RightClick" || action.name == "MiddleClick") return;
        if (action.activeControl == null || action.activeControl.EvaluateMagnitude() < 0.2f) return;
        Play(action.name == "Cancel" || action.name == "Escape" ? 2 : action.name == "Submit" ? 1 : 0);
    }
}
