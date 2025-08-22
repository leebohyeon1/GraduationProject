using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Reflection;
using BH_Lib.DI;
using BH_Lib.ExcelConverter.Settings;
using BH_Lib.ExcelConverter.Convert;
using BH_Lib.ExcelConverter.Utility;

namespace BH_Lib.ExcelConverter.Editor
{
    /// <summary>
    /// 스크립터블 오브젝트와 엑셀 파일 간 데이터 동기화를 관리하는 에디터 윈도우
    /// </summary>
    public class ExcelToolWindow : EditorWindow
    {
        #region Private Variables
        private ScriptableObject _targetSO;
        private string _excelFilePath = "";
        private string _exportMessage = "";
        private string _importMessage = "";
        private bool _showExportSettings = false;
        private bool _showImportSettings = false;
        private bool _showSyncSettings = false;
        private bool _showLogs = false;
        private Vector2 _scrollPosition;
        private string _exportFileName = "";
        private string _soTypeName = "";
        private SyncSettingItem _currentSyncItem;
        private bool _isRegisteredSO = false;

        [Inject]
        private ISettingsProvider _settingsProvider;

        [Inject]
        private ISyncUtility _syncUtility;
        #endregion

        #region Unity Methods
        [MenuItem("BH_Lib/엑셀 데이터 동기화 도구")]
        public static void ShowWindow()
        {
            GetWindow<ExcelToolWindow>("엑셀 데이터 동기화 도구");
        }

        private void OnEnable()
        {
            // 기본 엑셀 폴더 생성
            string defaultExcelPath = Application.dataPath + "/00_Data/ExcelData";
            if (!Directory.Exists(defaultExcelPath))
                Directory.CreateDirectory(defaultExcelPath);

            // DI 컨테이너를 통한 의존성 주입
            DIContainer.Instance.InjectInto(this);
        }

