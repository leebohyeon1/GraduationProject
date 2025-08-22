# Excel Converter 라이브러리

## 개요
Excel Converter는 유니티의 ScriptableObject와 엑셀 파일 간의 양방향 데이터 동기화를 위한 라이브러리입니다. 이 라이브러리를 사용하면 게임 데이터를 엑셀에서 편집하고 유니티로 가져오거나, 유니티에서 수정한 데이터를 엑셀로 내보낼 수 있습니다.

## 주요 기능
- ScriptableObject ↔ Excel 간 양방향 데이터 동기화
- 자동 타입 변환 지원 (int, float, bool, Vector2, Vector3, Color 등)
- 배열 및 리스트 동기화 지원
- 중첩 클래스 구조 지원
- 자동 동기화 설정 기능

## 구조

### 코어 모듈
- **IDataConverter**: 다양한 데이터 타입 간 변환 인터페이스
- **DataConverter**: 데이터 타입 변환 구현체

### 설정 모듈
- **ISettingsProvider**: 엑셀 동기화 설정 제공 인터페이스
- **ExcelSyncSettingsSO**: 동기화 설정을 관리하는 ScriptableObject
- **SyncSettingItem**: 동기화 설정 항목 클래스

### 변환 모듈
- **IExcelImporter**: 엑셀에서 ScriptableObject로 가져오기 인터페이스
- **ExcelImporter**: 엑셀 데이터를 ScriptableObject로 가져오는 구현체
- **IExcelExporter**: ScriptableObject에서 엑셀로 내보내기 인터페이스
- **ExcelExporter**: ScriptableObject 데이터를 엑셀로 내보내는 구현체

## 사용 방법

### 1. 라이브러리 초기화
라이브러리는 유니티 에디터 시작 시 자동으로 초기화됩니다. 필요한 경우 수동으로 초기화할 수도 있습니다:

```csharp
ExcelConverterInitializer.Initialize();
```

### 2. 동기화 설정 추가

```csharp
// 설정 프로바이더 가져오기
var settings = ExcelSyncSettingsSO.Instance;

// 새 설정 항목 추가
settings.AddSyncItem(
    "ItemDatabase",              // 이름
    "Data/ItemDatabase.asset",   // ScriptableObject 경로
    "ExcelData/Items.xlsx",      // 엑셀 파일 경로
    true                         // 자동 동기화 여부
);
```

### 3. 엑셀 파일에서 데이터 가져오기

```csharp
// ScriptableObject 인스턴스
var myData = AssetDatabase.LoadAssetAtPath<MyScriptableObject>("Assets/Data/MyData.asset");

// 엑셀 파일 경로
string excelPath = Path.GetFullPath(Path.Combine(Application.dataPath, "ExcelData/MyData.xlsx"));

// 임포터 생성 및 데이터 가져오기
var importer = new ExcelImporter<MyScriptableObject>(myData, excelPath);
bool success = importer.Import();

if (success)
{
    EditorUtility.SetDirty(myData);
    AssetDatabase.SaveAssets();
}
```

### 4. ScriptableObject에서 엑셀 파일로 데이터 내보내기

```csharp
// ScriptableObject 인스턴스
var myData = AssetDatabase.LoadAssetAtPath<MyScriptableObject>("Assets/Data/MyData.asset");

// 엑셀 파일 경로
string excelPath = Path.GetFullPath(Path.Combine(Application.dataPath, "ExcelData/MyData.xlsx"));

// 익스포터 생성 및 데이터 내보내기
var exporter = new ExcelExporter<MyScriptableObject>(myData, excelPath);
bool success = exporter.Export();
```

## 지원하는 데이터 타입

### 기본 타입
- int, float, double, bool, string
- DateTime
- Enum 타입
- Unity 타입 (Vector2, Vector3, Color)

### 복합 타입
- 배열 (예: `int[]`, `string[]`)
- 리스트 (예: `List<int>`, `List<string>`)
- 사용자 정의 클래스
- 중첩된 클래스 구조

## 주의사항
1. 엑셀 파일을 편집할 때 헤더 행(첫 번째 행)은 수정하지 마세요.
2. 엑셀 파일을 열고 있는 상태에서 동기화를 시도하면 충돌 해결 대화 상자가 표시됩니다.
3. 복잡한 배열이나 리스트 구조는 각각 별도의 시트로 처리됩니다.

## 의존성
- ClosedXML 라이브러리 (Excel 파일 조작)
- BH_Lib.DI (의존성 주입 시스템)
