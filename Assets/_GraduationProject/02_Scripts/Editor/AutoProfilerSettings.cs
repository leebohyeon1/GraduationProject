using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class AutoProfilerSettings
{
    private const string PrefsPrefix = "AutoProfilerAI_";
    private const string ApiKeyPath = PrefsPrefix + "ApiKey";
    private const string CpuThresholdPath = PrefsPrefix + "CpuThreshold";
    private const string GcThresholdPath = PrefsPrefix + "GcThreshold";

    // 기본값 설정
    public static string ApiKey
    {
        get => EditorPrefs.GetString(ApiKeyPath, "");
        set => EditorPrefs.SetString(ApiKeyPath, value);
    }

    public static float CpuThresholdMs
    {
        get => EditorPrefs.GetFloat(CpuThresholdPath, 20f);
        set => EditorPrefs.SetFloat(CpuThresholdPath, value);
    }

    public static float GcThresholdKb
    {
        get => EditorPrefs.GetFloat(GcThresholdPath, 50f);
        set => EditorPrefs.SetFloat(GcThresholdPath, value);
    }

    [SettingsProvider]
    public static SettingsProvider CreateAutoProfilerSettingsProvider()
    {
        var provider = new SettingsProvider("Preferences/Auto-Profiler AI", SettingsScope.User)
        {
            label = "Auto-Profiler AI",
            guiHandler = (searchContext) =>
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("LLM API Settings", EditorStyles.boldLabel);

                // API Key 마스킹 처리
                EditorGUI.BeginChangeCheck();
                string maskedKey = new string('*', ApiKey.Length > 0 ? 10 : 0);
                string newKey = EditorGUILayout.PasswordField("API Key", ApiKey);
                if (EditorGUI.EndChangeCheck())
                {
                    ApiKey = newKey;
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Performance Thresholds", EditorStyles.boldLabel);

                // CPU Spike 임계값 (ms)
                CpuThresholdMs = EditorGUILayout.FloatField("CPU Spike Threshold (ms)", CpuThresholdMs);
                
                // GC 할당 임계값 (KB)
                GcThresholdKb = EditorGUILayout.FloatField("GC Alloc Threshold (KB)", GcThresholdKb);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("임계값을 초과하는 프레임이 감지되면 프로파일러 데이터가 자동으로 기록됩니다.", MessageType.Info);
            },

            // 검색 키워드 등록
            keywords = new HashSet<string>(new[] { "Auto", "Profiler", "AI", "LLM", "Performance" })
        };

        return provider;
    }
}
