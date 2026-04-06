using UnityEngine;
using System.Collections.Generic;

public class PerformanceTestScenario : MonoBehaviour
{
    [Header("Settings")]
    public bool enableBadCode = false;
    public int loopCount = 5000;

    private List<string> dataList = new List<string>();

    void Update()
    {
        if (!enableBadCode) return;

        // 1. 과도한 GC 할당 (매 프레임 새로운 배열 생성)
        CauseMemorySpike();

        // 2. 비효율적인 유니티 API 호출 (Update에서 GetComponent 반복 호출)
        CauseApiInefficiency();

        // 3. 무거운 연산 (CPU 스파이크)
        CauseCpuSpike();
    }

    /// <summary>
    /// 매 프레임 새로운 리스트를 만들고 문자열을 추가하여 GC 부하를 일으킵니다.
    /// </summary>
    void CauseMemorySpike()
    {
        string[] tempArray = new string[1000];
        for (int i = 0; i < tempArray.Length; i++)
        {
            tempArray[i] = "Data_" + i; // 문자열 결합은 메모리를 많이 소모합니다.
        }
    }

    /// <summary>
    /// Update 함수 내부에서 GetComponent를 반복 호출하는 전형적인 성능 저하 사례입니다.
    /// </summary>
    void CauseApiInefficiency()
    {
        for (int i = 0; i < 100; i++)
        {
            // 이 컴포넌트를 캐싱하지 않고 매번 찾습니다.
            Transform t = GetComponent<Transform>();
            float x = t.position.x;
        }
    }

    /// <summary>
    /// 복잡한 수학 연산을 루프로 돌려 CPU 스파이크를 발생시킵니다.
    /// </summary>
    void CauseCpuSpike()
    {
        float value = 0;
        for (int i = 0; i < loopCount; i++)
        {
            value += Mathf.Sqrt(Mathf.Pow(i, 2) + Mathf.Sin(i));
        }
    }
}
