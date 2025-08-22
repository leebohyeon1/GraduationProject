using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Linq;
using BH_Lib.DI;
using BH_Lib.ExcelConverter.Settings;
using BH_Lib.ExcelConverter.Convert;

namespace BH_Lib.ExcelConverter.Utility
{
    /// <summary>
    /// 다양한 ScriptableObject 타입과 엑셀 파일 간의 동기화 기능을 제공하는 유틸리티 클래스
    /// </summary>
    [Register(typeof(ISyncUtility))]
    public class SyncUtility : ISyncUtility
    {
        #region Private Variables
        [Inject]
        private readonly ISettingsProvider _settingsProvider;

        // 동기화 처리 중인지 확인하는 상태
        private bool _isSyncProcessing = false;
        #endregion

        #region Properties
        /// <summary>
        /// 동기화 처리 중인지 확인하는 프로퍼티
        /// </summary>
        public bool IsSyncProcessing => _isSyncProcessing;
        #endregion

        #region Constructor
        /// <summary>
        /// SyncUtility 생성자
        /// </summary>
        public SyncUtility()
        {
            // DI 컨테이너를 통한 의존성 주입
            DIContainer.Instance.InjectInto(this);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 특정 동기화 항목의 ScriptableObject에서 엑셀 파일로 데이터를 내보내는 메소드
        /// </summary>
        /// <typeparam name="T">ScriptableObject 타입</typeparam>
        /// <param name="syncItem">동기화 설정 항목</param>
        /// <param name="scriptableObject">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        public bool ExportSOToExcel<T>(SyncSettingItem syncItem, T scriptableObject) where T : ScriptableObject
        {
            if (_settingsProvider == null || syncItem == null || scriptableObject == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] 동기화 설정 또는 ScriptableObject가 없습니다.");
                return false;
            }

            string excelFilePath = _settingsProvider.GetFullExcelPath(syncItem);

            try
            {
                _isSyncProcessing = true;

                // 디렉토리 확인 및 생성
                string directory = Path.GetDirectoryName(excelFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // ExcelExporter를 사용하여 데이터를 내보내기
                var exporter = new ExcelExporter<T>(scriptableObject, excelFilePath, _settingsProvider.ShowLogs);
                bool success = exporter.Export();

                if (success)
                {
                    if (_settingsProvider.ShowLogs)
                    {
                        UnityEngine.Debug.Log($"[ExcelSync] {syncItem.Name} 데이터를 성공적으로 엑셀로 내보냈습니다.");
                    }

                    // 에셋 데이터베이스 갱신
                    AssetDatabase.Refresh();
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogError($"[ExcelSync] {syncItem.Name} 데이터 내보내기에 실패했습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] {syncItem.Name} 데이터 내보내기 중 오류 발생: {ex.Message}");
                return false;
            }
            finally
            {
                _isSyncProcessing = false;
            }
        }

        /// <summary>
        /// 특정 동기화 항목의 엑셀 파일에서 ScriptableObject로 데이터를 가져오는 메소드
        /// </summary>
        /// <typeparam name="T">ScriptableObject 타입</typeparam>
        /// <param name="syncItem">동기화 설정 항목</param>
        /// <param name="scriptableObject">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        public bool ImportExcelToSO<T>(SyncSettingItem syncItem, T scriptableObject) where T : ScriptableObject
        {
            if (_settingsProvider == null || syncItem == null || scriptableObject == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] 동기화 설정 또는 ScriptableObject가 없습니다.");
                return false;
            }

            string excelFilePath = _settingsProvider.GetFullExcelPath(syncItem);

            // 파일 존재 확인
            if (!File.Exists(excelFilePath))
            {
                UnityEngine.Debug.LogError($"[ExcelSync] 엑셀 파일을 찾을 수 없습니다: {excelFilePath}");
                return false;
            }

            try
            {
                _isSyncProcessing = true;

                // ExcelImporter를 사용하여 데이터를 가져오기
                var importer = new ExcelImporter<T>(scriptableObject, excelFilePath, _settingsProvider.ShowLogs);
                bool success = importer.Import();

                if (success)
                {
                    if (_settingsProvider.ShowLogs)
                    {
                        UnityEngine.Debug.Log($"[ExcelSync] {syncItem.Name} 엑셀 데이터를 성공적으로 가져왔습니다.");
                    }

                    // 변경 사항 저장
                    EditorUtility.SetDirty(scriptableObject);
                    AssetDatabase.SaveAssets();
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogError($"[ExcelSync] {syncItem.Name} 엑셀 데이터 가져오기에 실패했습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] {syncItem.Name} 엑셀 데이터 가져오기 중 오류 발생: {ex.Message}");
                return false;
            }
            finally
            {
                _isSyncProcessing = false;
            }
        }

        /// <summary>
        /// 동기화 항목 등록된 모든 ScriptableObject를 엑셀로 내보내기
        /// </summary>
        public void ExportAllSOToExcel()
        {
            if (_settingsProvider == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] 동기화 설정을 찾을 수 없습니다.");
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var item in _settingsProvider.SyncItems)
            {
                string assetPath = _settingsProvider.GetFullAssetPath(item);
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                if (so != null)
                {
                    // 제네릭으로 내보내기 메소드 호출
                    bool success = ExportSOToExcelDynamic(item, so);

                    if (success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"[ExcelSync] ScriptableObject를 찾을 수 없습니다: {assetPath}");
                    failCount++;
                }
            }

            EditorUtility.DisplayDialog("내보내기 완료",
                $"총 {_settingsProvider.SyncItems.Count}개 항목 중 {successCount}개 성공, {failCount}개 실패",
                "확인");
        }

        /// <summary>
        /// 동기화 항목 등록된 모든 엑셀을 ScriptableObject로 가져오기
        /// </summary>
        public void ImportAllExcelToSO()
        {
            if (_settingsProvider == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] 동기화 설정을 찾을 수 없습니다.");
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var item in _settingsProvider.SyncItems)
            {
                string assetPath = _settingsProvider.GetFullAssetPath(item);
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                if (so != null)
                {
                    // 제네릭으로 가져오기 메소드 호출
                    bool success = ImportExcelToSODynamic(item, so);

                    if (success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"[ExcelSync] ScriptableObject를 찾을 수 없습니다: {assetPath}");
                    failCount++;
                }
            }

            EditorUtility.DisplayDialog("가져오기 완료",
                $"총 {_settingsProvider.SyncItems.Count}개 항목 중 {successCount}개 성공, {failCount}개 실패",
                "확인");
        }

