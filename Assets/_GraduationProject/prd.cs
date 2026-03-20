using UnityEngine;

public class prd
{
    /*
     프로젝트명: Auto-Profiler AI (유니티 에디터 확장 툴)
1. 프로젝트 개요
이 프로젝트는 유니티 엔진(Unity Editor) 내에서 작동하는 AI 기반의 성능 프로파일링 도구입니다. 게임 플레이 중 발생하는 CPU 스파이크(프레임 드랍)와 GC(가비지 컬렉션) 할당 데이터를 백그라운드에서 수집하고, 사용자가 요청할 때 LLM API(OpenAI 또는 Gemini 등)에 전송하여 원인 분석 및 해결 방향을 제안받는 에디터 윈도우를 개발합니다.
2. 기술 스택 및 제약 사항
환경: Unity 2021.3 LTS 이상 (C#)
UI 프레임워크: EditorGUI (또는 UI Toolkit)
API 연동: REST API (UnityEngine.Networking.UnityWebRequest 사용)
보안: 사용자의 API Key는 반드시 EditorPrefs를 사용하여 로컬에만 저장할 것.
제약: 빌드된 게임이 아닌 유니티 에디터(Editor) 전용 스크립트로 작성해야 함.
3. 핵심 기능 워크플로우
Step 1: 설정 및 기준값 세팅 (Preferences)
Edit > Preferences > Auto-Profiler AI 메뉴를 생성.
입력 필드: API Key (마스킹 처리), CPU 스파이크 임계값(기본값 20ms), GC 임계값(기본값 50KB).
Step 2: 백그라운드 데이터 수집 (Play Mode)
에디터가 Play Mode에 진입하면 ProfilerRecorder를 통해 매 프레임 'Main Thread' 시간과 'GC Allocated In Frame'을 감지.
설정된 임계값을 초과하는 프레임(스파이크)이 감지되면, HierarchyFrameDataView를 사용하여 해당 프레임의 콜스택(가장 부하가 큰 상위 함수 3~5개)을 추출.
수집된 스파이크 데이터는 메모리상의 List에 임시 저장.
Step 3: 수동 분석 요청
Play Mode 종료 시 에디터 윈도우에 "N개의 스파이크 수집됨. 분석하시겠습니까?" 버튼 활성화.
버튼 클릭 시 수집된 List 데이터를 JSON 형식으로 변환하여 LLM API로 POST 요청 전송.
Step 4: 결과 렌더링 및 상호작용 (Editor Window)
LLM이 반환한 JSON을 파싱하여 UI에 렌더링.
상호작용 기능: 분석 결과에 포함된 target_file과 line_number를 바탕으로, UI의 버튼을 누르면 해당 스크립트 파일이 IDE에서 열리도록 구현 (AssetDatabase.OpenAsset).
4. 데이터 모델 및 프롬프트 규격
4.1. LLM 시스템 프롬프트 (System Prompt)
LLM 요청 시 반드시 다음 시스템 프롬프트를 포함해야 하며, 출력은 오직 JSON이어야 합니다.
너는 10년 이상의 경력을 가진 시니어 유니티 성능 최적화 전문가야.
제공된 유니티 프로파일러의 스파이크 데이터(Call Stack, 시간, GC)를 분석하여 병목의 원인과 해결 방향을 제시해.
절대 코드를 직접 짜주지 마. 방향성만 제시해.
반드시 아래 JSON 스키마와 정확히 일치하게 응답하고, 마크다운이나 인사말 등 다른 텍스트는 일절 출력하지 마.

{
  "health_score": 0~100 정수,
  "summary": "핵심 요약 한 줄",
  "bottlenecks": [
    {
      "severity": "High 또는 Medium 또는 Low",
      "target_file": "Assets/Scripts/Player.cs 처럼 정확한 경로 (모르면 null)",
      "line_number": 발생 줄 번호 정수 (모르면 null),
      "description": "발생 원인 설명 (한국어)",
      "solution": "해결을 위한 아키텍처 및 로직 방향성 (한국어)"
    }
  ]
}


4.2. C# 역직렬화 클래스 구조 (JSON Parsing)
응답받은 JSON을 파싱하기 위해 다음 구조를 사용하세요.
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


5. UI 레이아웃 요구사항 (Editor Window)
창을 열었을 때 (Window > Analysis > Auto-Profiler AI) 다음 레이아웃을 구현하세요.
[상단] 건강도 점수 (색상 적용) 및 AI 총평.
[좌우 분할 뷰]
좌측 리스트: 병목 이슈 목록 (High/Med/Low 뱃지 표시, 클릭 시 우측 내용 변경).
우측 디테일: 선택된 이슈의 description (원인) 및 solution (해결 방향) 출력.
우측 하단 버튼: [스크립트 해당 라인 열기] 버튼 배치 (target_file 변수 활용).
6. 개발 지시 사항 (Action Items for AI)
위 명세서를 바탕으로 다음 4개의 C# 스크립트 파일을 작성해 주세요.
AutoProfilerSettings.cs: EditorPrefs를 이용한 환경설정 창 구현 (SettingsProvider 활용).
ProfilerDataCollector.cs: ProfilerRecorder와 HierarchyFrameDataView를 이용한 스파이크 감지 및 콜스택 추출 로직.
LLMClient.cs: 수집된 데이터를 바탕으로 프롬프트를 조립하고 UnityWebRequest로 API 통신을 수행하는 비동기 클래스.
AutoProfilerWindow.cs: 결과를 화면에 보여주고 상호작용하는 유니티 에디터 윈도우 UI.

*/
}
