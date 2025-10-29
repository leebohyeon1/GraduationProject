using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class BillboardUI : MonoBehaviour
{
    Camera camera;
    Slider slider;
    void Start()
    {
        camera = Camera.main;
        
    }
    public void Initialize()
    {
        slider = GetComponentInChildren<Slider>();
        slider.value = 1f;
    }
    public void SetHealthBar(int MaxValue, int CurrentValue)
    {
        if (slider == null)
        {
            Debug.Log("slider없음");
            return;
        }
        float ratio = (float)CurrentValue / MaxValue;
        Debug.Log($"SliderValue: {slider.value}, ratio: {ratio}, MaxValue: {CurrentValue}");
        slider.value = Mathf.Clamp01(ratio);
    }
    private void LateUpdate()
    {
        transform.rotation = camera.transform.rotation;
    }
}
