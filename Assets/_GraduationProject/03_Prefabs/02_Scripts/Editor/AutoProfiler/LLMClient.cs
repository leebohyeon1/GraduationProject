using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class LLMClient
{
    [System.Serializable]
    public class AIReportResponse
    {
        public int health_score;
        public string summary;
        public List<Bottleneck> bottlenecks;
    }

    [System.Serializable]
    public class Bottleneck
    {
        public string severity;
        public string target_file;
        public int line_number;
        public string description;
        public string solution;
    }

    [System.Serializable]
    public class ModelListResponse
    {
        public List<ModelInfo> models;
    }

    [System.Serializable]
    public class ModelInfo
    {
        public string name;
        public string displayName;
        public List<string> supportedGenerationMethods;
    }

    // 선택된 모델 ID를 저장 및 로드
    public static string ModelID 
    {
        get => EditorPrefs.GetString("AutoProfiler_ModelID", "models/gemini-1.5-flash");
        set => EditorPrefs.SetString("AutoProfiler_ModelID", value);
    }

    private static string ApiBaseUrl => "https://generativelanguage.googleapis.com/v1beta/";

    /// <summary>
    /// API에서 사용 가능한 모델 리스트를 가져와서 gemini 계열만 필터링합니다.
    /// </summary>
    public static async Task<List<ModelInfo>> FetchAvailableModels(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        
        string listUrl = $"{ApiBaseUrl}models?key={key.Trim()}";

        using (UnityWebRequest request = UnityWebRequest.Get(listUrl))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<ModelListResponse>(request.downloadHandler.text);
                // gemini 계열이면서 콘텐츠 생성이 가능한 모델만 필터링
                return response.models.FindAll(m => 
                    m.name.Contains("gemini") && 
                    m.supportedGenerationMethods.Contains("generateContent") &&
                    !m.name.Contains("tuning") // 튜닝용 제외
                );
            }
            else
            {
                Debug.LogError($"[Auto-Profiler AI] Failed to load model list: {request.error}");
                return null;
            }
        }
    }

    public static async Task<bool> ValidateApiKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        key = key.Trim();

        string testUrl = $"{ApiBaseUrl}{ModelID}:generateContent?key={key}";
        string jsonPayload = "{\"contents\":[{\"parts\":[{\"text\":\"hi\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest(testUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10; 

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[Auto-Profiler AI] Connection Successful! Model: {ModelID}");
                return true;
            }
            else
            {
                Debug.LogError($"[Auto-Profiler AI] Connection Failed!\nModel Used: {ModelID}\nResponse: {request.downloadHandler.text}");
                return false;
            }
        }
    }

    public static async Task<AIReportResponse> RequestAnalysis(List<ProfilerDataCollector.SpikeData> spikes)
    {
        string apiKey = EditorPrefs.GetString("AutoProfiler_ApiKey", "").Trim();
        if (string.IsNullOrEmpty(apiKey)) return null;

        string prompt = BuildPrompt(spikes);
        string targetContext = AutoProfilerSettings.TargetHardwareContext;

        var requestBody = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = $"You are a Unity Engine Performance Optimization Expert. The following is the target environment and optimization goals for this game:\n[{targetContext}]\nBased on this target environment, identify the most critical bottlenecks and provide solutions. RESPOND ONLY IN ENGLISH AND IN JSON FORMAT." } }
            },
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        string jsonPayload = JsonConvert.SerializeObject(requestBody);
        string fullUrl = $"{ApiBaseUrl}{ModelID}:generateContent?key={apiKey}";

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 90;

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var report = ParseAiResponse(request.downloadHandler.text);
                if (report != null) HistoryManager.SaveReport(report);
                return report;
            }
            return null;
        }
    }

    private static string BuildPrompt(List<ProfilerDataCollector.SpikeData> spikes)
    {
        StringBuilder dataInput = new StringBuilder();
        StringBuilder codeContext = new StringBuilder();
        HashSet<string> seenMethods = new HashSet<string>();

        int count = 1;
        foreach (var spike in spikes)
        {
            dataInput.AppendLine($"[Spike #{count++}] CPU: {spike.cpuTimeMs:F1}ms, GC: {spike.gcAllocKb:F1}KB");
            foreach (var sample in spike.topSamples) 
            {
                dataInput.AppendLine($"- {sample}");
                if (!seenMethods.Contains(sample))
                {
                    string snippet = SourceCodeUtility.GetCodeSnippet(sample);
                    if (!string.IsNullOrEmpty(snippet))
                    {
                        codeContext.AppendLine(snippet);
                        seenMethods.Add(sample);
                    }
                }
            }
        }

        return $@"Analyze these Unity spikes and code. Return JSON.
[Data]
{dataInput}
[Code]
{codeContext}
JSON Schema:
{{
  ""health_score"": 0-100,
  ""summary"": ""string"",
  ""bottlenecks"": [
    {{
      ""severity"": ""High/Medium/Low"",
      ""target_file"": ""string or null"",
      ""line_number"": 0,
      ""description"": ""string"",
      ""solution"": ""string""
    }}
  ]
}}";
    }

    private static AIReportResponse ParseAiResponse(string rawJson)
    {
        try
        {
            var response = JObject.Parse(rawJson);
            string cleanJson = response["candidates"][0]["content"]["parts"][0]["text"].ToString().Trim();
            return JsonConvert.DeserializeObject<AIReportResponse>(cleanJson);
        }
        catch { return null; }
    }
}