// RadialBlurPass.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RadialBlurPass : ScriptableRenderPass
{
    private Material material;
    
    // RenderTargetHandle 대신 RTHandle을 사용합니다.
    private RTHandle source;
    private RTHandle tempTexture;

    public RadialBlurPass(RenderPassEvent renderPassEvent, Material material)
    {
        this.renderPassEvent = renderPassEvent;
        this.material = material;

        // 임시 텍스처 핸들을 생성합니다. 이름으로 식별됩니다.
        tempTexture = RTHandles.Alloc("_TempBlurTexture", name: "_TempBlurTexture");
    }

    // Setup 대신 OnCameraSetup을 사용하여 RTHandle을 안전하게 받아옵니다.
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // 렌더링 소스를 RTHandle로 받아옵니다.
        source = renderingData.cameraData.renderer.cameraColorTargetHandle;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (material == null) return;
        if (!renderingData.cameraData.postProcessEnabled) return;

        var stack = VolumeManager.instance.stack;
        var radialBlur = stack.GetComponent<RadialBlur>();

        if (radialBlur == null || !radialBlur.IsActive()) return;

        CommandBuffer cmd = CommandBufferPool.Get("RadialBlur");
        
        // Volume Component에서 설정값을 가져와 머티리얼(셰이더)에 전달
        material.SetFloat("_Strength", radialBlur.strength.value);
        material.SetVector("_Center", radialBlur.center.value);
        material.SetInt("_Samples", radialBlur.sampleCount.value);

        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0; // 임시 텍스처에는 Depth 정보가 필요 없습니다.

        // 화면과 동일한 설정의 임시 렌더 텍스처를 할당합니다.
        RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, FilterMode.Bilinear);

        // Blit을 사용하여 소스 -> 임시 텍스처 (효과 적용) -> 소스로 다시 복사합니다.
        Blit(cmd, source, tempTexture, material, 0);
        Blit(cmd, tempTexture, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
    
    // 렌더링이 끝난 후 호출되어 할당했던 리소스를 해제합니다.
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // tempTexture.Release(); // RTHandles.Alloc으로 생성한 경우 명시적으로 해제할 필요가 없습니다.
    }
}