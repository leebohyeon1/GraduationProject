using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class SourceCodeUtility
{
    /// <summary>
    /// 샘플 이름(예: "Update")을 기반으로 관련 스크립트와 함수 코드를 찾아 반환합니다.
    /// </summary>
    public static string GetCodeSnippet(string sampleName)
    {
        if (string.IsNullOrEmpty(sampleName)) return null;

        // 1. 샘플 이름에서 메서드명만 추출 (예: "MyMethod (10.5ms)" -> "MyMethod")
        string methodName = CleanSampleName(sampleName);
        if (IsEngineMethod(methodName)) return null;

        // 2. 프로젝트 내 모든 .cs 파일 검색 (성능을 위해 캐싱이 필요할 수 있지만 일단 MVP)
        string[] guids = AssetDatabase.FindAssets("t:MonoScript");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("Assets/")) continue; // 라이브러리/패키지 제외

            string content = File.ReadAllText(path);
            
            // 3. 파일 내용에 해당 메서드 정의가 있는지 확인 (간단한 정규식)
            // 주의: 완벽한 파싱은 아니지만, 대부분의 경우 동작합니다.
            if (content.Contains("void " + methodName) || content.Contains("IEnumerator " + methodName) || content.Contains(" " + methodName + "("))
            {
                return ExtractMethodBody(content, methodName, path);
            }
        }

        return null;
    }

    private static string CleanSampleName(string raw)
    {
        int parenIndex = raw.IndexOf('(');
        string name = parenIndex > 0 ? raw.Substring(0, parenIndex) : raw;
        return name.Trim();
    }

    private static bool IsEngineMethod(string name)
    {
        // 유니티 엔진 내부 메서드는 소스 코드를 찾을 수 없으므로 제외
        string[] engineKeywords = { "EditorLoop", "Profiler", "IMGUI", "GUI", "Invoke", "Broadcast", "Execute" };
        foreach (var key in engineKeywords) if (name.Contains(key)) return true;
        return false;
    }

    private static string ExtractMethodBody(string fileContent, string methodName, string filePath)
    {
        // 메서드 위치를 찾고 중괄호 {} 쌍을 맞춰서 블록 전체를 추출하는 로직
        // 정교한 파서가 아니므로 약 20줄 정도의 컨텍스트를 반환합니다.
        int index = fileContent.IndexOf(methodName);
        if (index == -1) return null;

        int startLine = 0;
        string[] lines = fileContent.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(methodName))
            {
                startLine = i;
                break;
            }
        }

        // 해당 라인 기준으로 앞뒤 15줄 정도씩 추출
        int from = Mathf.Max(0, startLine - 2);
        int to = Mathf.Min(lines.Length - 1, startLine + 15);

        string snippet = $"[File: {Path.GetFileName(filePath)}]\n";
        for (int i = from; i <= to; i++)
        {
            snippet += $"{i + 1}: {lines[i]}\n";
        }

        return snippet;
    }
}
