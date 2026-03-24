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
    private string[] modelDisplayNames = { "모델 목록을 불러오는 중..." };
    private bool isFetchingModels = false;
    
    // 연결 상태 표시용 변수
    private string apiStatusText = "연결 상태 확인 중...";
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
            apiStatusText = "API 키가 설정되지 않았습니다.";
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
            modelDisplayNames = new string[] { "사용 가능한 모델이 없습니다." };
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

        // 1. AI 모델 선택 섹션
        DrawGlassHeader("🤖 AI MODEL SELECTION", new Color(0.3f, 0.6f, 1f, 0.12f));
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        if (isFetchingModels)
        {
            EditorGUILayout.LabelField("사용 가능한 모델을 확인 중입니다...", EditorStyles.miniLabel);
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
            EditorGUILayout.HelpBox("API 키를 입력하면 모델 목록이 활성화됩니다.", MessageType.Info);
        }

        if (GUILayout.Button("모델 목록 새로고침", GUILayout.Height(25)))
        {
            RefreshModels();
        }
            
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        // 2. API 설정 섹션
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

        if (GUILayout.Button("지금 연결 테스트 실행", GUILayout.Height(25)))
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
        apiStatusText = "연결 시도 중...";
        apiStatusColor = Color.white;
        Repaint();

        // 🚀 이제 팝업창(Dialog)을 띄우지 않고 상태 텍스트만 갱신합니다.
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
            apiStatusText = "API 연결됨 (정상)";
            apiStatusColor = new Color(0.4f, 1f, 0.4f);
        }
        else
        {
            apiStatusText = "API 연결 끊김 (오류)";
            apiStatusColor = new Color(1f, 0.4f, 0.4f);
        }
        Repaint();
    }

    private void DrawDashboardTab()
    {
        DrawHeader();

        EditorGUILayout.Space(10);

        if (lastReport == null)
        {
            DrawEmptyState();
        }
        else
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
        
        if (GUILayout.Button("새로고침", GUILayout.Height(25)))
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
        if (GUILayout.Button("전체 기록 삭제", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("경고", "모든 분석 기록을 삭제하시겠습니까?", "예", "아니오"))
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
            EditorGUILayout.LabelField("왼쪽 목록에서 기록을 선택하세요.", EditorStyles.centeredGreyMiniLabel);
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
            if (GUILayout.Button("데이터 초기화", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("데이터 초기화", "수집된 스파이크와 분석 결과를 모두 삭제하시겠습니까?", "예", "아니오"))
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
        
        EditorGUILayout.LabelField($"수집된 데이터: {spikeCount} 개의 성능 스파이크 감지됨");

        if (spikeCount > 0)
        {
            DrawMetricsSummary();
            if (!isAnalyzing)
            {
                if (GUILayout.Button("AI 성능 분석 시작", GUILayout.Height(30)))
                {
                    RunAnalysis();
                }
            }
        }
        
        if (isAnalyzing)
        {
            EditorGUILayout.HelpBox("AI가 데이터를 분석 중입니다. 잠시만 기다려주세요...", MessageType.Info);
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

    private void DrawEmptyState()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Play Mode에서 성능 저하가 감지되면 여기에 분석 버튼이 나타납니다.", EditorStyles.centeredGreyMiniLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
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