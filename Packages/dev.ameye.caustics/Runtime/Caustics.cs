using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using UnityEngine.Rendering.Universal;

namespace Caustics
{
    [ExcludeFromPreset]
    [DisallowMultipleRendererFeature("Caustics")]
#if UNITY_6000_0_OR_NEWER
    [SupportedOnRenderer(typeof(UniversalRendererData))]
#endif
    [Tooltip("The caustics renderer feature is used to pass the direction of the directional light to the caustics shader.")]
    [HelpURL("https://caustics.ameye.dev")]
    public class Caustics : ScriptableRendererFeature
    {
        private class CausticsPass : ScriptableRenderPass
        {
            private static readonly int MainLightDirection = Shader.PropertyToID("_MainLightDirection");

            public void Setup()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }

#if UNITY_6000_0_OR_NEWER
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var sunMatrix = RenderSettings.sun != null
                    ? RenderSettings.sun.transform.localToWorldMatrix
                    : Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90.0f, 0.0f, 0.0f), Vector3.one);

                Shader.SetGlobalMatrix(MainLightDirection, sunMatrix);
            }
#else
 public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cam = renderingData.cameraData.camera;
                if (cam.cameraType == CameraType.Preview) return;

                var sunMatrix = RenderSettings.sun != null
                    ? RenderSettings.sun.transform.localToWorldMatrix
                    : Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90.0f, 0.0f, 0.0f), Vector3.one);

                Shader.SetGlobalMatrix(MainLightDirection, sunMatrix);
            }
#endif

            public void Dispose()
            {

            }
        }

        private CausticsPass causticsPass;

        public override void Create()
        {
            causticsPass ??= new CausticsPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Don't render for some views.
            if (renderingData.cameraData.cameraType == CameraType.Preview
                || renderingData.cameraData.cameraType == CameraType.Reflection
#if UNITY_6000_0_OR_NEWER
                || UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
#else
                || UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData))
#endif
                return;

            causticsPass.Setup();
            renderer.EnqueuePass(causticsPass);
        }

        override protected void Dispose(bool disposing)
        {
            causticsPass?.Dispose();
            causticsPass = null;
        }
    }
}