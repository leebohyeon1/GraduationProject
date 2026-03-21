using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AutoProfilerSettings
{
    private const string PrefsPrefix = "AutoProfilerAI_";
    private const string ApiKeyPath = PrefsPrefix + "ApiKey";
    private const string CpuThresholdPath = PrefsPrefix + "CpuThreshold";
    private const string GcThresholdPath = PrefsPrefix + "GcThreshold";

    private static string _verificationStatus = "";
    private static MessageType _statusMessageType = MessageType.None;
    private static bool _isVerifying = false;

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

                // API Key 필드
                EditorGUI.BeginChangeCheck();
                string newKey = EditorGUILayout.PasswordField("API Key", ApiKey);
                if (EditorGUI.EndChangeCheck())
                {
                    ApiKey = newKey;
                    _verificationStatus = ""; // 키가 바뀌면 상태 초기화
                }

                // 검증 버튼
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.enabled = !_isVerifying && !string.IsNullOrEmpty(ApiKey);
                if (GUILayout.Button(_isVerifying ? "Verifying..." : "Verify API Key", GUILayout.Width(120)))
                {
                    VerifyKeyAsync();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                // 검증 결과 표시
                if (!string.IsNullOrEmpty(_verificationStatus))
                {
                    EditorGUILayout.HelpBox(_verificationStatus, _statusMessageType);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Performance Thresholds", EditorStyles.boldLabel);

                CpuThresholdMs = EditorGUILayout.FloatField("CPU Spike Threshold (ms)", CpuThresholdMs);
                GcThresholdKb = EditorGUILayout.FloatField("GC Alloc Threshold (KB)", GcThresholdKb);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("임계값을 초과하는 프레임이 감지되면 프로파일러 데이터가 자동으로 기록됩니다.", MessageType.Info);
            },

            keywords = new HashSet<string>(new[] { "Auto", "Profiler", "AI", "LLM", "Performance" })
        };

        return provider;
    }

    private static async void VerifyKeyAsync()
    {
        _isVerifying = true;
        _verificationStatus = "API 키를 검증 중입니다...";
        _statusMessageType = MessageType.Info;

        bool isValid = await LLMClient.ValidateApiKey(ApiKey);

        if (isValid)
        {
            _verificationStatus = "API 키 검증 성공! 정상적으로 사용할 수 있습니다.";
            _statusMessageType = MessageType.Info;
        }
        else
        {
            _verificationStatus = "API 키 검증 실패. 키를 다시 확인해주세요.";
            _statusMessageType = MessageType.Error;
        }

        _isVerifying = false;
        // SettingsProvider 화면 갱신을 위해 InspectorWindow 등을 Repaint해야 할 수 있지만 
        // Preferences 창은 마우스 오버 시 대개 자동 갱신됩니다.
    }
}
