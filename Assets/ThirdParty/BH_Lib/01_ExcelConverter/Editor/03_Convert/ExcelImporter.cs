using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ClosedXML.Excel;
using UnityEngine;
using UnityEditor;
using System.Linq;
using BH_Lib.DI;
using BH_Lib.ExcelConverter.Core;

namespace BH_Lib.ExcelConverter.Convert
{
    /// <summary>
    /// 제네릭 타입의 ScriptableObject를 위한 엑셀 임포터
    /// </summary>
    /// <typeparam name="T">ScriptableObject 타입</typeparam>
    [Register(typeof(IExcelImporter<>), LifetimeScope.Transient)]
    public class ExcelImporter<T> : IExcelImporter<T> where T : ScriptableObject
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
        /// ExcelImporter 생성
        /// </summary>
        /// <param name="scriptableObject">대상 ScriptableObject</param>
        /// <param name="excelFilePath">엑셀 파일 경로</param>
        /// <param name="showLogs">로그 출력 여부</param>
        public ExcelImporter(T scriptableObject, string excelFilePath, bool showLogs = false)
        {
            _scriptableObject = scriptableObject;
            _excelFilePath = excelFilePath;
            _showLogs = showLogs;

            // DI 컨테이너를 통한 의존성 주입
            DIContainer.Instance.InjectInto(this);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 엑셀 파일에서 데이터를 가져와 ScriptableObject에 적용
        /// </summary>
        /// <returns>성공 여부</returns>
        public bool Import()
        {
            UnityEngine.Debug.Log($"[ExcelSync] Import 시작 - SO 타입: {typeof(T).Name}");
            if (_scriptableObject == null)
            {
                UnityEngine.Debug.LogError($"{typeof(T).Name}이 할당되지 않았습니다!");
                return false;
            }

            if (!File.Exists(_excelFilePath))
            {
                UnityEngine.Debug.LogError($"파일을 찾을 수 없습니다. 경로: {_excelFilePath}");
                return false;
            }

            try
            {
                // 파일이 열려있어 읽기만 하는 경우 대체 방안 사용
                XLWorkbook workbook = null;
                try
                {
                    // 기본 방식으로 시도
                    workbook = new XLWorkbook(_excelFilePath);
                }
                catch (IOException)
                {
                    // 파일이 사용중인 경우 파일 스트림으로 대체 시도
                    try
                    {
                        using (FileStream fs = new FileStream(_excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            // 파일을 바이트로 메모리에 로드
                            byte[] bytes = new byte[fs.Length];
                            fs.Read(bytes, 0, (int)fs.Length);

                            // 메모리스트림으로 변환
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                workbook = new XLWorkbook(ms);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"파일 열기에 실패 하였습니다. 파일이 다른 프로그램에서 열려 있을 수 있습니다: {ex.Message}");
                        EditorUtility.DisplayDialog("파일 읽기 실패",
                            $"엑셀 파일이 다른 프로그램에서 열려 있어 읽기 전용으로 접근하지 못했습니다.\n파일을 닫고 다시 시도해보세요.\n\n오류: {ex.Message}",
                            "확인");
                        return false;
                    }
                }

                if (workbook != null)
                {
                    using (workbook)
                    {
                        // Excel 파일의 시트 목록 출력
                        UnityEngine.Debug.Log($"[ExcelSync] Excel 파일의 시트 목록:");
                        foreach (var worksheet in workbook.Worksheets)
                        {
                            UnityEngine.Debug.Log($"  - {worksheet.Name}");
                        }
                        
                        // ScriptableObject 클래스 객체의 기본 시트 처리
                        string soSheetName = typeof(T).Name;
                        if (workbook.TryGetWorksheet(soSheetName, out var soSheet))
                        {
                            if (_showLogs) UnityEngine.Debug.Log($"{soSheetName} 시트에서 기본 타입 데이터 가져오는 중");
                            ImportPrimitiveTypeData(soSheet);
                        }

                        // ScriptableObject의 모든 필드를 리플렉션으로 찾아서 시트 가져오기
                        var soFields = GetAllSerializableFields(typeof(T));
                        UnityEngine.Debug.Log($"[ExcelSync] 찾은 필드 수: {soFields.Count}");

                        foreach (var field in soFields)
                        {
                            UnityEngine.Debug.Log($"[ExcelSync] 필드 처리 중: {field.Name} (타입: {field.FieldType})");
                            var ft = field.FieldType;
                            // 1) 배열 타입인 경우 - List로 읽어서 Array로 변환 
                            if (ft.IsArray)
                            {
                                Type elementType = ft.GetElementType();
                                string sheetName = elementType.Name;
                                if (workbook.TryGetWorksheet(sheetName, out var sheet))
                                {
                                    if (_showLogs) UnityEngine.Debug.Log($"{sheetName}[] 배열 데이터 가져오는 중");

                                    // List<T>로 임시로 읽기
                                    var listType = typeof(List<>).MakeGenericType(elementType);
                                    var tempList = Activator.CreateInstance(listType) as IList;
                                    ImportListData(field, elementType, sheet, tempList);

                                    // Array로 변환해서 할당
                                    var array = Array.CreateInstance(elementType, tempList.Count);
                                    tempList.CopyTo(array, 0);
                                    field.SetValue(_scriptableObject, array);
                                }
                                continue;
                            }
                            // 2) List<T>인 경우
                            if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(List<>))
                            {
                                Type elementType = ft.GetGenericArguments()[0];
                                if (elementType.IsClass && elementType != typeof(string))
                                {
                                    string sheetName = elementType.Name;
                                    if (workbook.TryGetWorksheet(sheetName, out var sheet))
                                    {
                                        if (_showLogs) UnityEngine.Debug.Log($"List<{sheetName}> 리스트 데이터 가져오는 중");
                                        UnityEngine.Debug.Log($"[ExcelSync] 시트 '{sheetName}' 찾음. 필드: {field.Name}, 타입: {field.FieldType}");
                                        ImportListData(field, elementType, sheet);
                                    }
                                }
                                continue;
                            }
                            // 3) 일반 클래스 타입 필드 처리
                            if (ft.IsClass && !IsPrimitiveOrString(ft))
                            {
                                string sheetName = ft.Name;
                                if (workbook.TryGetWorksheet(sheetName, out var sheet))
                                {
                                    if (_showLogs) UnityEngine.Debug.Log($"{sheetName} 데이터 가져오는 중");
                                    ImportClassData(field, ft, sheet);
                                }
                            }
                        }

                        if (_showLogs)
                        {
                            UnityEngine.Debug.Log("엑셀 데이터 가져오기 완료");
                        }

                        // 데이터 확인 로그
                        UnityEngine.Debug.Log($"[ExcelSync] Import 완료 - SO 필드 확인:");
                        foreach (var field in soFields)
                        {
                            var value = field.GetValue(_scriptableObject);
                            if (value is IList list)
                            {
                                UnityEngine.Debug.Log($"  - {field.Name}: {list?.Count ?? 0}개 항목");
                            }
                            else
                            {
                                UnityEngine.Debug.Log($"  - {field.Name}: {value?.ToString() ?? "null"}");
                            }
                        }

                        EditorUtility.SetDirty(_scriptableObject);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"엑셀 파일 가져오기 중 오류 발생: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"엑셀 파일 가져오기 중 오류가 발생했습니다:\n{e.Message}", "확인");
                return false;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// ScriptableObject 시트에서 기본 타입 필드 데이터 가져오기
        /// </summary>
        /// <param name="sheet">엑셀 워크시트</param>
        private void ImportPrimitiveTypeData(IXLWorksheet sheet)
        {
            // 행이 2개 이상인 경우만 처리 (헤더 + 최소 1개의 데이터 행)
            if (sheet.RowsUsed().Count() < 2) return;

            // 헤더 이름과 열 번호 매핑
            var fieldMap = CreateHeaderMap(sheet);

            // 필드 값 적용 (public 및 SerializeField 필드 모두)
            var fields = GetAllSerializableFields(typeof(T));
            foreach (var field in fields)
            {
                // 필드 타입 확인
                var ft = field.FieldType;

                // 기본 타입이나 문자열, 배열, 값 타입 리스트인 경우 처리
                bool isValueTypeList = ft.IsGenericType &&
                                      ft.GetGenericTypeDefinition() == typeof(List<>) &&
                                      (ft.GetGenericArguments()[0].IsValueType ||
                                       ft.GetGenericArguments()[0] == typeof(string));

                if ((IsPrimitiveOrString(ft) || ft.IsArray || isValueTypeList) && fieldMap.ContainsKey(field.Name))
                {
                    int col = fieldMap[field.Name];
                    string cellValue = sheet.Cell(2, col).GetString();

                    // 타입별 변환 처리 수행
                    object convertedValue = _dataConverter.ConvertToType(cellValue, ft);
                    if (convertedValue != null)
                    {
                        field.SetValue(_scriptableObject, convertedValue);
                    }
                }
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
        /// 기본 타입 필드 데이터를 객체에 적용
        /// </summary>
        /// <param name="target">대상 객체</param>
        /// <param name="fieldData">필드 데이터</param>
        private void ApplyPrimitiveFieldsData(object target, Dictionary<string, object> fieldData)
        {
            foreach (var entry in fieldData)
            {
                string fieldPath = entry.Key;
                string fieldValue = entry.Value.ToString();

                // 필드 경로 분석 (중첩된 필드도 지원)
                string[] pathParts = fieldPath.Split('.');

                // 경로가 1단계인 경우 (직접 필드)
                if (pathParts.Length == 1)
                {
                    SetFieldValue(target, pathParts[0], fieldValue);
                }
                // 경로가 여러 단계인 경우
                else if (pathParts.Length > 1)
                {
                    // 첫 번째 필드 찾기
                    string firstField = pathParts[0];
                    FieldInfo field = target.GetType().GetField(firstField, BindingFlags.Public | BindingFlags.Instance);

                    if (field != null)
                    {
                        // 필드 값 가져오기
                        object fieldObj = field.GetValue(target);

                        // 필드 값이 null이면 새 인스턴스 생성
                        if (fieldObj == null)
                        {
                            fieldObj = Activator.CreateInstance(field.FieldType);
                            field.SetValue(target, fieldObj);
                        }

                        // 나머지 경로 처리
                        string remainingPath = string.Join(".", pathParts, 1, pathParts.Length - 1);

                        // 재귀적으로 하위 필드 처리
                        var nestedFieldData = new Dictionary<string, object>
                        {
                            { remainingPath, fieldValue }
                        };

                        ApplyPrimitiveFieldsData(fieldObj, nestedFieldData);
                    }
                }
            }
        }

        /// <summary>
        /// 객체의 필드 값 설정
        /// </summary>
        /// <param name="target">대상 객체</param>
        /// <param name="fieldName">필드 이름</param>
        /// <param name="value">설정할 값</param>
        private void SetFieldValue(object target, string fieldName, string value)
        {
            try
            {
                // 필드 찾기
                var field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    // 값 변환 및 설정
                    object convertedValue = _dataConverter.ConvertToType(value, field.FieldType);
                    field.SetValue(target, convertedValue);
                    return;
                }

                // 프로퍼티 찾기
                var property = target.GetType().GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    // 값 변환 및 설정
                    object convertedValue = _dataConverter.ConvertToType(value, property.PropertyType);
                    property.SetValue(target, convertedValue);
                    return;
                }

                if (_showLogs) UnityEngine.Debug.LogWarning($"필드나 프로퍼티를 찾을 수 없음: {fieldName}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"필드 값 설정 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 일반 클래스 데이터를 가져오는 메서드
        /// </summary>
        /// <param name="fieldInfo">필드 정보</param>
        /// <param name="classType">클래스 타입</param>
        /// <param name="sheet">엑셀 워크시트</param>
        private void ImportClassData(FieldInfo fieldInfo, Type classType, IXLWorksheet sheet)
        {
            // 대상 필드가 null인 경우 새 인스턴스 생성
            object fieldValue = fieldInfo.GetValue(_scriptableObject);
            if (fieldValue == null)
            {
                fieldValue = Activator.CreateInstance(classType);
                fieldInfo.SetValue(_scriptableObject, fieldValue);
            }

            // 행이 2개 이상인 경우만 처리 (헤더 + 최소 1개의 데이터 행)
            if (sheet.RowsUsed().Count() < 2) return;

            // 필드 헤더 및 위치 매핑
            var fieldMap = CreateHeaderMap(sheet);

            // 필드 값 적용
            var fields = classType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (fieldMap.ContainsKey(field.Name))
                {
                    int col = fieldMap[field.Name];
                    string cellValue = sheet.Cell(2, col).GetString();

                    // 타입별 변환 처리 수행
                    object convertedValue = _dataConverter.ConvertToType(cellValue, field.FieldType);
                    if (convertedValue != null)
                    {
                        field.SetValue(fieldValue, convertedValue);
                    }
                }
            }

            // 프로퍼티 값 적용
            var properties = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            foreach (var property in properties)
            {
                if (property.CanWrite && fieldMap.ContainsKey(property.Name))
                {
                    int col = fieldMap[property.Name];
                    string cellValue = sheet.Cell(2, col).GetString();

                    // 타입별 변환 처리 수행
                    object convertedValue = _dataConverter.ConvertToType(cellValue, property.PropertyType);
                    if (convertedValue != null)
                    {
                        property.SetValue(fieldValue, convertedValue);
                    }
                }
            }
        }

        /// <summary>
        /// List<T> 기본 로드
        /// </summary>
        /// <param name="fieldInfo">필드 정보</param>
        /// <param name="elementType">요소 타입</param>
        /// <param name="sheet">엑셀 워크시트</param>
        private void ImportListData(FieldInfo fieldInfo, Type elementType, IXLWorksheet sheet)
        {
            UnityEngine.Debug.Log($"[ExcelSync] ImportListData 호출됨 - 필드: {fieldInfo.Name}, 요소 타입: {elementType.Name}");
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = Activator.CreateInstance(listType) as IList;
            fieldInfo.SetValue(_scriptableObject, list);
            ImportListData(fieldInfo, elementType, sheet, list);
        }

        /// <summary>
        /// List<T> 로드 오버로드 (Array 지원용)
        /// </summary>
        /// <param name="fieldInfo">필드 정보</param>
        /// <param name="elementType">요소 타입</param>
        /// <param name="sheet">엑셀 워크시트</param>
        /// <param name="list">리스트 인스턴스</param>
        private void ImportListData(FieldInfo fieldInfo, Type elementType, IXLWorksheet sheet, IList list)
        {
            UnityEngine.Debug.Log($"[ExcelSync] ImportListData 데이터 읽기 시작 - 시트: {sheet.Name}");
            if (sheet.RowsUsed().Count() < 2) 
            {
                UnityEngine.Debug.LogWarning($"[ExcelSync] 시트 '{sheet.Name}'에 데이터가 없음 (행 수: {sheet.RowsUsed().Count()})");
                return;
            }
            var fieldMap = CreateHeaderMap(sheet);
            UnityEngine.Debug.Log($"[ExcelSync] 헤더 맵 생성됨: {string.Join(", ", fieldMap.Keys)}");
            
            // 헤더 맵 상세 출력
            foreach (var header in fieldMap)
            {
                UnityEngine.Debug.Log($"[ExcelSync] 헤더: {header.Key} -> 열 {header.Value}");
            }
            int lastCol = sheet.LastColumnUsed().ColumnNumber();
            int lastRow = sheet.LastRowUsed().RowNumber();
            UnityEngine.Debug.Log($"[ExcelSync] 데이터 범위 - 열: 1-{lastCol}, 행: 2-{lastRow}");

            for (int row = 2; row <= lastRow; row++)
            {
                bool empty = true;
                for (int c = 1; c <= lastCol; c++)
                    if (!string.IsNullOrWhiteSpace(sheet.Cell(row, c).GetString())) { empty = false; break; }
                if (empty) continue;

                var item = Activator.CreateInstance(elementType);
                UnityEngine.Debug.Log($"[ExcelSync] 행 {row} 데이터 읽기 중...");
                SetObjectValues(item, fieldMap, sheet, row);
                list.Add(item);
                UnityEngine.Debug.Log($"[ExcelSync] 아이템 추가됨. 현재 리스트 크기: {list.Count}");
            }

            UnityEngine.Debug.Log($"[ExcelSync] {sheet.Name} 시트에서 총 {list.Count}개 항목을 읽음");
        }

        /// <summary>
        /// 헤더 이름과 열 번호를 매핑 생성
        /// </summary>
        /// <param name="sheet">엑셀 워크시트</param>
        /// <returns>헤더 매핑</returns>
        private Dictionary<string, int> CreateHeaderMap(IXLWorksheet sheet)
        {
            var fieldMap = new Dictionary<string, int>();
            int colCount = sheet.LastColumnUsed().ColumnNumber();

            for (int col = 1; col <= colCount; col++)
            {
                string header = sheet.Cell(1, col).GetString();

                // 타입 정보 제거 (헤더 이름이 "(타입)" 형태로 표시된 경우)
                int bracketIndex = header.IndexOf(" (");
                if (bracketIndex > 0)
                {
                    header = header.Substring(0, bracketIndex);
                }

                // 중복되지 않은 헤더만 추가
                if (!string.IsNullOrEmpty(header))
                {
                    fieldMap[header] = col;
                }
            }

            return fieldMap;
        }

        /// <summary>
        /// 객체의 필드 및 프로퍼티에 값 설정
        /// </summary>
        /// <param name="targetObject">대상 객체</param>
        /// <param name="fieldMap">필드 매핑</param>
        /// <param name="sheet">엑셀 워크시트</param>
        /// <param name="row">행 번호</param>
        private void SetObjectValues(object targetObject, Dictionary<string, int> fieldMap, IXLWorksheet sheet, int row)
        {
            Type objectType = targetObject.GetType();
            UnityEngine.Debug.Log($"[ExcelSync] SetObjectValues - 타입: {objectType.Name}, 행: {row}");

            // 직렬화 가능한 모든 필드 가져오기 (public 및 SerializeField)
            var fields = GetAllSerializableFields(objectType);
            UnityEngine.Debug.Log($"[ExcelSync] 찾은 필드 수: {fields.Count}");
            
            foreach (var field in fields)
            {
                UnityEngine.Debug.Log($"[ExcelSync] 필드 처리 중: {field.Name} (타입: {field.FieldType})");
                bool matched = false;
                
                // 필드명으로 직접 찾기
                if (fieldMap.ContainsKey(field.Name))
                {
                    int col = fieldMap[field.Name];
                    string cellValue = sheet.Cell(row, col).GetString();
                    UnityEngine.Debug.Log($"[ExcelSync] 필드명 매칭 - {field.Name}: {cellValue}");

                    // 타입별 변환 처리 수행
                    object convertedValue = _dataConverter.ConvertToType(cellValue, field.FieldType);
                    if (convertedValue != null)
                    {
                        field.SetValue(targetObject, convertedValue);
                        matched = true;
                    }
                }
                // 언더스코어를 제거한 이름으로 찾기 (_characterId -> CharacterId)
                else if (field.Name.StartsWith("_") && field.Name.Length > 1)
                {
                    string nameWithoutUnderscore = char.ToUpper(field.Name[1]) + field.Name.Substring(2);
                    UnityEngine.Debug.Log($"[ExcelSync] 언더스코어 변환 시도: {field.Name} -> {nameWithoutUnderscore}");
                    
                    if (fieldMap.ContainsKey(nameWithoutUnderscore))
                    {
                        int col = fieldMap[nameWithoutUnderscore];
                        string cellValue = sheet.Cell(row, col).GetString();
                        UnityEngine.Debug.Log($"[ExcelSync] 언더스코어 제거 매칭 성공 - {field.Name} -> {nameWithoutUnderscore}: {cellValue}");

                        // 타입별 변환 처리 수행
                        object convertedValue = _dataConverter.ConvertToType(cellValue, field.FieldType);
                        if (convertedValue != null)
                        {
                            field.SetValue(targetObject, convertedValue);
                            matched = true;
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[ExcelSync] 헤더에서 {nameWithoutUnderscore}를 찾을 수 없음");
                    }
                }
                
                if (!matched)
                {
                    UnityEngine.Debug.LogWarning($"[ExcelSync] 필드 {field.Name}에 대한 매칭을 찾을 수 없음");
                }
            }

            // 프로퍼티 값 적용
            var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            foreach (var property in properties)
            {
                if (property.CanWrite && fieldMap.ContainsKey(property.Name))
                {
                    int col = fieldMap[property.Name];
                    string cellValue = sheet.Cell(row, col).GetString();
                    UnityEngine.Debug.Log($"[ExcelSync] 프로퍼티 매칭 - {property.Name}: {cellValue}");

                    // 타입별 변환 처리 수행
                    object convertedValue = _dataConverter.ConvertToType(cellValue, property.PropertyType);
                    if (convertedValue != null)
                    {
                        property.SetValue(targetObject, convertedValue);
                    }
                }
            }
        }
        #endregion
    }
}