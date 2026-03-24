using UnityEditor;

/// <summary>
/// 성능 스파이크 감지 임계값 및 AI 설정을 관리하는 정적 클래스입니다.
/// </summary>
public static class AutoProfilerSettings
{
    // 1. CPU 성능 임계값 (기본 20ms)
    public static float CpuThresholdMs
    {
        get => EditorPrefs.GetFloat("AutoProfiler_CpuThreshold", 20.0f);
        set => EditorPrefs.SetFloat("AutoProfiler_CpuThreshold", value);
    }

    // 2. GC 할당량 임계값 (기본 100KB)
    public static float GcThresholdKb
    {
        get => EditorPrefs.GetFloat("AutoProfiler_GcThreshold", 100.0f);
        set => EditorPrefs.SetFloat("AutoProfiler_GcThreshold", value);
    }

    // 3. 자동 분석 여부 (기본 false)
    public static bool AutoAnalyzeOnDetection
    {
        get => EditorPrefs.GetBool("AutoProfiler_AutoAnalyze", false);
        set => EditorPrefs.SetBool("AutoProfiler_AutoAnalyze", value);
    }
    
    // 4. 최대 수집 데이터 수 (기본 15개)
    public static int MaxSpikeCount
    {
        get => EditorPrefs.GetInt("AutoProfiler_MaxSpikes", 15);
        set => EditorPrefs.SetInt("AutoProfiler_MaxSpikes", value);
    }
}