        /// <summary>
        /// 특정 이름의 동기화 항목 찾아서 ScriptableObject를 엑셀로 동기화
        /// </summary>
        /// <param name="syncItemName">동기화 항목 이름</param>
        /// <returns>성공 여부</returns>
        public bool ExportSOToExcel(string syncItemName)
        {
            if (_settingsProvider == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] 동기화 설정을 찾을 수 없습니다.");
                return false;
            }

            var syncItem = _settingsProvider.GetSyncItem(syncItemName);
            if (syncItem == null)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] 해당 이름의 동기화 항목을 찾을 수 없습니다: {syncItemName}");
                return false;
            }

            string assetPath = _settingsProvider.GetFullAssetPath(syncItem);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            if (so != null)
            {
                return ExportSOToExcelDynamic(syncItem, so);
            }
            else
            {
                UnityEngine.Debug.LogError($"[ExcelSync] ScriptableObject를 찾을 수 없습니다: {assetPath}");
                return false;
            }
        }

        /// <summary>
        /// 특정 이름의 동기화 항목 찾아서 엑셀을 ScriptableObject로 동기화
        /// </summary>
        /// <param name="syncItemName">동기화 항목 이름</param>
        /// <returns>성공 여부</returns>
        public bool ImportExcelToSO(string syncItemName)
        {
            if (_settingsProvider == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] 동기화 설정을 찾을 수 없습니다.");
                return false;
            }

            var syncItem = _settingsProvider.GetSyncItem(syncItemName);
            if (syncItem == null)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] 해당 이름의 동기화 항목을 찾을 수 없습니다: {syncItemName}");
                return false;
            }

            string assetPath = _settingsProvider.GetFullAssetPath(syncItem);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            if (so != null)
            {
                return ImportExcelToSODynamic(syncItem, so);
            }
            else
            {
                UnityEngine.Debug.LogError($"[ExcelSync] ScriptableObject를 찾을 수 없습니다: {assetPath}");
                return false;
            }
        }

        /// <summary>
        /// ScriptableObject 직접 전달하여 엑셀로 내보내기
        /// </summary>
        /// <param name="so">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        public bool ExportSOToExcel(ScriptableObject so)
        {
            if (so == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] ScriptableObject가 null입니다.");
                return false;
            }

            if (_settingsProvider == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] 동기화 설정을 찾을 수 없습니다.");
                return false;
            }

            // ScriptableObject의 경로 찾기
            string assetPath = AssetDatabase.GetAssetPath(so);
            if (string.IsNullOrEmpty(assetPath))
            {
                UnityEngine.Debug.LogError("[ExcelSync] ScriptableObject가 프로젝트에 저장되지 않았습니다.");
                return false;
            }

            // 동기화 항목 찾기
            var syncItem = _settingsProvider.GetSyncItemByAssetPath(assetPath);
            if (syncItem == null)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] 해당 경로의 동기화 항목을 찾을 수 없습니다: {assetPath}");
                return false;
            }

            return ExportSOToExcelDynamic(syncItem, so);
        }

        /// <summary>
        /// ScriptableObject 직접 전달하여 엑셀에서 가져오기
        /// </summary>
        /// <param name="so">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        public bool ImportExcelToSO(ScriptableObject so)
        {
            if (so == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] ScriptableObject가 null입니다.");
                return false;
            }

            if (_settingsProvider == null)
            {
                UnityEngine.Debug.LogError("[ExcelSync] 동기화 설정을 찾을 수 없습니다.");
                return false;
            }

            // ScriptableObject의 경로 찾기
            string assetPath = AssetDatabase.GetAssetPath(so);
            if (string.IsNullOrEmpty(assetPath))
            {
                UnityEngine.Debug.LogError("[ExcelSync] ScriptableObject가 프로젝트에 저장되지 않았습니다.");
                return false;
            }

            // 동기화 항목 찾기
            var syncItem = _settingsProvider.GetSyncItemByAssetPath(assetPath);
            if (syncItem == null)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] 해당 경로의 동기화 항목을 찾을 수 없습니다: {assetPath}");
                return false;
            }

            return ImportExcelToSODynamic(syncItem, so);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 런타임에 타입을 알 수 없는 ScriptableObject를 엑셀로 내보내는 헬퍼 메소드
        /// </summary>
        /// <param name="syncItem">동기화 설정 항목</param>
        /// <param name="so">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        private bool ExportSOToExcelDynamic(SyncSettingItem syncItem, ScriptableObject so)
        {
            Type soType = so.GetType();
            try
            {
                // 1) 이름이 맞고, 제네릭 정의 메소드이면서 파라미터가 (SyncSettingItem, T)인 메소드 하나를 얻는다
                var genericDef = typeof(SyncUtility)
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m =>
                        m.Name == nameof(ExportSOToExcel) &&
                        m.IsGenericMethodDefinition &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[0].ParameterType == typeof(SyncSettingItem)
                    );

                if (genericDef == null)
                {
                    UnityEngine.Debug.LogError("[ExcelSync] 제네릭 ExportSOToExcel 메소드를 찾을 수 없습니다.");
                    return false;
                }

                // 2) 실제 타입으로 특화
                MethodInfo concreteMethod = genericDef.MakeGenericMethod(soType);

                // 3) 호출
                return (bool)concreteMethod.Invoke(this, new object[] { syncItem, so });
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] ExportSOToExcel 호출 오류: {ex.Message}");
                if (ex.InnerException != null)
                    UnityEngine.Debug.LogError($"내부 오류: {ex.InnerException.Message}");
                return false;
            }
        }

        /// <summary>
        /// 런타임에 타입을 알 수 없는 ScriptableObject로 엑셀을 가져오는 헬퍼 메소드
        /// </summary>
        /// <param name="syncItem">동기화 설정 항목</param>
        /// <param name="so">ScriptableObject 인스턴스</param>
        /// <returns>성공 여부</returns>
        private bool ImportExcelToSODynamic(SyncSettingItem syncItem, ScriptableObject so)
        {
            Type soType = so.GetType();
            try
            {
                // 1) 이름이 맞고, 제네릭 정의 메소드이면서 파라미터가 (SyncSettingItem, T)인 메소드 하나를 얻는다
                var genericDef = typeof(SyncUtility)
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m =>
                        m.Name == nameof(ImportExcelToSO) &&
                        m.IsGenericMethodDefinition &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[0].ParameterType == typeof(SyncSettingItem)
                    );

                if (genericDef == null)
                {
                    UnityEngine.Debug.LogError("[ExcelSync] 제네릭 ImportExcelToSO 메소드를 찾을 수 없습니다.");
                    return false;
                }

                // 2) 실제 타입으로 특화
                MethodInfo concreteMethod = genericDef.MakeGenericMethod(soType);

                // 3) 호출
                return (bool)concreteMethod.Invoke(this, new object[] { syncItem, so });
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] ImportExcelToSO 메소드 호출 오류: {ex.Message}");
                if (ex.InnerException != null)
                    UnityEngine.Debug.LogError($"내부 오류: {ex.InnerException.Message}");
                return false;
            }
        }
        #endregion
    }
}