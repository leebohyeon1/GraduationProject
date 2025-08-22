using BH_Lib.DI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BH_Lib.ExcelConverter.Settings
{
    /// <summary>
    /// 엑셀 동기화 설정을 관리하는 ScriptableObject
    /// </summary>
    [Register(typeof(ISettingsProvider))]
    public class ExcelSyncSettingsSO : ScriptableObject, ISettingsProvider
    {
        private static string GetSettingsPath()
        {
            var script = MonoScript.FromScriptableObject(CreateInstance<ExcelSyncSettingsSO>());
            var scriptPath = AssetDatabase.GetAssetPath(script);
            var bhLibPath = scriptPath.Substring(0, scriptPath.IndexOf("/01_ExcelConverter"));
            return $"{bhLibPath}/Resources/ExcelSyncSettings.asset";
        }

        /// <summary>
        /// 모든 ScriptableObject 자동 동기화 활성화
        /// </summary>
        [Tooltip("모든 ScriptableObject 자동 동기화 활성화")]
        [SerializeField]
        private bool _globalAutoSyncEnabled = true;

        /// <summary>
        /// 동기화 항목 리스트
        /// </summary>
        [Tooltip("동기화 항목 리스트")]
        [SerializeField]
        private List<SyncSettingItem> _syncItems = new List<SyncSettingItem>();

        /// <summary>
        /// 로그 출력 여부
        /// </summary>
        [Tooltip("로그 출력 여부")]
        [SerializeField]
        private bool _showLogs = true;

        /// <summary>
        /// 자동 동기화 지연 시간(초)
        /// </summary>
        [Tooltip("자동 동기화 지연 시간(초)")]
        [Range(0.5f, 5.0f)]
        [SerializeField]
        private float _syncDelay = 1.5f;

        #region Singleton 구현
        private static ExcelSyncSettingsSO _instance;

        /// <summary>
        /// 싱글톤 인스턴스 접근 프로퍼티
        /// </summary>
        public static ExcelSyncSettingsSO Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 먼저 에셋을 로드 시도
                    var settingsPath = GetSettingsPath();
                    _instance = AssetDatabase.LoadAssetAtPath<ExcelSyncSettingsSO>(settingsPath);

                    // 에셋이 없으면 생성
                    if (_instance == null)
                    {
                        _instance = CreateInstance<ExcelSyncSettingsSO>();

                        // 디렉토리 경로 추출 및 정규화
                        string directoryPath = Path.GetDirectoryName(settingsPath).Replace('\\', '/');
                        string absoluteDirectoryPath = Path.GetFullPath(
                            Path.GetDirectoryName(Application.dataPath) + "/" + directoryPath);

                        // 디렉토리가 없는 경우에만 생성
                        if (!Directory.Exists(absoluteDirectoryPath))
                        {
                            Directory.CreateDirectory(absoluteDirectoryPath);
                            UnityEngine.Debug.Log($"Created directory: {absoluteDirectoryPath}");
                        }

                        // 에셋 생성 및 저장
                        AssetDatabase.CreateAsset(_instance, settingsPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        UnityEngine.Debug.Log($"Created new settings asset at: {settingsPath}");
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region ISettingsProvider 구현
        /// <summary>
        /// 전역 자동 동기화 활성화 여부
        /// </summary>
        public bool GlobalAutoSyncEnabled
        {
            get => _globalAutoSyncEnabled;
            set => _globalAutoSyncEnabled = value;
        }

        /// <summary>
        /// 로그 출력 여부
        /// </summary>
        public bool ShowLogs
        {
            get => _showLogs;
            set => _showLogs = value;
        }

        /// <summary>
        /// 자동 동기화 지연 시간(초)
        /// </summary>
        public float SyncDelay
        {
            get => _syncDelay;
            set => _syncDelay = value;
        }

        /// <summary>
        /// 동기화 항목 리스트
        /// </summary>
        public List<SyncSettingItem> SyncItems => _syncItems;

        /// <summary>
        /// 이름으로 설정 항목 조회
        /// </summary>
        /// <param name="name">설정 항목 이름</param>
        /// <returns>설정 항목</returns>
        public SyncSettingItem GetSyncItem(string name)
            => _syncItems.Find(i => i.Name == name);

        /// <summary>
        /// 에셋 경로로 설정 항목 조회
        /// </summary>
        /// <param name="assetPath">에셋 경로</param>
        /// <returns>설정 항목</returns>
        public SyncSettingItem GetSyncItemByAssetPath(string assetPath)
        {
            if (assetPath.StartsWith("Assets/"))
                assetPath = assetPath.Substring("Assets/".Length);
            
            return _syncItems.Find(i =>
                i.AssetPath == assetPath || ("Assets/" + i.AssetPath) == assetPath);
        }

        /// <summary>
        /// 새 설정 항목 추가
        /// </summary>
        /// <param name="name">이름</param>
        /// <param name="assetPath">에셋 경로</param>
        /// <param name="excelPath">엑셀 파일 경로</param>
        /// <param name="autoSync">자동 동기화 여부</param>
        /// <returns>추가된 설정 항목</returns>
        public SyncSettingItem AddSyncItem(string name, string assetPath, string excelPath, bool autoSync = true)
        {
            var existing = GetSyncItem(name);
            if (existing != null) return existing;

            if (assetPath.StartsWith("Assets/"))
                assetPath = assetPath.Substring("Assets/".Length);
            if (excelPath.StartsWith("Assets/"))
                excelPath = excelPath.Substring("Assets/".Length);

            var item = new SyncSettingItem
            {
                Name = name,
                AssetPath = assetPath,
                ExcelPath = excelPath,
                EnableSOToExcel = true,
                EnableExcelToSO = true,
            };

            _syncItems.Add(item);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            return item;
        }

        /// <summary>
        /// 설정 항목 삭제
        /// </summary>
        /// <param name="name">이름</param>
        /// <returns>삭제 성공 여부</returns>
        public bool RemoveSyncItem(string name)
        {
            int idx = _syncItems.FindIndex(i => i.Name == name);
            if (idx >= 0)
            {
                _syncItems.RemoveAt(idx);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 전체 경로로 엑셀 파일 위치 반환
        /// </summary>
        /// <param name="item">설정 항목</param>
        /// <returns>전체 엑셀 파일 경로</returns>
        public string GetFullExcelPath(SyncSettingItem item)
        {
            if (item == null) return null;
            
            string path = item.ExcelPath;
            
            // "Assets/"로 시작하면 제거
            if (path.StartsWith("Assets/"))
                path = path.Substring("Assets/".Length);
            
            // 절대 경로인 경우 그대로 반환
            if (Path.IsPathRooted(path))
                return path;
            
            // 상대 경로인 경우 Application.dataPath와 결합
            return Path.GetFullPath(Application.dataPath + "/" + path);
        }

        /// <summary>
        /// 전체 경로로 ScriptableObject 에셋 반환
        /// </summary>
        /// <param name="item">설정 항목</param>
        /// <returns>전체 에셋 경로</returns>
        public string GetFullAssetPath(SyncSettingItem item)
        {
            if (item == null) return null;
            var path = item.AssetPath.StartsWith("Assets/") ? item.AssetPath : "Assets/" + item.AssetPath;
            return path; // 슬래시 그대로 유지
        }
        #endregion
    }
}