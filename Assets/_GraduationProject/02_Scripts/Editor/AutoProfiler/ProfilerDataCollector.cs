using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;
using Unity.Profiling;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public static class ProfilerDataCollector
{
    // 수집된 데이터를 담는 클래스
    [System.Serializable]
    public class SpikeData
    {
        public float cpuTimeMs;
        public float gcAllocKb;
        
        // GPU 지표
        public int gpuSetPassCalls;
        public int gpuBatches;
        public int gpuTriangles;
        
        // 물리 지표
        public int physicsActiveBodies;
        public int physicsContacts;
        
        // 메모리 지표
        public float memoryTotalUsedMb;
        public float memoryGfxUsedMb;

        public List<string> topSamples = new List<string>();
        public double timestamp;

        public string ToJson() => JsonUtility.ToJson(this);
    }

    public static List<SpikeData> CollectedSpikes = new List<SpikeData>();
    
    // 기본 레코더
    private static ProfilerRecorder mainThreadRecorder;
    private static ProfilerRecorder gcAllocRecorder;
    
    // 확장 레코더 (GPU)
    private static ProfilerRecorder setPassCallsRecorder;
    private static ProfilerRecorder batchesRecorder;
    private static ProfilerRecorder trianglesRecorder;
    
    // 확장 레코더 (Physics)
    private static ProfilerRecorder activeBodiesRecorder;
    private static ProfilerRecorder contactsRecorder;
    
    // 확장 레코더 (Memory)
    private static ProfilerRecorder totalMemoryRecorder;
    private static ProfilerRecorder gfxMemoryRecorder;

    private static bool isMonitoring = false;

    static ProfilerDataCollector()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode) StartMonitoring();
        else if (state == PlayModeStateChange.ExitingPlayMode) StopMonitoring();
    }

    private static void StartMonitoring()
    {
        // CPU & GC
        mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 15);
        
        // GPU
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", 15);
        batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", 15);
        trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count", 15);
        
        // Physics
        activeBodiesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Physics, "Active Dynamic Bodies Count", 15);
        contactsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Physics, "Contacts Count", 15);
        
        // Memory
        totalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 1);
        gfxMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory", 1);
        
        CollectedSpikes.Clear();
        EditorApplication.update += MonitorUpdate;
        isMonitoring = true;
        Debug.Log("[Auto-Profiler AI] 데이터 수집 시작 (지표 확장 모드)");
    }

    private static void StopMonitoring()
    {
        if (!isMonitoring) return;
        
        mainThreadRecorder.Dispose();
        gcAllocRecorder.Dispose();
        setPassCallsRecorder.Dispose();
        batchesRecorder.Dispose();
        trianglesRecorder.Dispose();
        activeBodiesRecorder.Dispose();
        contactsRecorder.Dispose();
        totalMemoryRecorder.Dispose();
        gfxMemoryRecorder.Dispose();

        EditorApplication.update -= MonitorUpdate;
        isMonitoring = false;
    }

    private static void MonitorUpdate()
    {
        if (!Application.isPlaying) return;

        float currentCpuMs = mainThreadRecorder.LastValue / 1_000_000f;
        float currentGcKb = gcAllocRecorder.LastValue / 1024f;

        if (currentCpuMs >= AutoProfilerSettings.CpuThresholdMs || currentGcKb >= AutoProfilerSettings.GcThresholdKb)
        {
            CaptureFrameData(currentCpuMs, currentGcKb);
        }
    }

    private static void CaptureFrameData(float cpuMs, float gcKb)
    {
        if (CollectedSpikes.Count > 0 && EditorApplication.timeSinceStartup - CollectedSpikes.Last().timestamp < 0.5f) return;
        if (CollectedSpikes.Count >= 15) return;

        SpikeData spike = new SpikeData 
        { 
            cpuTimeMs = cpuMs, 
            gcAllocKb = gcKb, 
            timestamp = EditorApplication.timeSinceStartup,
            
            // GPU 데이터 캡처
            gpuSetPassCalls = (int)setPassCallsRecorder.LastValue,
            gpuBatches = (int)batchesRecorder.LastValue,
            gpuTriangles = (int)trianglesRecorder.LastValue,
            
            // 물리 데이터 캡처
            physicsActiveBodies = (int)activeBodiesRecorder.LastValue,
            physicsContacts = (int)contactsRecorder.LastValue,
            
            // 메모리 데이터 캡처 (Byte -> MB)
            memoryTotalUsedMb = totalMemoryRecorder.LastValue / (1024f * 1024f),
            memoryGfxUsedMb = gfxMemoryRecorder.LastValue / (1024f * 1024f)
        };
        int frameIndex = ProfilerDriver.lastFrameIndex;

        // 🔥 유니티 프로파일러의 'Time ms' 컬럼 고유 번호는 5번입니다!
        int timeColumn = 5;

        try
        {
            // 정렬 기준을 5(Time ms)로 지정하여 가장 오래 걸린 순서대로 데이터를 가져옵니다.
            using (var frameDataView = ProfilerDriver.GetHierarchyFrameDataView(frameIndex, 0, HierarchyFrameDataView.ViewModes.Default, timeColumn, false))
            {
                if (frameDataView != null && frameDataView.valid)
                {
                    int rootId = frameDataView.GetRootItemID();
                    ExtractHotPath(frameDataView, rootId, spike.topSamples);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Auto-Profiler AI] 프레임 데이터 추출 실패: {e.Message}");
        }

        CollectedSpikes.Add(spike);
        Debug.LogWarning($"[Auto-Profiler AI] 스파이크 감지! CPU: {cpuMs:F2}ms, GC: {gcKb:F2}KB");
    }

    // 🔥 가장 부하가 심한 경로를 끝까지 추적하는 재귀 함수
    private static void ExtractHotPath(HierarchyFrameDataView view, int currentId, List<string> path)
    {
        int depth = 0;
        int timeColumn = 5; // 🔥 여기도 5번(Time ms) 사용

        while (true)
        {
            string name = view.GetItemName(currentId);

            // 5번 컬럼을 사용해 0.00ms가 아닌 정확한 소요 시간을 가져옵니다!
            float time = view.GetItemColumnDataAsFloat(currentId, timeColumn);

            if (!string.IsNullOrEmpty(name))
            {
                path.Add($"{name} ({time:F2}ms)");
            }

            var children = new List<int>();
            view.GetItemChildren(currentId, children);

            if (children.Count == 0 || depth >= 30) break;

            int maxChild = -1;
            float maxTime = -1f;

            foreach (int child in children)
            {
                string childName = view.GetItemName(child);

                // 에디터/GUI 오버헤드 무시
                if (childName.Contains("EditorLoop") ||
                    childName.Contains("Profiler") ||
                    childName.Contains("IMGUI") ||
                    childName.Contains("GUI"))
                {
                    continue;
                }

                // 자식들 비교할 때도 5번 컬럼 사용
                float childTime = view.GetItemColumnDataAsFloat(child, timeColumn);

                if (childTime > maxTime)
                {
                    maxTime = childTime;
                    maxChild = child;
                }
            }

            if (maxChild == -1) break;

            currentId = maxChild;
            depth++;
        }
    }
}
