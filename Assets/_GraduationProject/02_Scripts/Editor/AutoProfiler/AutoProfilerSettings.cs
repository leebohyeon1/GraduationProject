using UnityEditor;

/// <summary>
/// Static class managing performance spike detection thresholds and AI settings.
/// </summary>
public static class AutoProfilerSettings
{
    // 1. Natural language target hardware context for AI analysis
    public static string TargetHardwareContext
    {
        get => EditorPrefs.GetString("AutoProfiler_TargetContext", "Example: This is a 3D action game targeting 60 FPS on mobile. Minimizing battery drain and heat is critical.");
        set => EditorPrefs.SetString("AutoProfiler_TargetContext", value);
    }

    // 2. CPU performance threshold (default 20ms)
    public static float CpuThresholdMs
    {
        get => EditorPrefs.GetFloat("AutoProfiler_CpuThreshold", 20.0f);
        set => EditorPrefs.SetFloat("AutoProfiler_CpuThreshold", value);
    }

    // 3. GC allocation threshold (default 100KB)
    public static float GcThresholdKb
    {
        get => EditorPrefs.GetFloat("AutoProfiler_GcThreshold", 100.0f);
        set => EditorPrefs.SetFloat("AutoProfiler_GcThreshold", value);
    }

    // 4. Auto-analysis toggle (default false)
    public static bool AutoAnalyzeOnDetection
    {
        get => EditorPrefs.GetBool("AutoProfiler_AutoAnalyze", false);
        set => EditorPrefs.SetBool("AutoProfiler_AutoAnalyze", value);
    }
    
    // 5. Maximum spikes to collect (default 15)
    public static int MaxSpikeCount
    {
        get => EditorPrefs.GetInt("AutoProfiler_MaxSpikes", 15);
        set => EditorPrefs.SetInt("AutoProfiler_MaxSpikes", value);
    }
}