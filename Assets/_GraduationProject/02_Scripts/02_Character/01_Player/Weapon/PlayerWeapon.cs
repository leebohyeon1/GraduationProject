using HighlightPlus;
using UnityEngine;

[System.Serializable]
public struct InnerGlowSettings
{
    [Range(0f, 5f)]
    public float Intensity;
    [Range(0, 2f)]
    public float Width;
    public Color Color;
    public InnerGlowBlendMode InnerGlowBlendMode;
    public Visibility InnerGlowVisibility;
}

public class PlayerWeapon : MonoBehaviour
{
    [Header("Weapon Highlight Effect")]
    [SerializeField] private HighlightEffect _highLigthEffect; // 무기 하이라이트 에셋
    [SerializeField] private InnerGlowSettings[] _innerGlowSettings; // 무기 이너글로우 세팅

    private void Start()
    {
        if(_highLigthEffect == null)
        {
            _highLigthEffect = GetComponent<HighlightEffect>();
        }
    }

    public void SetWeaponInnerGlowEffect(int chargeTier)
    {
        if(_highLigthEffect == null || chargeTier >= _innerGlowSettings.Length)
        {
            return;
        }

        _highLigthEffect.innerGlow = _innerGlowSettings[chargeTier].Intensity;
        _highLigthEffect.innerGlowWidth = _innerGlowSettings[chargeTier].Width;
        _highLigthEffect.innerGlowColor = _innerGlowSettings[chargeTier].Color;
        _highLigthEffect.innerGlowBlendMode = _innerGlowSettings[chargeTier].InnerGlowBlendMode;
        _highLigthEffect.innerGlowVisibility = _innerGlowSettings[chargeTier].InnerGlowVisibility;
    }

}
