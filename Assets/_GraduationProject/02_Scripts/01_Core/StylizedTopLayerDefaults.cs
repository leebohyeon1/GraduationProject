using System;
using UnityEngine;

/// <summary>
/// ANGRYMESH's rock/foliage shaders multiply material settings by globals that
/// otherwise exist only after opening its demo scene in the Editor.
/// Initialize the package's authored neutral defaults in every fresh player.
/// An explicit ASPGlobalTopLayer scene component can still override them.
/// </summary>
public static class StylizedTopLayerDefaults
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        var args = Environment.GetCommandLineArgs();
        bool audit = Array.IndexOf(args, "--soom-loading-smoke") >= 0 &&
                     Array.IndexOf(args, "--soom-top-layer-audit") >= 0;
        bool reproduceMissingDefaults = audit && Array.IndexOf(args, "--soom-top-layer-audit-disabled") >= 0;
        if (audit)
            Debug.Log($"[TopLayerAudit] before: props={Shader.GetGlobalFloat("ASPP_TopLayerIntensity")}, heightFade={Shader.GetGlobalFloat("ASPT_TopLayerHeightFade")}");
        if (!reproduceMissingDefaults)
        {
            Shader.SetGlobalFloat("ASPT_TopLayerHeightStart", -5000f);
            Shader.SetGlobalFloat("ASPT_TopLayerHeightFade", 1f);
            Shader.SetGlobalFloat("ASPT_TopLayerIntensity", 1f);
            Shader.SetGlobalFloat("ASPT_TopLayerOffset", 1f);
            Shader.SetGlobalFloat("ASPT_TopLayerContrast", 1f);
            Shader.SetGlobalFloat("ASPT_TopLayerArrowDirection", 1f);
            Shader.SetGlobalFloat("ASPT_TopLayerBottomOffset", 1f);
            Shader.SetGlobalFloat("ASPP_TopLayerIntensity", 1f);
            Shader.SetGlobalFloat("ASPP_TopLayerOffset", 1f);
            Shader.SetGlobalFloat("ASPP_TopLayerContrast", 1f);
        }
        if (audit)
            Debug.Log($"[TopLayerAudit] after: props={Shader.GetGlobalFloat("ASPP_TopLayerIntensity")}, heightFade={Shader.GetGlobalFloat("ASPT_TopLayerHeightFade")}; defaultsApplied={!reproduceMissingDefaults}");
    }
}
