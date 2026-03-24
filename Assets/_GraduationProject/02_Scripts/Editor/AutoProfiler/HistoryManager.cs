using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public static class HistoryManager
{
    private static readonly string HistoryPath = Path.Combine(Directory.GetCurrentDirectory(), "AutoProfilerHistory");

    [Serializable]
    public class HistoryItem
    {
        public string timestamp;
        public LLMClient.AIReportResponse report;
        
        public HistoryItem(LLMClient.AIReportResponse report)
        {
            this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.report = report;
        }
    }

    /// <summary>
    /// AI 분석 리포트를 파일로 저장합니다.
    /// </summary>
    public static void SaveReport(LLMClient.AIReportResponse report)
    {
        if (report == null) return;

        if (!Directory.Exists(HistoryPath))
        {
            Directory.CreateDirectory(HistoryPath);
        }

        var item = new HistoryItem(report);
        string fileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string fullPath = Path.Combine(HistoryPath, fileName);

        string json = JsonConvert.SerializeObject(item, Formatting.Indented);
        File.WriteAllText(fullPath, json);
        
        Debug.Log($"[Auto-Profiler AI] 분석 리포트 저장 완료: {fileName}");
    }

    /// <summary>
    /// 저장된 모든 히스토리 항목을 불러옵니다. (최신순)
    /// </summary>
    public static List<HistoryItem> LoadAllHistory()
    {
        var items = new List<HistoryItem>();
        if (!Directory.Exists(HistoryPath)) return items;

        var files = Directory.GetFiles(HistoryPath, "*.json")
                             .OrderByDescending(f => f) // 파일명 기준 정렬 (시간순)
                             .ToList();

        foreach (var file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                var item = JsonConvert.DeserializeObject<HistoryItem>(json);
                if (item != null) items.Add(item);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Auto-Profiler AI] 히스토리 파일 로드 실패 ({Path.GetFileName(file)}): {e.Message}");
            }
        }

        return items;
    }

    /// <summary>
    /// 히스토리 데이터를 모두 삭제합니다.
    /// </summary>
    public static void ClearHistory()
    {
        if (Directory.Exists(HistoryPath))
        {
            Directory.Delete(HistoryPath, true);
            Debug.Log("[Auto-Profiler AI] 모든 히스토리 데이터 삭제 완료");
        }
    }
}
