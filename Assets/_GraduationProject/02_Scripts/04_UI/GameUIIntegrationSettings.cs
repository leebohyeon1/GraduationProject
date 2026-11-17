using UnityEngine;
using UnityEngine.InputSystem;

public class GameUIIntegrationSettings : ScriptableObject
{
    public InputActionAsset actions;
    public TMPro.TMP_FontAsset guideFont;
    public Sprite[] playStationSprites;
    public Sprite[] xboxSprites;
    public AudioClip uiNavigate, uiConfirm, uiCancel, stoneRumble, uiWarning, puzzleClear, stoneHit;
    public Sprite[] keyboardSprites;
    public AudioClip gameOver, reflectedExplosion, gateCharge;
    public AudioClip skillLearned, insufficientMoney, mascotHit;
    public GameObject reflectedExplosionEffect;
}
