using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AutoProfilerWindow : EditorWindow
{
    private LLMClient.AIReportResponse lastReport;
    private int selectedIssueIndex = -1;
    private Vector2 scrollPosLeft;
    private Vector2 scrollPosRight;
    private bool isAnalyzing = false;

    [MenuItem("Window/Analysis/Auto-Profiler AI")]
    public static void ShowWindow()
    {
        GetWindow<AutoProfilerWindow>("Auto-Profiler AI");
    }

    private void OnGUI()
    {
        DrawHeader();

        EditorGUILayout.Space(10);

        if (lastReport == null)
        {
            DrawEmptyState();
        }
        else
        {
            DrawAnalysisResult();
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Auto-Profiler AI Dashboard", EditorStyles.boldLabel);
        
        int spikeCount = ProfilerDataCollector.CollectedSpikes.Count;
        EditorGUILayout.LabelField($"수집된 데이터: {spikeCount} 개의 성능 스파이크 감지됨");

        if (spikeCount > 0 && !isAnalyzing)
        {
            if (GUILayout.Button("AI 성능 분석 시작", GUILayout.Height(30)))
            {
                RunAnalysis();
            }
        }
        else if (isAnalyzing)
        {
            EditorGUILayout.HelpBox("AI가 데이터를 분석 중입니다. 잠시만 기다려주세요...", MessageType.Info);
        }

        if (lastReport != null && GUILayout.Button("데이터 초기화", GUILayout.Width(100)))
        {
            lastReport = null;
            ProfilerDataCollector.CollectedSpikes.Clear();
        }
        EditorGUILayout.EndVertical();
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

    private void DrawAnalysisResult()
    {
        // 상단 점수 및 요약
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        Color scoreColor = GetHealthColor(lastReport.health_score);
        GUI.color = scoreColor;
        EditorGUILayout.LabelField($"성능 건강도 점수: {lastReport.health_score}/100", EditorStyles.whiteLargeLabel);
        GUI.color = Color.white;
        EditorGUILayout.LabelField($"총평: {lastReport.summary}", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // 좌우 분할 뷰
        EditorGUILayout.BeginHorizontal();

        // 좌측: 이슈 리스트
        scrollPosLeft = EditorGUILayout.BeginScrollView(scrollPosLeft, GUILayout.Width(250), GUILayout.ExpandHeight(true));
        for (int i = 0; i < lastReport.bottlenecks.Count; i++)
        {
            var issue = lastReport.bottlenecks[i];
            GUI.backgroundColor = (selectedIssueIndex == i) ? new Color(0.7f, 0.7f, 1f) : Color.white;
            
            if (GUILayout.Button($"[{issue.severity}] {issue.target_file ?? "알 수 없음"}", EditorStyles.miniButton, GUILayout.Height(30)))
            {
                selectedIssueIndex = i;
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();

        // 우측: 상세 내용
        scrollPosRight = EditorGUILayout.BeginScrollView(scrollPosRight, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (selectedIssueIndex >= 0 && selectedIssueIndex < lastReport.bottlenecks.Count)
        {
            var selected = lastReport.bottlenecks[selectedIssueIndex];
            
            EditorGUILayout.LabelField("발생 원인", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(selected.description, MessageType.None);
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("해결 방향", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(selected.solution, MessageType.Info);

            EditorGUILayout.Space(20);

            if (!string.IsNullOrEmpty(selected.target_file))
            {
                if (GUILayout.Button("문제가 된 스크립트 열기", GUILayout.Height(40)))
                {
                    OpenScript(selected.target_file, selected.line_number);
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("이슈를 선택하면 상세 분석 결과가 표시됩니다.");
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndHorizontal();
    }

    private async void RunAnalysis()
    {
        isAnalyzing = true;
        lastReport = await LLMClient.RequestAnalysis(ProfilerDataCollector.CollectedSpikes);
        isAnalyzing = false;
        selectedIssueIndex = (lastReport != null && lastReport.bottlenecks.Count > 0) ? 0 : -1;
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
