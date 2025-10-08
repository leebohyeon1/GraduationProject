// RadialBlurFeature.cs
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RadialBlurFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class RadialBlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material blurMaterial = null;
    }

    public RadialBlurSettings settings = new RadialBlurSettings();
    private RadialBlurPass radialBlurPass;

    // Create 메서드는 그대로 유지됩니다.
    public override void Create()
    {
        radialBlurPass = new RadialBlurPass(settings.renderPassEvent, settings.blurMaterial);
    }

    // AddRenderPasses 메서드가 더 간단해집니다.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blurMaterial == null)
        {
            Debug.LogWarningFormat("Radial Blur 머티리얼이 할당되지 않았습니다.");
            return;
        }

        // Pass를 렌더러에 추가하기만 하면 됩니다.
        // Setup 호출은 삭제합니다.
        renderer.EnqueuePass(radialBlurPass);
    }
}