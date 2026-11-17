using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1000)]
public class TitleBossShortcut : MonoBehaviour
{
    public SceneDataSO bossScene;
    public Vector3 spawnPosition = new Vector3(952f,17.6f,-80f);
    public float holdSeconds = 5f;
    private float _heldTime;
    private int _device = -1;
    private bool _starting;
    private TitleManager _title;
    private void Awake() { _title = GetComponent<TitleManager>(); }
    public static bool IsChordHeld() => HeldDevice() != -1;
    private static int HeldDevice()
    {
        foreach (var pad in Gamepad.all)
            if (pad.leftShoulder.isPressed && pad.rightShoulder.isPressed) return pad.deviceId;
        var mouse = Mouse.current;
        return mouse != null && mouse.leftButton.isPressed && mouse.rightButton.isPressed ? mouse.deviceId : -1;
    }
    private void Update()
    {
        if (_starting) return;
        int device = HeldDevice();
        if (!_title || !_title.CanUseBossShortcut || device == -1 ||
            !SceneLoadingManager.Instance || SceneLoadingManager.Instance.IsTeleporting) {
            _heldTime = 0; _device = -1; return;
        }
        if (_device != device) { _device = device; _heldTime = 0; }
        _heldTime += Time.unscaledDeltaTime;
        if (_heldTime < holdSeconds) return;
        if (!bossScene || !DataManager.Instance) { _heldTime=0; return; }
        _starting = true;
        Time.timeScale = 1f;
        DataManager.Instance.BeginTemporaryGame(bossScene, spawnPosition);
        SceneLoadingManager.Instance.TeleportToScene(bossScene, spawnPosition);
    }
    private void OnApplicationFocus(bool focused) { if (!focused) { _heldTime=0; _device=-1; } }
    private void OnDisable() { _heldTime=0; _device=-1; }
}