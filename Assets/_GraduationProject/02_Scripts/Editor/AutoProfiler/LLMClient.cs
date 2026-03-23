using UnityEngine;
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

    private const string ModelID = "gemini-2.5-flash"; // 현재 서비스 중인 최신 Flash 모델로 변경!
    private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/" + ModelID + ":generateContent?key=";

    public static async Task<bool> ValidateApiKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        key = key.Trim();

        string jsonPayload = "{\"contents\":[{\"parts\":[{\"text\":\"hi\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest(ApiUrl + key, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10; // 10초 타임아웃 추가

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[Auto-Profiler AI] API Key 검증 성공! (Model: {ModelID})");
                return true;
            }
            else
            {
                Debug.LogError($"[Auto-Profiler AI] 검증 실패!\n상태코드: {request.responseCode}\n에러내용: {request.error}\n응답: {request.downloadHandler.text}");
                return false;
            }
        }
    }

    public static async Task<AIReportResponse> RequestAnalysis(List<ProfilerDataCollector.SpikeData> spikes)
    {
        // AutoProfilerSettings.ApiKey가 구현되어 있다고 가정
        string apiKey = AutoProfilerSettings.ApiKey.Trim();
        if (string.IsNullOrEmpty(apiKey)) return null;

        string prompt = BuildPrompt(spikes);

        // Gemini 1.5 JSON 모드 및 시스템 프롬프트 적용
        var requestBody = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = "너는 유니티 엔진 최적화 전문가야. 사용자의 프로파일러 데이터를 분석하고 문제점과 해결책을 제시해." } }
            },
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json" // 중요: 마크다운 없이 순수 JSON만 반환하도록 강제
            }
        };

        string jsonPayload = JsonConvert.SerializeObject(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(ApiUrl + apiKey, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 90; // 분석 데이터가 많으므로 90초로 넉넉하게 대기

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return ParseAiResponse(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Auto-Profiler AI] 분석 실패 (네트워크/API 에러): {request.error}\n응답: {request.downloadHandler.text}");
                return null;
            }
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
            dataInput.AppendLine($"[Spike #{count++}]");
            dataInput.AppendLine($"- CPU: {spike.cpuTimeMs:F2}ms, GC: {spike.gcAllocKb:F2}KB");
            dataInput.AppendLine($"- GPU: SetPassCalls={spike.gpuSetPassCalls}, Batches={spike.gpuBatches}, Triangles={spike.gpuTriangles}");
            dataInput.AppendLine($"- Physics: ActiveBodies={spike.physicsActiveBodies}, Contacts={spike.physicsContacts}");
            dataInput.AppendLine($"- Memory: Total={spike.memoryTotalUsedMb:F1}MB, Gfx={spike.memoryGfxUsedMb:F1}MB");
            dataInput.AppendLine("  * Hot Path:");
            
            foreach (var sample in spike.topSamples) 
            {
                dataInput.AppendLine($"    - {sample}");

                // 가장 병목이 큰 첫 번째 샘플들의 코드를 추출하여 컨텍스트에 추가
                if (!seenMethods.Contains(sample))
                {
                    string snippet = SourceCodeUtility.GetCodeSnippet(sample);
                    if (!string.IsNullOrEmpty(snippet))
                    {
                        codeContext.AppendLine("--- Source Code Snippet ---");
                        codeContext.AppendLine(snippet);
                        codeContext.AppendLine("---------------------------");
                        seenMethods.Add(sample);
                    }
                }
            }
            dataInput.AppendLine();
        }

        // 🔥 [디버깅용] 소스 코드까지 포함된 최종 데이터를 확인합니다.
        Debug.Log("[Auto-Profiler AI] AI에게 전송되는 컨텍스트 포함 데이터:\n" + dataInput.ToString());

        return $@"다음 유니티 프로파일러 데이터와 관련 소스 코드를 분석하여 최적화 리포트를 작성해줘. 
사용자의 스크립트 코드가 제공된 경우, 해당 코드를 분석하여 구체적인 수정 가이드라인을 제시해.

[프로파일러 데이터]
{dataInput.ToString()}

[관련 소스 코드]
{codeContext.ToString()}

다음 JSON 스키마를 엄격히 따라 응답해:
{{
  ""health_score"": 80,
  ""summary"": ""종합적인 성능 진단 및 개선 방향"",
  ""bottlenecks"": [
    {{
      ""severity"": ""High/Medium/Low"",
      ""target_file"": ""병목이 발생한 C# 파일명(예: Test.cs). 엔진 자체 문제라면 null"",
      ""line_number"": 0,
      ""description"": ""지표와 소스 코드를 근거로 한 병목 원인 설명"",
      ""solution"": ""구체적인 최적화 코드를 포함한 해결 방법""
    }}
  ]
}}";
    }

    private static AIReportResponse ParseAiResponse(string rawJson)
    {
        try
        {
            var response = JObject.Parse(rawJson);

            // 응답이 안전성 문제 등으로 차단되어 candidates가 없을 경우의 예외 처리
            var candidates = response["candidates"];
            if (candidates == null || !candidates.HasValues)
            {
                Debug.LogError("[Auto-Profiler AI] 유효한 AI 응답이 없습니다. (안전성 필터링이나 빈 응답일 수 있음)");
                return null;
            }

            // JSON 모드를 사용했으므로 Replace() 없이 바로 파싱 가능
            string cleanJson = candidates[0]["content"]["parts"][0]["text"].ToString().Trim();
            return JsonConvert.DeserializeObject<AIReportResponse>(cleanJson);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Auto-Profiler AI] 파싱 실패: {e.Message}\nRaw JSON: {rawJson}");
            return null;
        }
    }
}