using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class BillboardUI : MonoBehaviour
{
    [Tooltip("하얀색 바 줄어드는 속도")]
    public float damageTweenDuration = 0.3f;
    [Tooltip("데미지 입고 하얀색 바가 기다리는 시간")]
    public float damageDelay = 0.3f;
    private Tween healthTween, damageTween;
    Camera camera;
    Slider slider;
    public Slider healthSlider, damageSlider;
    void Start()
    {
        camera = Camera.main;
    }
    public void Initialize()
    {
        slider = GetComponentInChildren<Slider>();
        slider.value = 1f;
        DOTween.Init();

    }
    public void SetHealthBar(int MaxValue, int CurrentValue)
    {
        healthTween?.Kill();
        damageTween?.Kill();
        float ratio = 0;
        if (MaxValue > 0)
        {
            ratio = Mathf.Clamp01((float)CurrentValue / (float)MaxValue);
        }
        healthTween = DOTween.To(()=>healthSlider.value, x=>healthSlider.value = x, ratio, 0.1f).SetEase(Ease.OutCubic);
        damageTween = DOTween.To(()=>damageSlider.value, x=>damageSlider.value = x, ratio, damageDelay).SetEase(Ease.InQuad);
    }
    private void LateUpdate()
    {
        transform.rotation = camera.transform.rotation;
    }
}
