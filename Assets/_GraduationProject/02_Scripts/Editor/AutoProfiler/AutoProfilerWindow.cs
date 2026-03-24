using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AutoProfilerWindow : EditorWindow
{
    private enum Tab { Dashboard, History, Settings }
    private Tab currentTab = Tab.Dashboard;

    private LLMClient.AIReportResponse lastReport;
    private int selectedIssueIndex = -1;
    private Vector2 scrollPosLeft;
    private Vector2 scrollPosRight;
    private bool isAnalyzing = false;

    // 설정 및 상태 관련
    private List<LLMClient.ModelInfo> availableModels = new List<LLMClient.ModelInfo>();
    private string[] modelDisplayNames = { "Loading models..." };
    private bool isFetchingModels = false;
    private bool showAdvancedSettings = false; // 고급 설정 폴드아웃 토글
    
    // 연결 상태 표시용 변수
    private string apiStatusText = "Checking connection...";
    private Color apiStatusColor = Color.gray;

    // 히스토리 관련
    private List<HistoryManager.HistoryItem> historyItems = new List<HistoryManager.HistoryItem>();
    private Vector2 historyScrollPos;
    private int selectedHistoryIndex = -1;

    [MenuItem("Window/Analysis/Auto-Profiler AI")]
    public static void ShowWindow()
    {
        GetWindow<AutoProfilerWindow>("Auto-Profiler AI");
    }

    private void OnEnable()
    {
        historyItems = HistoryManager.LoadAllHistory();
        string key = EditorPrefs.GetString("AutoProfiler_ApiKey", "");
        if (!string.IsNullOrEmpty(key))
        {
            RefreshModels();
            TestConnectionSilent(key);
        }
        else
        {
            apiStatusText = "API Key not set.";
            apiStatusColor = new Color(1f, 0.4f, 0.4f);
        }
    }

    private async void RefreshModels()
    {
        string key = EditorPrefs.GetString("AutoProfiler_ApiKey", "");
        if (string.IsNullOrEmpty(key)) return;

        isFetchingModels = true;
        var models = await LLMClient.FetchAvailableModels(key);
        if (models != null && models.Count > 0)
        {
            availableModels = models;
            modelDisplayNames = new string[models.Count];
            for (int i = 0; i < models.Count; i++)
            {
                string displayName = models[i].name.Replace("models/", "");
                displayName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(displayName.Replace("-", " "));
                modelDisplayNames[i] = displayName;
            }
        }
        else
        {
            modelDisplayNames = new string[] { "No available models." };
        }
        isFetchingModels = false;
        Repaint();
    }

    private void OnGUI()
    {
        currentTab = (Tab)GUILayout.Toolbar((int)currentTab, new string[] { "Dashboard", "History", "Settings" });

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical();
        switch (currentTab)
        {
            case Tab.Dashboard: DrawDashboardTab(); break;
            case Tab.History: DrawHistoryTab(); break;
            case Tab.Settings: DrawSettingsTab(); break;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
    }

    private void DrawSettingsTab()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("⚙️ AUTO-PROFILER SETTINGS", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // 1. AI 분석 컨텍스트 (메인 설정)
        DrawGlassHeader("📱 TARGET HARDWARE CONTEXT", new Color(0.3f, 0.8f, 1f, 0.12f));
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Describe the target device and optimization goals for the AI to consider during analysis.", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.Space(2);
        
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;
        
        string contextText = EditorGUILayout.TextArea(AutoProfilerSettings.TargetHardwareContext, textAreaStyle, GUILayout.Height(60));
        if (contextText != AutoProfilerSettings.TargetHardwareContext)
        {
            AutoProfilerSettings.TargetHardwareContext = contextText;
        }
        
        EditorGUILayout.Space(5);
        
        // 고급 설정 (기존 수치 기반 슬라이더)
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Trigger Settings", true, EditorStyles.foldoutHeader);
        if (showAdvancedSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Set the threshold values for performance spike detection.", EditorStyles.miniLabel);
            AutoProfilerSettings.CpuThresholdMs = EditorGUILayout.Slider("CPU Spike (ms)", AutoProfilerSettings.CpuThresholdMs, 5f, 100f);
            AutoProfilerSettings.GcThresholdKb = EditorGUILayout.Slider("GC Allocation (KB)", AutoProfilerSettings.GcThresholdKb, 10f, 1000f);
            AutoProfilerSettings.MaxSpikeCount = EditorGUILayout.IntSlider("Max Data Points", AutoProfilerSettings.MaxSpikeCount, 5, 50);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        // 2. AI 자동화 설정 (AUTOMATION)
        DrawGlassHeader("⚡ AUTOMATION", new Color(1f, 0.5f, 0.3f, 0.12f));
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        AutoProfilerSettings.AutoAnalyzeOnDetection = EditorGUILayout.Toggle("Enable Auto-Analysis", AutoProfilerSettings.AutoAnalyzeOnDetection);
        if (AutoProfilerSettings.AutoAnalyzeOnDetection)
        {
            EditorGUILayout.HelpBox("Automatically request AI analysis when performance drops are detected. (Mind your API quota)", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        // 3. AI 모델 선택 섹션
        DrawGlassHeader("🤖 AI MODEL SELECTION", new Color(0.3f, 0.6f, 1f, 0.12f));
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        if (isFetchingModels)
        {
            EditorGUILayout.LabelField("Checking available models...", EditorStyles.miniLabel);
        }
        else if (availableModels.Count > 0)
        {
            int currentModelIndex = availableModels.FindIndex(m => m.name == LLMClient.ModelID);
            if (currentModelIndex < 0) currentModelIndex = 0;

            int newModelIndex = EditorGUILayout.Popup("Analysis Model", currentModelIndex, modelDisplayNames);
            if (newModelIndex != currentModelIndex)
            {
                LLMClient.ModelID = availableModels[newModelIndex].name;
                TestConnectionSilent(EditorPrefs.GetString("AutoProfiler_ApiKey", ""));
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter API Key to enable model selection.", MessageType.Info);
        }

        if (GUILayout.Button("Refresh Models", GUILayout.Height(25)))
        {
            RefreshModels();
        }
            
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        // 4. API 설정 섹션
        DrawGlassHeader("🔑 API CONFIGURATION", new Color(1f, 0.8f, 0.3f, 0.12f));
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        string currentKey = EditorPrefs.GetString("AutoProfiler_ApiKey", "");
        string newKey = EditorGUILayout.PasswordField("Gemini API Key", currentKey);
        
        if (newKey != currentKey)
        {
            EditorPrefs.SetString("AutoProfiler_ApiKey", newKey);
            RefreshModels();
            TestConnectionSilent(newKey);
        }

        if (GUILayout.Button("Test Connection Now", GUILayout.Height(25)))
        {
            TestConnectionManual(newKey);
        }

        DrawInlineStatus();

        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Auto-Profiler AI v1.5", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawInlineStatus()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(5);
        
        Rect dotRect = GUILayoutUtility.GetRect(10, 10, GUILayout.Width(10));
        dotRect.y += 3;
        EditorGUI.DrawRect(dotRect, apiStatusColor);
        
        EditorGUILayout.Space(5);
        
        var statusStyle = new GUIStyle(EditorStyles.miniLabel) { 
            normal = { textColor = apiStatusColor },
            fontStyle = FontStyle.Bold
        };
        EditorGUILayout.LabelField(apiStatusText, statusStyle);
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
    }

    private async void TestConnectionManual(string key)
    {
        apiStatusText = "Connecting...";
        apiStatusColor = Color.white;
        Repaint();

        bool success = await LLMClient.ValidateApiKey(key);
        UpdateStatusUI(success);
    }

    private async void TestConnectionSilent(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        bool success = await LLMClient.ValidateApiKey(key);
        UpdateStatusUI(success);
    }

    private void UpdateStatusUI(bool success)
    {
        if (success)
        {
            apiStatusText = "Connected (OK)";
            apiStatusColor = new Color(0.4f, 1f, 0.4f);
        }
        else
        {
            apiStatusText = "Disconnected (Error)";
            apiStatusColor = new Color(1f, 0.4f, 0.4f);
        }
        Repaint();
    }

    private void DrawDashboardTab()
    {
        int spikeCount = ProfilerDataCollector.CollectedSpikes.Count;
        bool hasReport = lastReport != null;

        if (spikeCount == 0 && !hasReport)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Auto-Profiler AI Dashboard", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Analysis tools will appear here when performance spikes are detected in Play Mode.", EditorStyles.miniLabel);
            return;
        }

        DrawHeader();

        EditorGUILayout.Space(10);

        if (hasReport)
        {
            DrawAnalysisResult(lastReport);
        }
    }

    private void DrawHistoryTab()
    {
        EditorGUILayout.BeginHorizontal();

        // 좌측: 히스토리 목록
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField(" 📑 ANALYSIS HISTORY", EditorStyles.miniBoldLabel);
        
        if (GUILayout.Button("Refresh", GUILayout.Height(25)))
        {
            historyItems = HistoryManager.LoadAllHistory();
        }

        EditorGUILayout.Space(5);
        historyScrollPos = EditorGUILayout.BeginScrollView(historyScrollPos, EditorStyles.helpBox);
        for (int i = 0; i < historyItems.Count; i++)
        {
            DrawHistoryCard(historyItems[i], i);
        }
        EditorGUILayout.EndScrollView();
        
        GUI.backgroundColor = new Color(1, 0.7f, 0.7f);
        if (GUILayout.Button("Clear All History", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Warning", "Delete all analysis history?", "Yes", "No"))
            {
                HistoryManager.ClearHistory();
                historyItems.Clear();
                selectedHistoryIndex = -1;
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();

        // 우측: 상세 내용
        EditorGUILayout.BeginVertical();
        if (selectedHistoryIndex >= 0 && selectedHistoryIndex < historyItems.Count)
        {
            DrawAnalysisResult(historyItems[selectedHistoryIndex].report);
        }
        else
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Select a record from the list on the left.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawHistoryCard(HistoryManager.HistoryItem item, int index)
    {
        bool isSelected = (selectedHistoryIndex == index);
        Rect cardRect = GUILayoutUtility.GetRect(0, 45, GUILayout.ExpandWidth(true));
        
        if (isSelected) EditorGUI.DrawRect(cardRect, new Color(0.2f, 0.4f, 0.7f, 0.25f));
        else if (cardRect.Contains(Event.current.mousePosition)) EditorGUI.DrawRect(cardRect, new Color(1, 1, 1, 0.03f));

        Rect lineRect = new Rect(cardRect.x + 10, cardRect.yMax - 1, cardRect.width - 20, 1);
        EditorGUI.DrawRect(lineRect, new Color(1, 1, 1, 0.05f));

        Color scoreColor = GetHealthColor(item.report.health_score);
        EditorGUI.DrawRect(new Rect(cardRect.x + 4, cardRect.y + 6, 3, cardRect.height - 12), scoreColor);

        var timeStyle = new GUIStyle(EditorStyles.label) { fontSize = 11, fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal, normal = { textColor = isSelected ? Color.white : new Color(0.8f, 0.8f, 0.8f) } };
        EditorGUI.LabelField(new Rect(cardRect.x + 15, cardRect.y + 6, cardRect.width - 25, 18), item.timestamp, timeStyle);

        var scoreStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = scoreColor } };
        EditorGUI.LabelField(new Rect(cardRect.x + 15, cardRect.y + 24, cardRect.width - 25, 16), $"Health Score: {item.report.health_score}%", scoreStyle);

        if (Event.current.type == EventType.MouseDown && cardRect.Contains(Event.current.mousePosition))
        {
            selectedHistoryIndex = index;
            selectedIssueIndex = 0;
            GUI.FocusControl(null);
            Repaint();
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Auto-Profiler AI Dashboard", EditorStyles.boldLabel);
        
        int spikeCount = ProfilerDataCollector.CollectedSpikes.Count;
        bool hasData = (lastReport != null || spikeCount > 0);

        if (hasData)
        {
            GUI.enabled = !isAnalyzing;
            if (GUILayout.Button("Reset Data", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Reset Data", "Delete all collected spikes and analysis results?", "Yes", "No"))
                {
                    lastReport = null;
                    ProfilerDataCollector.CollectedSpikes.Clear();
                    selectedIssueIndex = -1;
                    GUI.FocusControl(null);
                }
            }
            GUI.enabled = true;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.LabelField($"Captured Data: {spikeCount} spikes detected");

        if (spikeCount > 0)
        {
            DrawMetricsSummary();
            if (!isAnalyzing)
            {
                if (GUILayout.Button("Start AI Analysis", GUILayout.Height(30)))
                {
                    RunAnalysis();
                }
            }
        }
        
        if (isAnalyzing)
        {
            EditorGUILayout.HelpBox("AI is analyzing data. Please wait...", MessageType.Info);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawMetricsSummary()
    {
        var spikes = ProfilerDataCollector.CollectedSpikes;
        if (spikes.Count == 0) return;
        var last = spikes[spikes.Count - 1];

        EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("⚡ CPU/GC", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"CPU: {last.cpuTimeMs:F1}ms");
        EditorGUILayout.LabelField($"GC: {last.gcAllocKb:F1}KB");
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("🎨 GPU", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"Batches: {last.gpuBatches}");
        EditorGUILayout.LabelField($"Tris: {last.gpuTriangles / 1000}k");
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("🏗️ Phys/Mem", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"Contacts: {last.physicsContacts}");
        EditorGUILayout.LabelField($"Mem: {last.memoryTotalUsedMb:F0}MB");
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
    }

    private void DrawAnalysisResult(LLMClient.AIReportResponse report)
    {
        if (report == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        Rect headerRect = EditorGUILayout.BeginVertical(GUILayout.Height(75));
        EditorGUI.DrawRect(headerRect, new Color(0.15f, 0.15f, 0.15f, 0.4f));

        EditorGUILayout.Space(12);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(15);

        Rect scoreCircleRect = GUILayoutUtility.GetRect(55, 55);
        Color scoreColor = GetHealthColor(report.health_score);
        DrawScoreGauge(scoreCircleRect, report.health_score, scoreColor);

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginVertical();
        var summaryTitleStyle = new GUIStyle(EditorStyles.miniBoldLabel) { fontSize = 11, normal = { textColor = new Color(0.7f, 0.8f, 1f) } };
        EditorGUILayout.LabelField("AI PERFORMANCE DIAGNOSIS", summaryTitleStyle);
        
        var summaryContentStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, fontSize = 13, fontStyle = FontStyle.Normal, richText = true, normal = { textColor = Color.white } };
        float sumWidth = position.width - 180;
        float sumHeight = summaryContentStyle.CalcHeight(new GUIContent(report.summary), sumWidth);
        EditorGUILayout.LabelField(report.summary, summaryContentStyle, GUILayout.Height(Mathf.Max(45, sumHeight + 5)));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(12);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(280));
        EditorGUILayout.LabelField("DETECTED BOTTLENECKS", EditorStyles.miniBoldLabel);
        scrollPosLeft = EditorGUILayout.BeginScrollView(scrollPosLeft, GUILayout.ExpandHeight(true));
        for (int i = 0; i < report.bottlenecks.Count; i++)
        {
            DrawIssueCard(report.bottlenecks[i], i, report.bottlenecks.Count);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        scrollPosRight = EditorGUILayout.BeginScrollView(scrollPosRight);
        if (selectedIssueIndex >= 0 && selectedIssueIndex < report.bottlenecks.Count)
        {
            DrawDetailedAnalysis(report.bottlenecks[selectedIssueIndex]);
        }
        else
        {
            DrawSelectPrompt();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawScoreGauge(Rect rect, int score, Color color)
    {
        if (Event.current.type != EventType.Repaint) return;
        Handles.BeginGUI();
        Handles.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        Handles.DrawSolidDisc(rect.center, Vector3.forward, rect.width / 2);
        Handles.color = color;
        Handles.DrawWireArc(rect.center, Vector3.forward, Vector3.up, 360f * (score / 100f), rect.width / 2 - 3);
        Handles.EndGUI();
        var scoreNumStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 22, normal = { textColor = color } };
        EditorGUI.LabelField(rect, score.ToString(), scoreNumStyle);
    }

    private void DrawIssueCard(LLMClient.Bottleneck issue, int index, int totalCount)
    {
        bool isSelected = (selectedIssueIndex == index);
        Color severityColor = GetSeverityColor(issue.severity);
        Rect cardRect = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        if (isSelected) EditorGUI.DrawRect(cardRect, new Color(0.2f, 0.4f, 0.7f, 0.25f));
        else if (cardRect.Contains(Event.current.mousePosition)) EditorGUI.DrawRect(cardRect, new Color(1, 1, 1, 0.03f));

        EditorGUI.DrawRect(new Rect(cardRect.x + 4, cardRect.y + 6, 3, cardRect.height - 12), severityColor);
        EditorGUI.LabelField(new Rect(cardRect.x + 15, cardRect.y + 8, cardRect.width - 25, 18), issue.target_file ?? "Engine Subsystem", EditorStyles.boldLabel);
        
        if (Event.current.type == EventType.MouseDown && cardRect.Contains(Event.current.mousePosition))
        {
            selectedIssueIndex = index;
            GUI.FocusControl(null);
            Repaint();
        }
    }

    private void DrawDetailedAnalysis(LLMClient.Bottleneck selected)
    {
        EditorGUILayout.Space(10);
        DrawGlassHeader("🔍 ROOT CAUSE ANALYSIS", new Color(1, 0.3f, 0.3f, 0.12f));
        
        var contentStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, fontSize = 13, richText = true, padding = new RectOffset(10, 10, 10, 10) };
        float sidebarWidths = (currentTab == Tab.History) ? 530 : 280;
        float availableWidth = position.width - sidebarWidths - 100;

        string formattedDescription = FormatAiText(selected.description);
        float descHeight = contentStyle.CalcHeight(new GUIContent(formattedDescription), availableWidth);
        EditorGUILayout.SelectableLabel(formattedDescription, contentStyle, GUILayout.Height(descHeight + 50));

        DrawGlassHeader("💡 OPTIMIZATION STRATEGY", new Color(0.3f, 1f, 0.5f, 0.12f));
        string formattedSolution = FormatAiText(selected.solution);
        float solHeight = contentStyle.CalcHeight(new GUIContent(formattedSolution), availableWidth);
        EditorGUILayout.SelectableLabel(formattedSolution, contentStyle, GUILayout.Height(solHeight + 50));

        if (!string.IsNullOrEmpty(selected.target_file))
        {
            if (GUILayout.Button($"Open {selected.target_file}")) OpenScript(selected.target_file, selected.line_number);
        }
    }

    private string FormatAiText(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        text = text.Replace("\\n", "\n").Replace("\r\n", "\n").Trim();
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"'([^']*)'", @"<color=#FFD67A>'$1'</color>");
        return text;
    }

    private void DrawGlassHeader(string title, Color color)
    {
        Rect rect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4, rect.height), new Color(color.r, color.g, color.b, 1f));
        EditorGUI.LabelField(new Rect(rect.x + 12, rect.y, rect.width, rect.height), title.ToUpper(), EditorStyles.boldLabel);
    }

    private void DrawSelectPrompt()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Select an issue to see details.", EditorStyles.centeredGreyMiniLabel);
        GUILayout.FlexibleSpace();
    }

    private Color GetSeverityColor(string severity)
    {
        switch (severity?.ToLower())
        {
            case "high": return Color.red;
            case "medium": return Color.yellow;
            case "low": return Color.green;
            default: return Color.gray;
        }
    }

    private async void RunAnalysis()
    {
        isAnalyzing = true;
        lastReport = await LLMClient.RequestAnalysis(ProfilerDataCollector.CollectedSpikes);
        isAnalyzing = false;
        historyItems = HistoryManager.LoadAllHistory();
        Repaint();
    }

    private Color GetHealthColor(int score)
    {
        if (score >= 80) return Color.green;
        if (score >= 50) return Color.yellow;
        return Color.red;
    }

    private void OpenScript(string targetName, int line)
    {
        string className = targetName.Replace(".cs", "").Split('(')[0].Trim();
        string[] guids = AssetDatabase.FindAssets("t:MonoScript " + className);
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null) AssetDatabase.OpenAsset(asset, line > 0 ? line : 1);
        }
    }
}