        private void OnGUI()
        {
            if (_settingsProvider == null)
            {
                DIContainer.Instance.InjectInto(this);
                if (_settingsProvider == null)
                {
                    EditorGUILayout.HelpBox("DI 컨테이너가 초기화되지 않았습니다.", MessageType.Error);
                    if (GUILayout.Button("DI 컨테이너 초기화"))
                    {
                        ExcelConverterInitializer.Initialize();
                        DIContainer.Instance.InjectInto(this);
                    }
                    return;
                }
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("엑셀 데이터 동기화 도구", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("이 창에서 ScriptableObject를 Excel 파일과 동기화할 수 있습니다.", MessageType.Info);
            GUILayout.Space(10);

            // ScriptableObject 객체 선택
            DrawScriptableObjectField();

            if (_targetSO != null)
            {
                DrawSOInfo();
                DrawSyncSettings();

                GUILayout.Space(15);

                if (GUILayout.Button("동기화 설정 관리...", GUILayout.Height(25)))
                    ExcelSyncSettingsEditor.ShowWindow();

                GUILayout.Space(10);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Space(10);

                // 내보내기 섹션
                DrawExportSection();

                GUILayout.Space(20);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Space(20);

                // 가져오기 섹션
                DrawImportSection();

                DrawFileOperationButtons();

                // 자동 동기화 설정
                if (_isRegisteredSO)
                {
                    DrawAutoSyncSettings();
                }
            }

            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region GUI Drawing Methods
        /// <summary>
        /// ScriptableObject 필드 그리기
        /// </summary>
        private void DrawScriptableObjectField()
        {
            EditorGUILayout.BeginHorizontal();
            var newTargetSO = (ScriptableObject)EditorGUILayout.ObjectField("ScriptableObject", _targetSO, typeof(ScriptableObject), false);
            if (newTargetSO != _targetSO)
            {
                _targetSO = newTargetSO;
                UpdateSOInfo();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// ScriptableObject 정보 그리기
        /// </summary>
        private void DrawSOInfo()
        {
            _soTypeName = _targetSO.GetType().Name;
            EditorGUILayout.LabelField("타입:", _soTypeName);

            string assetPath = AssetDatabase.GetAssetPath(_targetSO);
            EditorGUILayout.LabelField("에셋 경로:", assetPath);
        }

        /// <summary>
        /// 동기화 설정 정보 그리기
        /// </summary>
        private void DrawSyncSettings()
        {
            string assetPath = AssetDatabase.GetAssetPath(_targetSO);
            _currentSyncItem = _settingsProvider.GetSyncItemByAssetPath(assetPath);
            _isRegisteredSO = _currentSyncItem != null;

            if (_isRegisteredSO)
            {
                EditorGUILayout.HelpBox(
                    $"이 ScriptableObject는 이미 동기화 설정되어 있습니다.\n" +
                    $"이름: {_currentSyncItem.Name}\n" +
                    $"엑셀 경로: {_currentSyncItem.ExcelPath}",
                    MessageType.Info
                );

                _excelFilePath = _settingsProvider.GetFullExcelPath(_currentSyncItem);
                if (string.IsNullOrEmpty(_exportFileName))
                    _exportFileName = Path.GetFileName(_excelFilePath);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "이 ScriptableObject는 아직 동기화 설정이 없습니다. 동기화 설정을 추가하세요.",
                    MessageType.Warning
                );

                if (string.IsNullOrEmpty(_exportFileName))
                    _exportFileName = $"{_soTypeName}.xlsx";

                if (string.IsNullOrEmpty(_excelFilePath))
                    _excelFilePath = Application.dataPath + "/00_Data/ExcelData/" + _exportFileName;

                if (GUILayout.Button("동기화 설정 추가", GUILayout.Height(30)))
                    AddSyncSetting();
            }
        }

        /// <summary>
        /// 내보내기 섹션 그리기
        /// </summary>
        private void DrawExportSection()
        {
            _showExportSettings = EditorGUILayout.Foldout(_showExportSettings, "내보내기 설정", true);
            if (_showExportSettings)
            {
                EditorGUI.indentLevel++;
                if (!_isRegisteredSO)
                {
                    _exportFileName = EditorGUILayout.TextField("파일 이름", _exportFileName);
                    if (!_exportFileName.EndsWith(".xlsx"))
                        _exportFileName += ".xlsx";

                    _excelFilePath = EditorGUILayout.TextField("파일 경로", _excelFilePath);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("다른 위치 지정..."))
                    {
                        string path = EditorUtility.SaveFilePanel("엑셀 파일 저장", Path.GetDirectoryName(_excelFilePath), _exportFileName, "xlsx");
                        if (!string.IsNullOrEmpty(path))
                        {
                            _excelFilePath = path;
                            _exportFileName = Path.GetFileName(path);
                        }
                    }
                    if (GUILayout.Button("기본 위치로"))
                    {
                        _excelFilePath = Application.dataPath + "/00_Data/ExcelData/" + _exportFileName;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField("파일 이름:", Path.GetFileName(_excelFilePath));
                    EditorGUILayout.LabelField("파일 경로:", _excelFilePath);
                }
                _showLogs = EditorGUILayout.Toggle("로그 출력", _showLogs);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _targetSO != null;
            if (GUILayout.Button("Excel로 내보내기", GUILayout.Height(30)))
                ExportToExcel();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_exportMessage))
                EditorGUILayout.HelpBox(_exportMessage, MessageType.Info);
        }

        /// <summary>
        /// 가져오기 섹션 그리기
        /// </summary>
        private void DrawImportSection()
        {
            _showImportSettings = EditorGUILayout.Foldout(_showImportSettings, "가져오기 설정", true);
            if (_showImportSettings)
            {
                EditorGUI.indentLevel++;
                if (!_isRegisteredSO)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField("파일 경로", _excelFilePath);
                    if (GUILayout.Button("찾아보기...", GUILayout.Width(100)))
                    {
                        string path = EditorUtility.OpenFilePanel("엑셀 파일 열기", Path.GetDirectoryName(_excelFilePath), "xlsx");
                        if (!string.IsNullOrEmpty(path))
                            _excelFilePath = path;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField("파일 경로:", _excelFilePath);
                }
                _showLogs = EditorGUILayout.Toggle("로그 출력", _showLogs);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _targetSO != null && File.Exists(_excelFilePath);
            if (GUILayout.Button("Excel에서 가져오기", GUILayout.Height(30)))
                ImportFromExcel();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_importMessage))
                EditorGUILayout.HelpBox(_importMessage, MessageType.Info);
        }

        /// <summary>
        /// 파일 조작 버튼 그리기
        /// </summary>
        private void DrawFileOperationButtons()
        {
            if (File.Exists(_excelFilePath))
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("파일 열기", GUILayout.Height(25)))
                    Application.OpenURL("file://" + _excelFilePath);
                if (GUILayout.Button("폴더 열기", GUILayout.Height(25)))
                    Application.OpenURL("file://" + Path.GetDirectoryName(_excelFilePath));
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 자동 동기화 설정 그리기
        /// </summary>
        private void DrawAutoSyncSettings()
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);

            _showSyncSettings = EditorGUILayout.Foldout(_showSyncSettings, "자동 동기화 설정", true);
            if (_showSyncSettings)
            {
                EditorGUI.indentLevel++;
                bool soToExcel = EditorGUILayout.Toggle("SO → Excel 자동 동기화", _currentSyncItem.EnableSOToExcel);
                if (soToExcel != _currentSyncItem.EnableSOToExcel)
                {
                    _currentSyncItem.EnableSOToExcel = soToExcel;
                    EditorUtility.SetDirty(ExcelSyncSettingsSO.Instance);
                    AssetDatabase.SaveAssets();
                }

                bool excelToSO = EditorGUILayout.Toggle("Excel → SO 자동 동기화", _currentSyncItem.EnableExcelToSO);
                if (excelToSO != _currentSyncItem.EnableExcelToSO)
                {
                    _currentSyncItem.EnableExcelToSO = excelToSO;
                    EditorUtility.SetDirty(ExcelSyncSettingsSO.Instance);
                    AssetDatabase.SaveAssets();
                }
                EditorGUILayout.HelpBox(
                    soToExcel
                        ? "SO 저장 시 자동으로 엑셀에 동기화됩니다."
                        : "SO 저장 시 자동으로 엑셀에 동기화가 비활성화되었습니다. 수동으로 동기화해야 합니다.",
                    MessageType.Info
                );
                EditorGUILayout.HelpBox(
                    excelToSO
                        ? "엑셀 저장 시 자동으로 SO에 동기화됩니다."
                        : "엑셀 저장 시 자동으로 SO에 동기화가 비활성화되었습니다. 수동으로 동기화해야 합니다.",
                    MessageType.Info
                );
                EditorGUI.indentLevel--;
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// ScriptableObject 정보 업데이트
        /// </summary>
        private void UpdateSOInfo()
        {
            _exportMessage = "";
            _importMessage = "";

            if (_targetSO == null)
                return;

            _soTypeName = _targetSO.GetType().Name;
            string assetPath = AssetDatabase.GetAssetPath(_targetSO);

            _currentSyncItem = _settingsProvider.GetSyncItemByAssetPath(assetPath);
            _isRegisteredSO = _currentSyncItem != null;

            if (_isRegisteredSO)
            {
                _excelFilePath = _settingsProvider.GetFullExcelPath(_currentSyncItem);
                _exportFileName = Path.GetFileName(_excelFilePath);
            }
            else
            {
                string baseName = _soTypeName.EndsWith("SO") ? _soTypeName.Substring(0, _soTypeName.Length - 2) : _soTypeName;
                _exportFileName = $"{baseName}.xlsx";
                _excelFilePath = Path.GetFullPath(Application.dataPath + "/00_Data/ExcelData/" + _exportFileName);
            }
        }

        /// <summary>
        /// 동기화 설정 추가
        /// </summary>
        private void AddSyncSetting()
        {
            if (_targetSO == null)
                return;

            string assetPath = AssetDatabase.GetAssetPath(_targetSO);
            if (string.IsNullOrEmpty(assetPath))
            {
                EditorUtility.DisplayDialog("알림", "ScriptableObject가 에셋이 아닙니다.", "확인");
                return;
            }

            string relativeExcelPath = _excelFilePath.Replace(Application.dataPath, "").TrimStart('/', '\\').Replace('\\', '/');

            string itemName = _soTypeName.EndsWith("SO")
                ? _soTypeName.Substring(0, _soTypeName.Length - 2)
                : _soTypeName;

            _currentSyncItem = _settingsProvider.AddSyncItem(
                itemName,
                assetPath.Replace("Assets/", ""),
                relativeExcelPath,
                true
            );

            _isRegisteredSO = true;
            EditorUtility.SetDirty(ExcelSyncSettingsSO.Instance);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("동기화 설정 추가", $"{itemName} 동기화 정보가 추가되었습니다.", "확인");
            
            // 동기화 설정 추가 후 자동으로 Excel로 내보내기 실행
            if (EditorUtility.DisplayDialog("Excel 내보내기", 
                "동기화 설정이 추가되었습니다.\n지금 Excel 파일로 내보내시겠습니까?", 
                "내보내기", "나중에"))
            {
                // 직접 Exporter 호출하여 첫 번째 내보내기 수행
                try
                {
                    bool success = InvokeExporter(_targetSO, _excelFilePath, true);
                    if (success)
                    {
                        EditorUtility.DisplayDialog("내보내기 완료",
                            $"Excel 파일이 성공적으로 생성되었습니다.\n\n경로: {_excelFilePath}",
                            "확인");
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Excel 내보내기 실패: {e.Message}");
                    EditorUtility.DisplayDialog("내보내기 실패", 
                        $"Excel 파일 생성 중 오류가 발생했습니다.\n{e.Message}", 
                        "확인");
                }
            }
        }

        /// <summary>
        /// Excel로 내보내기
        /// </summary>
        private void ExportToExcel()
        {
            if (_targetSO == null)
            {
                _exportMessage = "오류: ScriptableObject를 선택하세요!";
                return;
            }

            try
            {
                bool success = _isRegisteredSO
                    ? _syncUtility.ExportSOToExcel(_targetSO)
                    : InvokeExporter(_targetSO, _excelFilePath, _showLogs);

                if (success)
                {
                    _exportMessage = $"성공: Excel로 내보내기 완료!\n경로: {_excelFilePath}";
                    if (EditorUtility.DisplayDialog("내보내기 완료",
                        $"Excel 파일이 성공적으로 내보내졌습니다.\n\n경로: {_excelFilePath}",
                        "열기", "닫기"))
                    {
                        Application.OpenURL("file://" + _excelFilePath);
                    }
                }
                else
                {
                    _exportMessage = "오류: 내보내기에 실패했습니다.";
                }
            }
            catch (Exception e)
            {
                _exportMessage = $"오류: 내보내기 중 예외 발생: {e.Message}";
                UnityEngine.Debug.LogError($"ExportToExcel 예외: {e}");
            }
        }

        /// <summary>
        /// Excel에서 가져오기
        /// </summary>
        private void ImportFromExcel()
        {
            if (_targetSO == null)
            {
                _importMessage = "오류: ScriptableObject를 선택하세요!";
                return;
            }

            if (!File.Exists(_excelFilePath))
            {
                _importMessage = $"오류: 파일을 찾을 수 없습니다.\n경로: {_excelFilePath}";
                return;
            }

            try
            {
                bool success = _isRegisteredSO
                    ? _syncUtility.ImportExcelToSO(_targetSO)
                    : InvokeImporter(_targetSO, _excelFilePath, _showLogs);

                if (success)
                {
                    _importMessage = "성공: Excel에서 가져오기 완료!";
                    EditorUtility.SetDirty(_targetSO);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    _importMessage = "오류: 가져오기에 실패했습니다.";
                }
            }
            catch (Exception e)
            {
                _importMessage = $"오류: 가져오기 중 예외 발생: {e.Message}";
                UnityEngine.Debug.LogError($"ImportFromExcel 예외: {e}");
            }
        }

        /// <summary>
        /// 제네릭 익스포터 호출
        /// </summary>
        /// <param name="soInstance">ScriptableObject 인스턴스</param>
        /// <param name="path">파일 경로</param>
        /// <param name="log">로그 출력 여부</param>
        /// <returns>성공 여부</returns>
        private bool InvokeExporter(object soInstance, string path, bool log)
        {
            var soType = soInstance.GetType();
            var exporterType = typeof(ExcelExporter<>).MakeGenericType(soType);
            var exporter = Activator.CreateInstance(exporterType, new object[] { soInstance, path, log });
            var method = exporterType.GetMethod("Export");
            return (bool)method.Invoke(exporter, null);
        }

        /// <summary>
        /// 제네릭 임포터 호출
        /// </summary>
        /// <param name="soInstance">ScriptableObject 인스턴스</param>
        /// <param name="path">파일 경로</param>
        /// <param name="log">로그 출력 여부</param>
        /// <returns>성공 여부</returns>
        private bool InvokeImporter(object soInstance, string path, bool log)
        {
            var soType = soInstance.GetType();
            var importerType = typeof(ExcelImporter<>).MakeGenericType(soType);
            var importer = Activator.CreateInstance(importerType, new object[] { soInstance, path, log });
            var method = importerType.GetMethod("Import");
            return (bool)method.Invoke(importer, null);
        }
        #endregion
    }
}