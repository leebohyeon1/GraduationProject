using UnityEngine;
using TMPro;

// Counts stalled puzzle play, rather than time spent elsewhere in the dungeon.
public class PuzzleMascotHint : MonoBehaviour
{
    public PuzzleGridManager puzzle;
    public GameObject mascot;
    public TMP_Text hintText;
    public float delaySeconds = 120f;
    private PlayerController _player;
    private float _stalledTime;
    private int _bestCompleted;
    private bool _revealed;
    private bool _visibleTarget;
    private float _reveal;
    private Transform _revealRoot;
    public float revealDuration = .6f;

    private void Awake() {
        if (mascot) {
            // Reveal a separate parent so the mascot's Animator and Feel squash keep their authored scale.
            var visual = mascot.transform;
            _revealRoot = new GameObject("HintRevealRoot").transform;
            _revealRoot.SetParent(visual.parent, false);
            _revealRoot.localPosition = visual.localPosition;
            _revealRoot.localRotation = Quaternion.identity;
            visual.SetParent(_revealRoot, false);
            visual.localPosition = Vector3.zero;
            mascot.SetActive(false);
            _revealRoot.localScale = Vector3.zero;
        }
        if (hintText) {
            hintText.transform.localPosition = Vector3.up * 4.5f;
            hintText.outlineWidth = .25f;
            hintText.outlineColor = Color.black;
        }
        SetVisible(false);
    }
    private void Update()
    {
        if (!puzzle || !mascot) return;
        if (!_player) _player = FindFirstObjectByType<PlayerController>();
        if (!_player || puzzle.IsSolved) { SetVisible(false); return; }
        bool inside = puzzle.IsPlayerInPuzzle(_player.transform.position);
        if (!inside) { SetVisible(false); return; }
        int completed = puzzle.CompletedTargetCount;
        if (completed > _bestCompleted) {
            _bestCompleted = completed;
            _stalledTime = 0f;
            _revealed = false;
        }
        if (Time.timeScale <= 0f || InputReaderSO.ActiveInputMode != InputReaderSO.InputMode.Gameplay) {
            SetVisible(false); return;
        }
        _stalledTime += Time.deltaTime;
        if (_stalledTime >= delaySeconds) _revealed = true;
        SetVisible(_revealed);
        if (hintText && hintText.gameObject.activeInHierarchy && Camera.main)
            hintText.transform.rotation = Camera.main.transform.rotation;
    }
    private void SetVisible(bool visible)
    {
        _visibleTarget=visible;
    }
    private void LateUpdate() {
        _reveal=Mathf.MoveTowards(_reveal,_visibleTarget?1f:0f,Time.unscaledDeltaTime/Mathf.Max(.1f,revealDuration));
        float eased=Mathf.SmoothStep(0,1,_reveal);
        if(mascot) mascot.SetActive(_reveal>0);
        if(_revealRoot) _revealRoot.localScale=Vector3.one*eased;
        if(hintText) { hintText.gameObject.SetActive(_reveal>0);hintText.alpha=eased;if(Camera.main)hintText.transform.rotation=Camera.main.transform.rotation; }
    }
    private void OnDisable() { _visibleTarget=false;_reveal=0;if(mascot)mascot.SetActive(false);if(_revealRoot)_revealRoot.localScale=Vector3.zero;if(hintText)hintText.gameObject.SetActive(false); }
}