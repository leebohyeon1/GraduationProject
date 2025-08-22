using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ClosedXML.Excel;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Linq;
using BH_Lib.DI;
using BH_Lib.ExcelConverter.Core;
using Debug = UnityEngine.Debug;
using BH_Lib.ExcelConverter.Editor;

namespace BH_Lib.ExcelConverter.Convert
{
    /// <summary>
    /// 제네릭 타입의 ScriptableObject를 엑셀 파일로 내보내는 클래스
    /// </summary>
    /// <typeparam name="T">ScriptableObject 타입</typeparam>
    [Register(typeof(IExcelExporter<>), LifetimeScope.Transient)]
    public class ExcelExporter<T> : IExcelExporter<T> where T : ScriptableObject
    {
        #region Private Variables
        private readonly T _scriptableObject;
        private readonly string _excelFilePath;
        private readonly bool _showLogs;

        [Inject]
        private readonly IDataConverter _dataConverter;
        #endregion

        #region Constructor
        /// <summary>
        /// ExcelExporter 생성
        /// </summary>
        /// <param name="scriptableObject">대상 ScriptableObject</param>
        /// <param name="excelFilePath">엑셀 파일 경로</param>
        /// <param name="showLogs">로그 출력 여부</param>
        public ExcelExporter(T scriptableObject, string excelFilePath, bool showLogs = false)
        {
            _scriptableObject = scriptableObject;
            _excelFilePath = excelFilePath;
            _showLogs = showLogs;

            // DI 컨테이너 초기화 확인
            if (DIContainer.Instance == null)
            {
                ExcelConverterInitializer.Initialize();
            }

            // DI 컨테이너를 통한 의존성 주입
            DIContainer.Instance.InjectInto(this);
            
            // DataConverter가 주입되지 않은 경우 수동으로 생성
            if (_dataConverter == null)
            {
                _dataConverter = new DataConverter();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// ScriptableObject의 데이터를 엑셀 파일로 내보내기
        /// </summary>
        /// <returns>성공 여부</returns>
        public bool Export()
        {
            UnityEngine.Debug.Log($"[ExcelSync] Export 시작 - SO 타입: {typeof(T).Name}");
            if (_scriptableObject == null)
            {
                UnityEngine.Debug.LogError($"{typeof(T).Name}이 할당되지 않았습니다!");
                return false;
            }
            UnityEngine.Debug.Log($"[ExcelSync] SO 인스턴스 타입: {_scriptableObject.GetType().Name}");

            // 1) 저장 경로 폴더 준비
            string directory = Path.GetDirectoryName(_excelFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // 2) 파일 잠금 여부 먼저 확인
            if (IsFileLocked(_excelFilePath))
            {
                // 사용자에게 두 가지 옵션만 제공
                bool closeAndSync = EditorUtility.DisplayDialog(
                    "엑셀 파일 동기화",
                    $"'{Path.GetFileName(_excelFilePath)}' 파일이 현재 열려있습니다.\n\n" +
                    "• 파일을 닫고 동기화: 엑셀을 직접 닫은 후 스크립터블 오브젝트의 데이터로 동기화합니다.\n\n" +
                    "• 취소: 스크립터블 오브젝트를 현재 엑셀 데이터로 업데이트합니다.",
                    "파일을 닫고 동기화",  // true: Close Excel and sync SO -> Excel
                    "취소"         // false: Reverse sync direction (Excel -> SO)
                );


                if (closeAndSync)
                {
                    if (IsFileLocked(_excelFilePath))
                    {
                        // 파일을 닫고 동기화하는 옵션 선택
                        var notification = new SyncNotificationWindow(
                        Path.GetFileName(_excelFilePath),
                        _excelFilePath,
                        OnExcelManuallyClosedCallback,
                        () =>  {
                            var importer = new ExcelImporter<T>(_scriptableObject, _excelFilePath, _showLogs);
                            bool success = importer.Import();
                            EditorUtility.SetDirty(_scriptableObject);
                            AssetDatabase.SaveAssets();
                                }
                        );
                        notification.ShowUtility();
                        return false; // 지금은 동기화를 중단하고 콜백으로 재시도
                    }
                    else
                    {
                        bool success = Export();
                        if (success)
                        {
                            EditorUtility.DisplayDialog("동기화 완료",
                                $"'{Path.GetFileName(_excelFilePath)}' 파일 동기화가 완료되었습니다.", "확인");
                        }
                    }
                }
                else
                {
                    // 역방향 동기화 옵션 선택 (Excel -> SO)
                    UnityEngine.Debug.Log("사용자 선택에 따라 스크립터블 오브젝트를 엑셀 데이터로 동기화합니다.");
                    return ImportExcelToScriptableObject();
                }
            }

            // 3) 임시 파일 생성
            string tempFilePath = Path.Combine(
                Path.GetDirectoryName(_excelFilePath),
                "_temp_" + Path.GetFileName(_excelFilePath)
            );

            try
            {
                UnityEngine.Debug.Log($"[ExcelSync] 워크북 생성 시작");
                // 4) 워크북 생성 및 시트 작성
                using (var workbook = File.Exists(_excelFilePath) ? new XLWorkbook(_excelFilePath) : new XLWorkbook())
                {
                    UnityEngine.Debug.Log($"[ExcelSync] UpdateOrCreatePrimitiveTypeSheet 호출");
                    // 기본 타입 시트 및 리플렉션 기반 시트 업데이트 또는 생성
                    UpdateOrCreatePrimitiveTypeSheet(workbook);
                    UnityEngine.Debug.Log($"[ExcelSync] UpdateOrCreateReflectionSheets 호출");
                    UpdateOrCreateReflectionSheets(workbook);

                    workbook.SaveAs(tempFilePath);
                }

                // 5) 임시 파일에서 실제 파일로 복사
                File.Copy(tempFilePath, _excelFilePath, true);
                UnityEngine.Debug.Log($"[ExcelSync] 엑셀 파일 내보내기 완료: {_excelFilePath}");
                return true; // 성공
            }
            catch (IOException ioEx) // 파일 접근 IO 예외 처리
            {
                UnityEngine.Debug.LogError($"엑셀 파일 생성/저장 중 IO 오류 발생: {ioEx.Message}");
                EditorUtility.DisplayDialog("오류 발생",
                    $"엑셀 파일 생성/저장 중 오류가 발생했습니다.\n{ioEx.Message}\n\n" +
                    "파일이 다른 프로그램에서 열려있거나 권한 문제가 있을 수 있습니다.", "확인");
                return false;
            }
            catch (Exception e) // 기타 예외 처리
            {
                UnityEngine.Debug.LogError($"엑셀 파일 내보내기 중 오류 발생: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"엑셀 파일 내보내기 중 오류가 발생했습니다:\n{e.Message}", "확인");
                return false;
            }
            finally
            {
                // 6. 성공/실패 여부에 관계없이 임시 파일 삭제
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); }
                    catch (Exception delEx) { UnityEngine.Debug.LogWarning($"임시 파일 삭제 실패: {delEx.Message}"); }
                }
            }
        }

        // 사용자가 엑셀을 수동으로 닫은 후 호출되는 콜백
        private void OnExcelManuallyClosedCallback()
        {
            // 파일이 여전히 잠겨있는지 확인
            if (IsFileLocked(_excelFilePath))
            {
                EditorUtility.DisplayDialog("동기화 실패",
                    $"'{Path.GetFileName(_excelFilePath)}' 파일이 여전히 열려있거나 접근할 수 없습니다.\n\n" +
                    "파일이 완전히 닫혔는지 확인한 후 다시 시도해주세요.", "확인");
                return;
            }

            // 파일이 잠금 해제되었으면 동기화 시도
            bool success = Export();
            if (success)
            {
                EditorUtility.DisplayDialog("동기화 완료",
                    $"'{Path.GetFileName(_excelFilePath)}' 파일 동기화가 완료되었습니다.", "확인");
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// 스크립터블 오브젝트 클래스의 기본 타입 시트 생성 또는 업데이트
        /// </summary>
        /// <param name="workbook">엑셀 워크북</param>
        private void UpdateOrCreatePrimitiveTypeSheet(XLWorkbook workbook)
        {
            Type actualType = null;
            string sheetName = null;
            IXLWorksheet sheet = null;
            
            try
            {
                // 실제 인스턴스의 타입으로부터 이름 가져오기
                actualType = _scriptableObject.GetType();
                sheetName = actualType.Name;

                if (_showLogs) UnityEngine.Debug.Log($"{sheetName} 기본 필드 시트 업데이트 중");

                // 시트가 이미 존재하는지 확인
                if (workbook.TryGetWorksheet(sheetName, out sheet))
                {
                    // 헤더 정보 백업
                    Dictionary<string, int> existingHeaders = CreateHeaderMap(sheet);

                    // 시트 데이터 초기화 (헤더 포함)
                    var usedRows = sheet.RowsUsed();
                    foreach (var row in usedRows.ToList()) // ToList()로 복사본을 만들어 순회 중 삭제 문제 방지
                    {
                        row.Delete();
                    }
                }
                else
                {
                    // 시트가 없으면 새로 생성
                    sheet = workbook.Worksheets.Add(sheetName);
                }

                // 모든 필드 (public 및 SerializeField 어트리뷰트가 있는 필드)를 수집
                var allFields = GetAllSerializableFields(actualType);
                UnityEngine.Debug.Log($"[ExcelSync] 찾은 필드 수: {allFields.Count}");

            // 헤더 생성
            int currentColIndex = 1;
            foreach (var field in allFields)
            {
                    UnityEngine.Debug.Log($"[ExcelSync] 필드 처리: {field.Name} - 타입: {field.FieldType}");
                var fieldType = field.FieldType;

                // 배열 타입 처리
                if (fieldType.IsArray)
                {
                    sheet.Cell(1, currentColIndex).Value = field.Name;
                    AddHeaderComment(sheet.Cell(1, currentColIndex), fieldType);

                    var arrayValue = field.GetValue(_scriptableObject) as Array;
                    if (arrayValue != null)
                    {
                        var elems = new List<string>();
                        foreach (var item in arrayValue)
                            elems.Add(item?.ToString() ?? string.Empty);

                        sheet.Cell(2, currentColIndex).Value = "{" + string.Join(",", elems) + "}";
                    }

                    currentColIndex++;
                    continue;
                }

                // 값 타입 리스트 처리 (새로 추가된 부분)
                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type elementType = fieldType.GetGenericArguments()[0];

                    // 값 타입 리스트 또는 string 리스트인 경우
                    if (elementType.IsValueType || elementType == typeof(string))
                    {
                        sheet.Cell(1, currentColIndex).Value = field.Name;
                        AddHeaderComment(sheet.Cell(1, currentColIndex), fieldType);

                        var listValue = field.GetValue(_scriptableObject) as IEnumerable;
                        if (listValue != null)
                        {
                            var elems = new List<string>();
                            foreach (var item in listValue)
                                elems.Add(item?.ToString() ?? string.Empty);

                            sheet.Cell(2, currentColIndex).Value = "{" + string.Join(",", elems) + "}";
                        }

                        currentColIndex++;
                        continue;
                    }
                }

                // 기본 타입 처리 (기존 코드 유지)
                if (IsPrimitiveOrString(fieldType))
                {
                    sheet.Cell(1, currentColIndex).Value = field.Name;
                    AddHeaderComment(sheet.Cell(1, currentColIndex), fieldType);

                    object value = field.GetValue(_scriptableObject);
                    if (value != null)
                    {
                        if (_dataConverter != null)
                        {
                            sheet.Cell(2, currentColIndex).Value = _dataConverter.ConvertToString(value);
                        }
                        else
                        {
                                UnityEngine.Debug.LogError("[ExcelSync] DataConverter가 null입니다!");
                            sheet.Cell(2, currentColIndex).Value = value.ToString();
                        }
                    }

                    currentColIndex++;
                }
            }

            // 헤더 스타일 적용
            ApplyHeaderStyle(sheet.Row(1));

            // 열 너비 자동 조정
            sheet.Columns().AdjustToContents();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] UpdateOrCreatePrimitiveTypeSheet 처리 중 오류: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }
        /// <summary>
        /// 직렬화 가능한 모든 필드 수집 (public 및 SerializeField 어트리뷰트가 있는 필드)
        /// </summary>
        /// <param name="type">대상 타입</param>
        /// <returns>필드 목록</returns>
        private List<FieldInfo> GetAllSerializableFields(Type type)
        {
            var result = new List<FieldInfo>();

            // 모든 인스턴스 필드 가져오기 (private 및 protected 포함)
            var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in allFields)
            {
                // public 필드는 무조건 추가
                if (field.IsPublic)
                {
                    result.Add(field);
                    continue;
                }

                // private/protected 필드 중 SerializeField 어트리뷰트 있는 것만 추가
                var attributes = field.GetCustomAttributes(typeof(UnityEngine.SerializeField), false);
                if (attributes != null && attributes.Length > 0)
                {
                    result.Add(field);
                }
            }

            return result;
        }
        /// <summary>
        /// 리플렉션 타입 속성 업데이트 또는 생성
        /// </summary>
        /// <param name="workbook">엑셀 워크북</param>
        private void UpdateOrCreateReflectionSheets(XLWorkbook workbook)
        {
            // 모든 직렬화 가능한 필드 수집
            Type actualType = _scriptableObject.GetType();
            UnityEngine.Debug.Log($"[ExcelSync] UpdateOrCreateReflectionSheets - SO 타입: {actualType.Name}");
            var soFields = GetAllSerializableFields(actualType);
            UnityEngine.Debug.Log($"[ExcelSync] 찾은 필드 수: {soFields.Count}");

            // 스크립터블 오브젝트의 필드 타입을 유일화
            var fieldTypes = new HashSet<Type>();

            foreach (var field in soFields)
            {
                UnityEngine.Debug.Log($"[ExcelSync] 필드 처리 중: {field.Name} ({field.FieldType})");
                object fieldValue = field.GetValue(_scriptableObject);
                if (fieldValue == null) 
                {
                    UnityEngine.Debug.Log($"[ExcelSync] 필드 값이 null: {field.Name}");
                    continue;
                }

                var fieldType = field.FieldType;

                // 배열(T[])의 경우 (복합타입)
                if (fieldType.IsArray)
                {
                    Type elementType = fieldType.GetElementType();
                    if (elementType.IsClass && elementType != typeof(string))
                    {
                        fieldTypes.Add(elementType);
                    }
                    continue;
                }

                // List<T>의 경우 (복합타입)
                if (fieldType.IsGenericType &&
                    fieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type elementType = fieldType.GetGenericArguments()[0];
                    if (elementType.IsClass && elementType != typeof(string))
                    {
                        fieldTypes.Add(elementType);
                    }
                    continue;
                }

                // 일반 클래스 타입 필드
                if (fieldType.IsClass && !IsPrimitiveOrString(fieldType))
                {
                    fieldTypes.Add(fieldType);
                }
            }

            // 시트 처리
            foreach (var field in soFields)
            {
                object fieldValue = field.GetValue(_scriptableObject);
                if (fieldValue == null) continue;

                var fieldType = field.FieldType;

                // 배열(T[])의 경우 (복합타입)
                if (fieldType.IsArray)
                {
                    Type elementType = fieldType.GetElementType();
                    if (elementType.IsClass && elementType != typeof(string))
                    {
                        string sheetName = elementType.Name;
                        if (_showLogs) UnityEngine.Debug.Log($"{sheetName} 배열 시트 업데이트 중");
                        UpdateOrCreateListSheet(workbook, sheetName, elementType, (IEnumerable)fieldValue);
                    }
                    continue;
                }

                // List<T>의 경우 (복합타입)
                if (fieldType.IsGenericType &&
                    fieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type elementType = fieldType.GetGenericArguments()[0];
                    if (elementType.IsClass && elementType != typeof(string))
                    {
                        string sheetName = elementType.Name;
                        if (_showLogs) UnityEngine.Debug.Log($"{sheetName} 리스트 시트 업데이트 중");
                        UpdateOrCreateListSheet(workbook, sheetName, elementType, (IEnumerable)fieldValue);
                    }
                    continue;
                }

                // 일반 클래스 타입 필드
                if (fieldType.IsClass && !IsPrimitiveOrString(fieldType))
                {
                    string sheetName = fieldType.Name;
                    if (_showLogs) UnityEngine.Debug.Log($"{sheetName} 시트 업데이트 중");
                    UpdateOrCreateClassSheet(workbook, sheetName, fieldType, fieldValue);
                }
            }

            // 엑셀에 있지만 현재 SO에 없는 시트 삭제 처리 (선택적으로 사용)
            RemoveObsoleteSheets(workbook, fieldTypes);
        }

        /// <summary>
        /// 더 이상 사용되지 않는 시트 삭제
        /// </summary>
        /// <param name="workbook">엑셀 워크북</param>
        /// <param name="validTypes">유효한 타입들</param>
        private void RemoveObsoleteSheets(XLWorkbook workbook, HashSet<Type> validTypes)
        {
            // 스크립터블 오브젝트 자체 시트는 항상 유지
            string mainSheetName = typeof(T).Name;

            // 삭제할 시트 이름 목록
            var sheetsToRemove = new List<string>();

            // 모든 시트 확인
            foreach (var sheet in workbook.Worksheets)
            {
                string sheetName = sheet.Name;

                // 메인 시트는 건너뜀
                if (sheetName == mainSheetName)
                    continue;

                // 유효한 타입 이름과 일치하는지 확인
                bool isValid = false;
                foreach (var type in validTypes)
                {
                    if (type.Name == sheetName)
                    {
                        isValid = true;
                        break;
                    }
                }

                if (!isValid)
                {
                    sheetsToRemove.Add(sheetName);
                }
            }

            // 불필요한 시트 삭제
            foreach (var sheetName in sheetsToRemove)
            {
                if (_showLogs) UnityEngine.Debug.Log($"더 이상 사용되지 않는 시트 삭제: {sheetName}");
                workbook.Worksheet(sheetName).Delete();
            }
        }

        /// <summary>
        /// 일반 클래스 타입의 인스턴스를 시트로 생성/업데이트
        /// </summary>
        /// <param name="workbook">엑셀 워크북</param>
        /// <param name="sheetName">시트 이름</param>
        /// <param name="classType">클래스 타입</param>
        /// <param name="instance">인스턴스</param>
        private void UpdateOrCreateClassSheet(XLWorkbook workbook, string sheetName, Type classType, object instance)
        {
            if (instance == null) return;

            // 클래스의 프로퍼티와 필드 정보 가져오기
            var properties = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = classType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            // 시트 존재 확인
            IXLWorksheet sheet;
            if (workbook.TryGetWorksheet(sheetName, out sheet))
            {
                // 기존 헤더 맵 생성
                var existingHeaderMap = CreateHeaderMap(sheet);

                // 새 헤더 목록 생성
                var newHeaders = new List<string>();
                foreach (var prop in properties)
                {
                    newHeaders.Add(prop.Name);
                }
                foreach (var field in fields)
                {
                    newHeaders.Add(field.Name);
                }

                // 헤더 일치 여부 확인
                bool headersMatch = true;
                if (existingHeaderMap.Count != newHeaders.Count)
                {
                    headersMatch = false;
                }
                else
                {
                    foreach (var header in newHeaders)
                    {
                        if (!existingHeaderMap.ContainsKey(header))
                        {
                            headersMatch = false;
                            break;
                        }
                    }
                }

                // 헤더가 일치하지 않으면 시트 재생성
                if (!headersMatch)
                {
                    if (_showLogs) UnityEngine.Debug.Log($"시트 구조 변경 감지: {sheetName} - 시트를 재생성합니다.");
                    sheet.Delete();
                    sheet = workbook.Worksheets.Add(sheetName);

                    // 새 헤더 추가
                    int col = 1;
                    foreach (var prop in properties)
                    {
                        sheet.Cell(1, col).Value = prop.Name;
                        AddHeaderComment(sheet.Cell(1, col), prop.PropertyType);
                        col++;
                    }
                    foreach (var field in fields)
                    {
                        sheet.Cell(1, col).Value = field.Name;
                        AddHeaderComment(sheet.Cell(1, col), field.FieldType);
                        col++;
                    }

                    // 헤더 스타일 적용
                    ApplyHeaderStyle(sheet.Row(1));
                }
                else
                {
                    // 기존 데이터 초기화 (헤더 유지)
                    var usedRows = sheet.RowsUsed().Skip(1).ToList(); // 헤더 행 제외
                    foreach (var row in usedRows)
                    {
                        row.Delete();
                    }
                }
            }
            else
            {
                // 시트가 없으면 새로 생성
                sheet = workbook.Worksheets.Add(sheetName);

                // 헤더 추가
                int col = 1;
                foreach (var prop in properties)
                {
                    sheet.Cell(1, col).Value = prop.Name;
                    AddHeaderComment(sheet.Cell(1, col), prop.PropertyType);
                    col++;
                }
                foreach (var field in fields)
                {
                    sheet.Cell(1, col).Value = field.Name;
                    AddHeaderComment(sheet.Cell(1, col), field.FieldType);
                    col++;
                }

                // 헤더 스타일 적용
                ApplyHeaderStyle(sheet.Row(1));
            }

            // 헤더 맵 생성
            var headerMap = CreateHeaderMap(sheet);

            // 데이터 추가 (2행부터 시작)
            // 프로퍼티 값 추가
            foreach (var prop in properties)
            {
                if (headerMap.ContainsKey(prop.Name))
                {
                    int col = headerMap[prop.Name];
                    sheet.Cell(2, col).Value = _dataConverter.ConvertToString(prop.GetValue(instance));
                }
            }

            // 필드 값 추가
            foreach (var field in fields)
            {
                if (headerMap.ContainsKey(field.Name))
                {
                    int col = headerMap[field.Name];
                    sheet.Cell(2, col).Value = _dataConverter.ConvertToString(field.GetValue(instance));
                }
            }

            // 열 너비 자동 조정
            sheet.Columns().AdjustToContents();
        }

        /// <summary>
        /// 리스트 타입의 시트 생성/업데이트
        /// </summary>
        /// <param name="workbook">엑셀 워크북</param>
        /// <param name="sheetName">시트 이름</param>
        /// <param name="elementType">요소 타입</param>
        /// <param name="listInstance">리스트 인스턴스</param>
        private void UpdateOrCreateListSheet(XLWorkbook workbook, string sheetName, Type elementType, System.Collections.IEnumerable listInstance)
        {
            if (listInstance == null) return;

            // 리스트 타입의 프로퍼티와 필드 정보 가져오기
            var properties = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = elementType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            // 시트 존재 확인
            IXLWorksheet sheet;
            if (workbook.TryGetWorksheet(sheetName, out sheet))
            {
                // 기존 헤더 맵 생성
                var existingHeaderMap = CreateHeaderMap(sheet);

                // 새 헤더 목록 생성
                var newHeaders = new List<string>();
                foreach (var prop in properties)
                {
                    newHeaders.Add(prop.Name);
                }
                foreach (var field in fields)
                {
                    newHeaders.Add(field.Name);
                }

                // 헤더 일치 여부 확인
                bool headersMatch = true;
                if (existingHeaderMap.Count != newHeaders.Count)
                {
                    headersMatch = false;
                }
                else
                {
                    foreach (var header in newHeaders)
                    {
                        if (!existingHeaderMap.ContainsKey(header))
                        {
                            headersMatch = false;
                            break;
                        }
                    }
                }

                // 헤더가 일치하지 않으면 시트 재생성
                if (!headersMatch)
                {
                    if (_showLogs) UnityEngine.Debug.Log($"리스트 시트 구조 변경 감지: {sheetName} - 시트를 재생성합니다.");
                    sheet.Delete();
                    sheet = workbook.Worksheets.Add(sheetName);

                    // 새 헤더 추가
                    int col = 1;
                    foreach (var prop in properties)
                    {
                        sheet.Cell(1, col).Value = prop.Name;
                        AddHeaderComment(sheet.Cell(1, col), prop.PropertyType);
                        col++;
                    }
                    foreach (var field in fields)
                    {
                        sheet.Cell(1, col).Value = field.Name;
                        AddHeaderComment(sheet.Cell(1, col), field.FieldType);
                        col++;
                    }

                    // 헤더 스타일 적용
                    ApplyHeaderStyle(sheet.Row(1));
                }
                else
                {
                    // 기존 데이터 초기화 (헤더 유지)
                    var usedRows = sheet.RowsUsed().Skip(1).ToList(); // 헤더 행 제외
                    foreach (var dataRow in usedRows) // 'row'에서 'dataRow'로 변수명 변경
                    {
                        dataRow.Delete();
                    }
                }
            }
            else
            {
                // 시트가 없으면 새로 생성
                sheet = workbook.Worksheets.Add(sheetName);

                // 헤더 추가
                int col = 1;
                foreach (var prop in properties)
                {
                    sheet.Cell(1, col).Value = prop.Name;
                    AddHeaderComment(sheet.Cell(1, col), prop.PropertyType);
                    col++;
                }
                foreach (var field in fields)
                {
                    sheet.Cell(1, col).Value = field.Name;
                    AddHeaderComment(sheet.Cell(1, col), field.FieldType);
                    col++;
                }

                // 헤더 스타일 적용
                ApplyHeaderStyle(sheet.Row(1));
            }

            // 헤더 맵 생성
            var headerMap = CreateHeaderMap(sheet);

            // 데이터 추가 (2행부터 시작)
            int rowIndex = 2;
            foreach (var item in listInstance)
            {
                // 프로퍼티 값 추가
                foreach (var prop in properties)
                {
                    if (headerMap.ContainsKey(prop.Name))
                    {
                        int col = headerMap[prop.Name];
                        sheet.Cell(rowIndex, col).Value = _dataConverter.ConvertToString(prop.GetValue(item));
                    }
                }

                // 필드 값 추가
                foreach (var field in fields)
                {
                    if (headerMap.ContainsKey(field.Name))
                    {
                        int col = headerMap[field.Name];
                        sheet.Cell(rowIndex, col).Value = _dataConverter.ConvertToString(field.GetValue(item));
                    }
                }

                rowIndex++;
            }

            // 열 너비 자동 조정
            sheet.Columns().AdjustToContents();
        }

        /// <summary>
        /// 기본 타입 여부 확인
        /// </summary>
        /// <param name="type">타입</param>
        /// <returns>기본 타입 여부</returns>
        private bool IsPrimitiveOrString(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
                   type == typeof(DateTime) || type.IsEnum || type == typeof(Vector2) ||
                   type == typeof(Vector3) || type == typeof(Color);
        }

        /// <summary>
        /// 헤더 스타일 적용
        /// </summary>
        /// <param name="row">헤더 행</param>
        private void ApplyHeaderStyle(IXLRow row)
        {
            row.Style.Font.Bold = true;
            row.Style.Fill.BackgroundColor = XLColor.LightGray;
            row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        /// <summary>
        /// 헤더의 필드에 타입 정보 추가
        /// </summary>
        /// <param name="cell">셀</param>
        /// <param name="fieldType">필드 타입</param>
        private void AddHeaderComment(IXLCell cell, Type fieldType)
        {
            // 셀 값에 타입 정보 추가
            string typeInfo = GetTypeDisplayName(fieldType);
            string currentValue = cell.Value.ToString();
            cell.Value = $"{currentValue} ({typeInfo})";
        }

        /// <summary>
        /// 타입 이름을 읽기 쉬운 형태로 변환
        /// </summary>
        /// <param name="type">타입</param>
        /// <returns>표시용 타입 이름</returns>
        private string GetTypeDisplayName(Type type)
        {
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(Vector2)) return "Vector2(x,y)";
            if (type == typeof(Vector3)) return "Vector3(x,y,z)";
            if (type == typeof(Color)) return "color(r,g,b,a)";
            if (type.IsEnum) return $"Enum({type.Name})";

            return type.Name;
        }

        /// <summary>
        /// 파일이 잠겨 있는지 확인 (엑셀 파일 열려있는지 확인)
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <returns>파일 잠겼는지 여부</returns>
        private bool IsFileLocked(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false; // 파일이 없으면 잠겨있지 않음
            }

            FileStream stream = null;
            try
            {
                // 파일을 읽기/쓰기 모드, 공유なし로 열어보고, 열리지 않으면 다른 프로세스가 파일을 사용 중
                stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                // 파일을 열 수 없는 경우 (IOException 발생), 다른 프로세스가 파일을 잠갔다고 간주
                return true;
            }
            catch (Exception) // 혹은 다른 예외 상황 (예: 권한 문제)
            {
                return true; // 안전을 위해 잠겼다고 간주
            }
            finally
            {
                // 스트림이 열렸다면 반드시 닫아줘야 함
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

            // 파일을 열 수 있다면 잠겨있지 않음
            return false;
        }

        /// <summary>
        /// 헤더 열의 위치 찾기
        /// </summary>
        /// <param name="sheet">시트</param>
        /// <param name="headerName">헤더 이름</param>
        /// <returns>열 번호, 못 찾으면 -1</returns>
        private int FindHeaderColumn(IXLWorksheet sheet, string headerName)
        {
            var headerRow = sheet.Row(1);
            foreach (var cell in headerRow.CellsUsed())
            {
                string cellValue = cell.GetString();

                // 타입 정보 제거 (헤더 이름이 "(타입)" 형태로 표시된 경우)
                int bracketIndex = cellValue.IndexOf(" (");
                if (bracketIndex > 0)
                {
                    cellValue = cellValue.Substring(0, bracketIndex);
                }

                if (cellValue == headerName)
                {
                    return cell.WorksheetColumn().ColumnNumber();
                }
            }
            return -1;
        }

        /// <summary>
        /// 헤더 이름을 열 번호로 매핑
        /// </summary>
        /// <param name="sheet">시트</param>
        /// <returns>헤더 이름과 열 번호 매핑</returns>
        private Dictionary<string, int> CreateHeaderMap(IXLWorksheet sheet)
        {
            var fieldMap = new Dictionary<string, int>();
            var lastColumn = sheet.LastColumnUsed();
            if (lastColumn == null) return fieldMap; // 빈 시트인 경우 빈 맵 반환
            int colCount = lastColumn.ColumnNumber();

            for (int col = 1; col <= colCount; col++)
            {
                string header = sheet.Cell(1, col).GetString();

                // 타입 정보 제거 (헤더 이름이 "(타입)" 형태로 표시된 경우)
                int bracketIndex = header.IndexOf(" (");
                if (bracketIndex > 0)
                {
                    header = header.Substring(0, bracketIndex);
                }

                // 중복이거나 빈 헤더가 아닌 경우에만 추가
                if (!string.IsNullOrEmpty(header))
                {
                    fieldMap[header] = col;
                }
            }

            return fieldMap;
        }

        /// <summary>
        /// 엑셀 데이터를 스크립터블 오브젝트로 가져오는 메서드
        /// </summary>
        /// <returns>성공 여부</returns>
        private bool ImportExcelToScriptableObject()
        {
            try
            {
                // 현재 스크립터블 오브젝트와 경로를, Excel Importer를 이용하여 동기화
                var importer = new ExcelImporter<T>(_scriptableObject, _excelFilePath, _showLogs);
                bool success = importer.Import();

                if (success)
                {
                    UnityEngine.Debug.Log($"[ExcelSync] 스크립터블 오브젝트를 엑셀 데이터로 업데이트했습니다: {_excelFilePath}");

                    // 변경 사항 저장
                    EditorUtility.SetDirty(_scriptableObject);
                    AssetDatabase.SaveAssets();
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogError($"[ExcelSync] 엑셀에서 스크립터블 오브젝트로 가져오기 실패: {_excelFilePath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"엑셀에서 스크립터블 오브젝트로 가져오기 중 오류: {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}