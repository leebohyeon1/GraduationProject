using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using BH_Lib.DI;
using BH_Lib.ExcelConverter.Settings;
using BH_Lib.ExcelConverter.Utility;

namespace BH_Lib.ExcelConverter.Editor
{
    /// <summary>
    /// 에셋 변경 후처리기 - 파일 변경 감지 및 자동 동기화 처리
    /// </summary>
    public class ExcelSyncPostprocessor : AssetPostprocessor
    {
        // 동기화 중복 실행 방지 플래그
        private static bool _isProcessingSync = false;

        // 마지막 동기화 타임스탬프
        private static double _lastSyncTime = 0;

        // 동기화 쿨다운 시간 (초)
        private static readonly double _syncCooldown = 1.0;

        // 현재 동기화 중인 에셋 추적
        private static readonly HashSet<string> _syncingAssets = new HashSet<string>();

        /// <summary>
        /// 에셋 후처리 이벤트 핸들러
        /// </summary>
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            try
            {
                // 도메인 리로드 후에는 추가 처리가 필요하지 않음
                if (didDomainReload)
                    return;

                var settings = ExcelSyncSettingsSO.Instance;
                if (settings == null || !settings.GlobalAutoSyncEnabled || _isProcessingSync)
                    return;

                // 마지막 동기화로부터 쿨다운 시간 확인
                double currentTime = EditorApplication.timeSinceStartup;
                if ((currentTime - _lastSyncTime) < _syncCooldown)
                {
                    if (settings.ShowLogs)
                        UnityEngine.Debug.Log($"[ExcelSync] 동기화 쿨다운 중... ({_syncCooldown - (currentTime - _lastSyncTime):F1}초 남음)");
                    return;
                }

                _isProcessingSync = true;
                _lastSyncTime = currentTime;

                // 동기화 유틸리티 가져오기
                var syncUtility = DIContainer.Instance.Resolve<ISyncUtility>();
                if (syncUtility == null)
                {
                    UnityEngine.Debug.LogError("[ExcelSync] 동기화 유틸리티를 찾을 수 없습니다.");
                    _isProcessingSync = false;
                    return;
                }

                // ScriptableObject 변경 처리 
                ProcessChangedScriptableObjects(importedAssets, settings, syncUtility);

                // Excel 파일 변경 처리
                ProcessChangedExcelFiles(importedAssets, movedAssets, movedFromAssetPaths, settings, syncUtility);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ExcelSync] 후처리 중 오류 발생: {ex.Message}");
            }
            finally
            {
                _isProcessingSync = false;
                _syncingAssets.Clear();
            }
        }

        /// <summary>
        /// ScriptableObject 변경 감지 및 처리
        /// </summary>
        /// <param name="importedAssets">가져온 에셋 경로 배열</param>
        /// <param name="settings">동기화 설정</param>
        /// <param name="syncUtility">동기화 유틸리티</param>
        private static void ProcessChangedScriptableObjects(
            string[] importedAssets,
            ISettingsProvider settings,
            ISyncUtility syncUtility)
        {
            foreach (string assetPath in importedAssets)
            {
                // 이미 동기화 중인 에셋은 건너뜀
                if (_syncingAssets.Contains(assetPath))
                    continue;

                // ScriptableObject 에셋인지 확인
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                if (so == null) continue;

                // 동기화 항목에 등록된 ScriptableObject인지 확인
                var syncItem = settings.GetSyncItemByAssetPath(assetPath);
                if (syncItem == null || !syncItem.EnableSOToExcel)
                    continue;

                // Excel로 내보내기
                if (settings.ShowLogs)
                    UnityEngine.Debug.Log($"[ExcelSync] ScriptableObject 변경 감지: {syncItem.Name}");

                _syncingAssets.Add(assetPath);
                syncUtility.ExportSOToExcel(so);
            }
        }

        /// <summary>
        /// Excel 파일 변경 감지 및 처리
        /// </summary>
        /// <param name="importedAssets">가져온 에셋 경로 배열</param>
        /// <param name="movedAssets">이동된 에셋 경로 배열</param>
        /// <param name="movedFromAssetPaths">이동 전 에셋 경로 배열</param>
        /// <param name="settings">동기화 설정</param>
        /// <param name="syncUtility">동기화 유틸리티</param>
        private static void ProcessChangedExcelFiles(
            string[] importedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            ISettingsProvider settings,
            ISyncUtility syncUtility)
        {
            // 변경된 Excel 파일 경로 수집
            HashSet<string> changedExcelPaths = new HashSet<string>();

            // 1. 임포트된 에셋 확인
            foreach (string assetPath in importedAssets)
            {
                if (IsExcelFile(assetPath) && !_syncingAssets.Contains(assetPath))
                    changedExcelPaths.Add(assetPath);
            }

            // 2. 이동된 에셋 확인
            for (int i = 0; i < movedAssets.Length; i++)
            {
                if (IsExcelFile(movedAssets[i]) && !_syncingAssets.Contains(movedAssets[i]))
                    changedExcelPaths.Add(movedAssets[i]);

                if (IsExcelFile(movedFromAssetPaths[i]) && !_syncingAssets.Contains(movedFromAssetPaths[i]))
                    changedExcelPaths.Add(movedFromAssetPaths[i]);
            }

            // 변경된 Excel 파일에 대한 동기화 처리
            foreach (string excelPath in changedExcelPaths)
            {
                _syncingAssets.Add(excelPath);
                ProcessExcelFileChange(excelPath, settings, syncUtility);
            }
        }

        /// <summary>
        /// Excel 파일 변경 처리
        /// </summary>
        /// <param name="excelPath">엑셀 파일 경로</param>
        /// <param name="settings">동기화 설정</param>
        /// <param name="syncUtility">동기화 유틸리티</param>
        /// <returns>동기화 수행 여부</returns>
        private static bool ProcessExcelFileChange(
            string excelPath,
            ISettingsProvider settings,
            ISyncUtility syncUtility)
        {
            string fullChangedPath = Path.GetFullPath(excelPath).Replace('\\', '/');
            bool hasProcessed = false;

            // 동기화 항목에서 Excel 파일 경로와 일치하는 항목 찾기
            foreach (var syncItem in settings.SyncItems)
            {
                if (!syncItem.EnableExcelToSO)
                    continue;

                string fullItemPath = Path.GetFullPath(Path.Combine(Application.dataPath, syncItem.ExcelPath)).Replace('\\', '/');

                if (string.Equals(fullItemPath, fullChangedPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (settings.ShowLogs)
                        UnityEngine.Debug.Log($"[ExcelSync] Excel 파일 변경 감지: {excelPath}");

                    // ScriptableObject 로드
                    string assetPath = "Assets/" + syncItem.AssetPath;

                    // 이미 동기화 중인 에셋은 건너뜀
                    if (_syncingAssets.Contains(assetPath))
                        continue;

                    _syncingAssets.Add(assetPath);

                    var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                    if (so != null)
                    {
                        // Excel에서 ScriptableObject로 데이터 가져오기
                        syncUtility.ImportExcelToSO(so);
                        hasProcessed = true;

                        if (settings.ShowLogs)
                            UnityEngine.Debug.Log($"[ExcelSync] {syncItem.Name} 동기화 완료: Excel → SO");
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"[ExcelSync] ScriptableObject를 찾을 수 없음: {assetPath}");
                    }

                    break;
                }
            }

            return hasProcessed;
        }

        /// <summary>
        /// 엑셀 파일인지 확인
        /// </summary>
        /// <param name="path">파일 경로</param>
        /// <returns>엑셀 파일 여부</returns>
        private static bool IsExcelFile(string path)
        {
            return path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);
        }
    }
}