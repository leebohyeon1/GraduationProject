# Excel Converter

Unity의 ScriptableObject와 Excel 파일을 양방향으로 손쉽게 동기화하는 에디터 확장 라이브러리입니다. `BH_Lib.DI` 시스템을 기반으로 동작하여, 설정과 실제 변환 로직이 분리된 유연한 구조를 가집니다.

데이터를 Excel에서 편리하게 관리하고, Unity에서는 ScriptableObject로 안전하게 사용하세요.

## 주요 기능

- **강력한 양방향 동기화**: ScriptableObject ↔ Excel 간 데이터 동기화
- **에디터 통합**: 메뉴와 인스펙터를 통해 코딩 없이 동기화 설정 및 실행 가능
- **자동 동기화**: ScriptableObject 또는 Excel 파일 변경 시 자동으로 동기화 실행
- **폭넓은 타입 지원**: 기본 타입, Unity 타입(Vector, Color 등), Enum, 배열, 리스트, 중첩 클래스 등 대부분의 데이터 구조 지원
- **파일 잠금 감지**: Excel 파일이 열려있을 경우, 안전한 동기화를 위한 알림 및 처리 기능 제공

## 사용 방법

### 1. 동기화 설정 (코딩 불필요)

가장 일반적인 사용법은 에디터 메뉴를 이용하는 것입니다.

1.  상단 메뉴에서 `BH_Lib > 엑셀 동기화 설정 관리`를 선택하여 설정 창을 엽니다.
2.  '새 항목 추가' 섹션에서 동기화할 ScriptableObject와 Excel 파일의 경로를 지정하고 '추가' 버튼을 누릅니다.
    -   `…` 버튼을 눌러 파일 탐색기로 쉽게 경로를 지정할 수 있습니다.

### 2. 동기화 실행

- **자동 동기화 (기본)**: 설정된 ScriptableObject나 Excel 파일을 저장하면, 변경 사항이 자동으로 감지되어 동기화가 실행됩니다.
- **수동 동기화**: 
  - `엑셀 동기화 설정` 창의 '일괄 동기화' 버튼으로 모든 항목을 한 번에 동기화할 수 있습니다.
  - 동기화할 ScriptableObject를 선택하고, 인스펙터 창에 나타나는 `Excel Tool Window`에서 'Excel로 내보내기' 또는 'Excel에서 가져오기' 버튼을 눌러 개별적으로 동기화할 수 있습니다.

### 3. 스크립트를 통한 제어 (고급)

필요한 경우, 스크립트를 통해 동기화 유틸리티를 직접 호출할 수 있습니다.

```csharp
[Inject] private ISyncUtility _syncUtility;

public void SyncMyData()
{
    // 이름으로 동기화 실행
    _syncUtility.ExportSOToExcel("MyDataName");
    _syncUtility.ImportExcelToSO("MyDataName");

    // ScriptableObject 인스턴스로 직접 실행
    var mySo = AssetDatabase.LoadAssetAtPath<MyScriptableObject>("Assets/Path/To/MySO.asset");
    _syncUtility.ExportSOToExcel(mySo);
}
```

## 내부 동작

- **`ExcelSyncSettingsSO`**: 모든 동기화 설정을 담고 있는 ScriptableObject입니다. 이 설정은 DI 컨테이너에 `ISettingsProvider`로 등록됩니다.
- **`SyncUtility`**: 실제 동기화 로직을 수행하는 핵심 클래스입니다. `IExcelExporter`, `IExcelImporter`를 내부적으로 사용하여 변환을 처리하며, `ISyncUtility` 인터페이스로 DI 컨테이너에 등록됩니다.
- **`ExcelSyncPostprocessor`**: Unity 에디터의 에셋 변경을 감지하여 `SyncUtility`를 통해 자동 동기화를 트리거합니다.

## 지원하는 데이터 타입

- **기본 타입**: `int`, `float`, `double`, `bool`, `string`, `DateTime`, `Enum`
- **Unity 타입**: `Vector2`, `Vector3`, `Color`
- **복합 타입**: 배열(`int[]`), 리스트(`List<string>`), 그리고 사용자가 정의한 클래스를 포함한 리스트(`List<MyClass>`) 및 중첩 클래스

## 의존성

- **BH_Lib.DI**: 필수. 모든 내부 구성요소가 의존성 주입으로 연결됩니다.
- **ClosedXML**: Excel(.xlsx) 파일 처리를 위해 내장되어 있습니다.