using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AutoProfilerWindow : EditorWindow
{
    private enum Tab { Dashboard, History }
    private Tab currentTab = Tab.Dashboard;

    private LLMClient.AIReportResponse lastReport;
    private int selectedIssueIndex = -1;
    private Vector2 scrollPosLeft;
    private Vector2 scrollPosRight;
    private bool isAnalyzing = false;

    // 히스토리 관련
    private List<HistoryManager.HistoryItem> historyItems = new List<HistoryManager.HistoryItem>();
    private Vector2 historyScrollPos;
    private int selectedHistoryIndex = -1;

    [MenuItem("Window/Analysis/Auto-Profiler AI")]
    public static void ShowWindow()
    {
        GetWindow<AutoProfilerWindow>("Auto-Profiler AI");
    }

    private void OnGUI()
    {
        currentTab = (Tab)GUILayout.Toolbar((int)currentTab, new string[] { "Dashboard", "History" });

        EditorGUILayout.Space(5);

        if (currentTab == Tab.Dashboard)
        {
            DrawDashboardTab();
        }
        else
        {
            DrawHistoryTab();
        }
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
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        EditorGUILayout.LabelField("분석 기록", EditorStyles.boldLabel);
        
        if (GUILayout.Button("새로고침"))
        {
            historyItems = HistoryManager.LoadAllHistory();
        }

        historyScrollPos = EditorGUILayout.BeginScrollView(historyScrollPos, EditorStyles.helpBox);
        for (int i = 0; i < historyItems.Count; i++)
        {
            var item = historyItems[i];
            bool isSelected = (selectedHistoryIndex == i);
            
            GUI.backgroundColor = isSelected ? new Color(0.7f, 0.7f, 1f) : Color.white;
            if (GUILayout.Button($"{item.timestamp}\nScore: {item.report.health_score}", GUILayout.Height(40)))
            {
                selectedHistoryIndex = i;
                selectedIssueIndex = 0; // 히스토리 선택 시 이슈 선택 초기화
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
        
        if (GUILayout.Button("기록 모두 삭제", GUILayout.Width(200)))
        {
            if (EditorUtility.DisplayDialog("경고", "모든 분석 기록을 삭제하시겠습니까?", "예", "아니오"))
            {
                HistoryManager.ClearHistory();
                historyItems.Clear();
                selectedHistoryIndex = -1;
            }
        }
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

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 타이틀과 초기화 버튼을 한 줄에 배치
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Auto-Profiler AI Dashboard", EditorStyles.boldLabel);
        
        int spikeCount = ProfilerDataCollector.CollectedSpikes.Count;
        bool hasData = (lastReport != null || spikeCount > 0);

        // 데이터가 있을 때만 초기화 버튼 표시
        if (hasData)
        {
            GUI.enabled = !isAnalyzing; // 분석 중에는 클릭 방지
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

        // 가장 최근 스파이크 데이터 기준 요약
        var last = spikes[spikes.Count - 1];

        EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
        
        // 컬럼 1: CPU/GC
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("⚡ CPU/GC", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"CPU: {last.cpuTimeMs:F1}ms");
        EditorGUILayout.LabelField($"GC: {last.gcAllocKb:F1}KB");
        EditorGUILayout.EndVertical();

        // 컬럼 2: GPU
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("🎨 GPU", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"Batches: {last.gpuBatches}");
        EditorGUILayout.LabelField($"Tris: {last.gpuTriangles / 1000}k");
        EditorGUILayout.EndVertical();

        // 컬럼 3: Physics/Memory
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

        // --- 1. 상단 점수 카드 (현대적인 헤더 디자인) ---
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        Rect headerRect = EditorGUILayout.BeginVertical(GUILayout.Height(75));
        EditorGUI.DrawRect(headerRect, new Color(0.15f, 0.15f, 0.15f, 0.4f));

        EditorGUILayout.Space(12);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(15);

        // 점수 원형 강조 UI
        Rect scoreCircleRect = GUILayoutUtility.GetRect(55, 55);
        Color scoreColor = GetHealthColor(report.health_score);
        DrawScoreGauge(scoreCircleRect, report.health_score, scoreColor);

        EditorGUILayout.Space(20);

        // 요약 텍스트 섹션
        EditorGUILayout.BeginVertical();
        var summaryTitleStyle = new GUIStyle(EditorStyles.miniBoldLabel) { fontSize = 11, normal = { textColor = new Color(0.7f, 0.8f, 1f) } };
        EditorGUILayout.LabelField("AI PERFORMANCE DIAGNOSIS", summaryTitleStyle);
        
        var summaryContentStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, fontSize = 13, fontStyle = FontStyle.Normal, richText = true, normal = { textColor = Color.white } };
        
        // 상단 요약 텍스트 높이 동적 계산
        float sumWidth = position.width - 180; // 점수 원형과 여백 제외
        float sumHeight = summaryContentStyle.CalcHeight(new GUIContent(report.summary), sumWidth);
        EditorGUILayout.LabelField(report.summary, summaryContentStyle, GUILayout.Height(Mathf.Max(45, sumHeight + 5)));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(12);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        // --- 2. 메인 분석 영역 (좌우 분할) ---
        EditorGUILayout.BeginHorizontal();

        // 좌측 사이드바: 이슈 카드 리스트
        EditorGUILayout.BeginVertical(GUILayout.Width(280));
        var sectionTitleStyle = new GUIStyle(EditorStyles.miniBoldLabel) { margin = new RectOffset(5, 0, 0, 5) };
        EditorGUILayout.LabelField("DETECTED BOTTLENECKS", sectionTitleStyle);
        
        scrollPosLeft = EditorGUILayout.BeginScrollView(scrollPosLeft, GUILayout.ExpandHeight(true));
        for (int i = 0; i < report.bottlenecks.Count; i++)
        {
            DrawIssueCard(report.bottlenecks[i], i, report.bottlenecks.Count);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // 우측 메인: 상세 정보 영역
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        scrollPosRight = EditorGUILayout.BeginScrollView(scrollPosRight);
        
        if (selectedIssueIndex >= 0 && selectedIssueIndex < report.bottlenecks.Count)
        {
            var selected = report.bottlenecks[selectedIssueIndex];
            DrawDetailedAnalysis(selected);
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

        // 배경 원
        Handles.BeginGUI();
        Handles.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        Handles.DrawSolidDisc(rect.center, Vector3.forward, rect.width / 2);
        
        // 점수 호 (Arc)
        Handles.color = color;
        Handles.DrawWireArc(rect.center, Vector3.forward, Vector3.up, 360f * (score / 100f), rect.width / 2 - 3);
        
        // 얇은 안쪽 원
        Handles.color = new Color(color.r, color.g, color.b, 0.2f);
        Handles.DrawWireDisc(rect.center, Vector3.forward, rect.width / 2 - 8);
        Handles.EndGUI();

        // 점수 숫자
        var scoreNumStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 22, normal = { textColor = color } };
        EditorGUI.LabelField(rect, score.ToString(), scoreNumStyle);
    }

    private void DrawIssueCard(LLMClient.Bottleneck issue, int index, int totalCount)
    {
        bool isSelected = (selectedIssueIndex == index);
        Color severityColor = GetSeverityColor(issue.severity);
        
        Rect cardRect = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        
        // 배경 디자인
        if (isSelected) EditorGUI.DrawRect(cardRect, new Color(0.2f, 0.4f, 0.7f, 0.25f));
        else if (cardRect.Contains(Event.current.mousePosition)) EditorGUI.DrawRect(cardRect, new Color(1, 1, 1, 0.03f));

        // 하단 구분선
        if (index < totalCount - 1)
        {
            Rect lineRect = new Rect(cardRect.x + 10, cardRect.yMax - 1, cardRect.width - 20, 1);
            EditorGUI.DrawRect(lineRect, new Color(1, 1, 1, 0.05f));
        }

        // 왼쪽 심각도 인디케이터
        Rect barRect = new Rect(cardRect.x + 4, cardRect.y + 6, 3, cardRect.height - 12);
        EditorGUI.DrawRect(barRect, severityColor);

        // 텍스트 레이아웃
        Rect textRect = new Rect(cardRect.x + 15, cardRect.y + 8, cardRect.width - 25, 18);
        var titleStyle = new GUIStyle(EditorStyles.label) { fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal, fontSize = 12, normal = { textColor = isSelected ? Color.white : new Color(0.85f, 0.85f, 0.85f) } };
        EditorGUI.LabelField(textRect, issue.target_file ?? "Engine Subsystem", titleStyle);

        Rect descRect = new Rect(cardRect.x + 15, cardRect.y + 26, cardRect.width - 25, 16);
        var descStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } };
        string shortDesc = issue.description.Length > 40 ? issue.description.Substring(0, 37) + "..." : issue.description;
        EditorGUI.LabelField(descRect, shortDesc, descStyle);

        // 인터랙션
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
        
        // --- 섹션 1: 원인 분석 ---
        DrawGlassHeader("🔍 ROOT CAUSE ANALYSIS", new Color(1, 0.3f, 0.3f, 0.12f));
        
        var contentStyle = new GUIStyle(EditorStyles.label) 
        { 
            wordWrap = true, 
            fontSize = 13, 
            richText = true, 
            padding = new RectOffset(15, 15, 12, 12),
            normal = { textColor = new Color(0.92f, 0.92f, 0.92f) }
        };

        string formattedDescription = FormatAiText(selected.description);
        
        // 가용 너비 계산 (사이드바 280 + 간격/스크롤바 여유 70)
        float availableWidth = position.width - 350; 
        if (availableWidth < 150) availableWidth = 150;
        
        float descHeight = contentStyle.CalcHeight(new GUIContent(formattedDescription), availableWidth);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        // SelectableLabel은 높이를 약간 더 넉넉하게 주어야 짤리지 않음
        EditorGUILayout.SelectableLabel(formattedDescription, contentStyle, GUILayout.Height(descHeight + 40));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        // --- 섹션 2: 해결책 ---
        DrawGlassHeader("💡 OPTIMIZATION STRATEGY", new Color(0.3f, 1f, 0.5f, 0.12f));
        
        var solutionStyle = new GUIStyle(contentStyle);
        solutionStyle.normal.textColor = new Color(0.85f, 1f, 0.85f);

        string formattedSolution = FormatAiText(selected.solution);
        float solHeight = solutionStyle.CalcHeight(new GUIContent(formattedSolution), availableWidth);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.SelectableLabel(formattedSolution, solutionStyle, GUILayout.Height(solHeight + 40));
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(25);

        // 스크립트 액션 버튼
        if (!string.IsNullOrEmpty(selected.target_file))
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.25f, 0.5f, 0.9f);
            if (GUILayout.Button($" 📂 Open {selected.target_file} ", GUILayout.Height(38), GUILayout.Width(240)))
            {
                OpenScript(selected.target_file, selected.line_number);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(25);
        }
    }

    /// <summary>
    /// AI가 보낸 마크다운 텍스트를 유니티 리치 텍스트로 변환하여 가독성을 높입니다.
    /// </summary>
    private string FormatAiText(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        // 1. 마크다운 굵게: **text** -> <b>text</b>
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>");
        
        // 2. 마크다운 기울임: *text* -> <i>text</i>
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*(.*?)\*", "<i>$1</i>");

        // 3. 마크다운 제목: # Header -> <size=14><b>Header</b></size>
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^#+\s+(.*)$", "<b><size=15><color=#FFFFFF>$1</color></size></b>", System.Text.RegularExpressions.RegexOptions.Multiline);

        // 4. 불렛포인트 강조 (줄 시작 부분의 - 또는 * 만 변경)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^[-\*]\s+", "  •  ", System.Text.RegularExpressions.RegexOptions.Multiline);

        // 5. 숫자로 된 리스트 강조 (1. 2. 등)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^(\d+)\.", @"<color=#7ABFFF><b>$1.</b></color>", System.Text.RegularExpressions.RegexOptions.Multiline);

        // 6. 주요 단어(따옴표 안) 강조
        text = System.Text.RegularExpressions.Regex.Replace(text, @"'([^']*)'", @"<color=#FFD67A>'$1'</color>");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"""([^""]*)""", @"<color=#FFD67A>""$1""</color>");

        return text;
    }

    private void DrawGlassHeader(string title, Color color)
    {
        Rect rect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, color);
        
        // 왼쪽 포인트 라인
        Rect lineRect = new Rect(rect.x, rect.y, 4, rect.height);
        EditorGUI.DrawRect(lineRect, new Color(color.r, color.g, color.b, 1f));
        
        var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 11, normal = { textColor = new Color(0.9f, 0.9f, 0.9f) } };
        EditorGUI.LabelField(new Rect(rect.x + 12, rect.y, rect.width, rect.height), title.ToUpper(), style);
    }

    private void DrawSelectPrompt()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 13 };
        EditorGUILayout.LabelField("Select an issue from the list to see detailed analysis.", style);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    private void DrawSectionHeader(string title, Color bgColor) { } // 더이상 사용하지 않음

    private Color GetSeverityColor(string severity)
    {
        switch (severity.ToLower())
        {
            case "high": return new Color(1f, 0.35f, 0.35f);
            case "medium": return new Color(1f, 0.75f, 0.25f);
            case "low": return new Color(0.35f, 0.85f, 0.35f);
            default: return new Color(0.7f, 0.7f, 0.7f);
        }
    }

    private async void RunAnalysis()
    {
        isAnalyzing = true;
        lastReport = await LLMClient.RequestAnalysis(ProfilerDataCollector.CollectedSpikes);
        isAnalyzing = false;
        selectedIssueIndex = (lastReport != null && lastReport.bottlenecks.Count > 0) ? 0 : -1;
        
        // 분석 완료 후 히스토리 갱신
        historyItems = HistoryManager.LoadAllHistory();
        Repaint();
    }

    private Color GetHealthColor(int score)
    {
        if (score >= 80) return Color.green;
        if (score >= 50) return Color.yellow;
        return new Color(1f, 0.4f, 0.4f); // Light Red
    }

    private void OpenScript(string targetName, int line)
    {
        if (string.IsNullOrEmpty(targetName)) return;

        string lowerName = targetName.ToLower();

        // 1. AI가 "진짜 모르겠다"고 선언한 명백한 키워드만 차단 (꼼수 제거!)
        if (lowerName == "n/a" || lowerName == "null" || lowerName.Contains("없음"))
        {
            Debug.LogWarning("[Auto-Profiler AI] AI가 특정 스크립트를 지목하지 못했습니다. (엔진 내부 병목으로 추정)");
            return;
        }

        // 2. 문자열 정제 (확장자 제거, 괄호 제거, 메서드명 제거)
        string className = targetName.Replace(".cs", "").Trim();
        
        int parenIndex = className.IndexOf('(');
        if (parenIndex > 0) className = className.Substring(0, parenIndex).Trim();
        
        if (className.Contains(".")) className = className.Split('.')[0].Trim();

        // 3. 특수문자나 슬래시(/)가 포함된 프로파일러 카테고리 이름(예: "Engine / Thread")은 
        // 애초에 C# 클래스 이름이 될 수 없으므로 안전하게 검색에서 걸러집니다.
        string[] guids = AssetDatabase.FindAssets("t:MonoScript " + className);

        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset, line > 0 ? line : 1);
                Debug.Log($"[Auto-Profiler AI] 스크립트 열기 성공: {path}");
                return;
            }
        }

        // 4. 검색 실패 시 (여기가 핵심!)
        // 내 프로젝트에 없는 이름이라면, AI가 엔진 내부 용어(Garbage Collection 등)를 
        // 스크립트로 착각했거나, 실제로 프로젝트에 없는 파일인 것입니다.
        Debug.LogWarning($"[Auto-Profiler AI] '{className}' 스크립트를 프로젝트에서 찾을 수 없습니다. 사용자 스크립트가 아닌 유니티 엔진 자체의 병목(GC, 렌더링 등)일 확률이 높습니다.");
    }
}
