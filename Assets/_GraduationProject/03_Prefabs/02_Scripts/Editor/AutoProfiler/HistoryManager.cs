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
    /// Saves the AI analysis report to a file.
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
        
        Debug.Log($"[Auto-Profiler AI] Analysis report saved: {fileName}");
    }

    /// <summary>
    /// Loads all saved history items (newest first).
    /// </summary>
    public static List<HistoryItem> LoadAllHistory()
    {
        var items = new List<HistoryItem>();
        if (!Directory.Exists(HistoryPath)) return items;

        var files = Directory.GetFiles(HistoryPath, "*.json")
                             .OrderByDescending(f => f) // Sort by filename (chronological)
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
                Debug.LogWarning($"[Auto-Profiler AI] Failed to load history file ({Path.GetFileName(file)}): {e.Message}");
            }
        }

        return items;
    }

    /// <summary>
    /// Deletes all history data.
    /// </summary>
    public static void ClearHistory()
    {
        if (Directory.Exists(HistoryPath))
        {
            Directory.Delete(HistoryPath, true);
            Debug.Log("[Auto-Profiler AI] All history data cleared.");
        }
    }
}
