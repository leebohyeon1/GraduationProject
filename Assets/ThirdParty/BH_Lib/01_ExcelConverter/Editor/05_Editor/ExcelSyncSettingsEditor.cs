using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using BH_Lib.ExcelConverter.Settings;
using BH_Lib.ExcelConverter.Utility;
using BH_Lib.DI;

namespace BH_Lib.ExcelConverter.Editor
{
    /// <summary>
    /// 엑셀 동기화 설정을 편집할 수 있는 에디터 윈도우
    /// </summary>
    public class ExcelSyncSettingsEditor : EditorWindow
    {
        #region Private Variables
        private ExcelSyncSettingsSO _settings;
        private SerializedObject _serialized;
        private SerializedProperty _itemsProp;
        private Vector2 _scroll;
        private bool _showGlobal = true;
        private string _newName = "";
        private string _newAsset = "";
        private string _newExcel = "";
        #endregion

        #region Unity Methods
        [MenuItem("BH_Lib/엑셀 동기화 설정 관리")]
        public static void ShowWindow()
        {
            var w = GetWindow<ExcelSyncSettingsEditor>("엑셀 동기화 설정");
            w.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            _settings = ExcelSyncSettingsSO.Instance;
            _serialized = new SerializedObject(_settings);
            _itemsProp = _serialized.FindProperty("_syncItems");
        }

        private void OnGUI()
        {
            if (_settings == null) OnEnable();
            _serialized.Update();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            // 전역 설정
            DrawGlobalSettings();

            EditorGUILayout.Space();
            GUILayout.Label("동기화 항목", EditorStyles.boldLabel);

            // 리스트 헤더
            DrawListHeader();

            // 각 항목
            DrawSyncItems();

            EditorGUILayout.Space();
            DrawNewItemSection();

            EditorGUILayout.Space();
            DrawBulkSyncSection();

            EditorGUILayout.EndScrollView();
            if (_serialized.hasModifiedProperties)
            {
                _serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
            }
        }
        #endregion

        #region GUI Drawing Methods
        /// <summary>
        /// 전역 설정 섹션 그리기
        /// </summary>
        private void DrawGlobalSettings()
        {
            _showGlobal = EditorGUILayout.Foldout(_showGlobal, "전역 설정", true);
            if (_showGlobal)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_serialized.FindProperty("_globalAutoSyncEnabled"), new GUIContent("자동 동기화 활성화"));
                EditorGUILayout.PropertyField(_serialized.FindProperty("_showLogs"), new GUIContent("로그 출력"));
                EditorGUILayout.PropertyField(_serialized.FindProperty("_syncDelay"), new GUIContent("동기화 지연(초)"));
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// 리스트 헤더 그리기
        /// </summary>
        private void DrawListHeader()
        {
            if (_itemsProp.arraySize > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("이름", GUILayout.Width(150));
                GUILayout.Label("SO 경로", GUILayout.Width(200));
                GUILayout.Label("Excel 경로", GUILayout.Width(190));
                GUILayout.Label("SO To Excel", GUILayout.Width(80));
                GUILayout.Label("Excel To SO", GUILayout.Width(100));
                GUILayout.Label("", GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 동기화 항목 그리기
        /// </summary>
        private void DrawSyncItems()
        {
            for (int i = 0; i < _itemsProp.arraySize; i++)
            {
                var itemProp = _itemsProp.GetArrayElementAtIndex(i);
                var nameProp = itemProp.FindPropertyRelative("Name");
                var assetProp = itemProp.FindPropertyRelative("AssetPath");
                var excelProp = itemProp.FindPropertyRelative("ExcelPath");
                var excelToSOProp = itemProp.FindPropertyRelative("EnableExcelToSO");
                var soToExcelProp = itemProp.FindPropertyRelative("EnableSOToExcel");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(nameProp, GUIContent.none, GUILayout.Width(150));

                EditorGUILayout.PropertyField(assetProp, GUIContent.none, GUILayout.Width(170));
                if (GUILayout.Button("…", GUILayout.Width(25)))
                {
                    string p = EditorUtility.OpenFilePanel("SO 선택", Application.dataPath, "asset");
                    if (!string.IsNullOrEmpty(p))
                        assetProp.stringValue = p.Replace(Application.dataPath + "/", "");
                }

                EditorGUILayout.PropertyField(excelProp, GUIContent.none, GUILayout.Width(170));
                if (GUILayout.Button("…", GUILayout.Width(25)))
                {
                    string p = EditorUtility.OpenFilePanel("Excel 선택", Application.dataPath, "xlsx");
                    if (!string.IsNullOrEmpty(p))
                        excelProp.stringValue = p.Replace(Application.dataPath + "/", "");
                }

                EditorGUILayout.PropertyField(soToExcelProp, GUIContent.none, GUILayout.Width(80));
                EditorGUILayout.PropertyField(excelToSOProp, GUIContent.none, GUILayout.Width(80));

                if (GUILayout.Button("삭제", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("항목 삭제", $"{nameProp.stringValue}을(를) 삭제하시겠습니까?", "예", "아니오"))
                    {
                        _itemsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 새 항목 추가 섹션 그리기
        /// </summary>
        private void DrawNewItemSection()
        {
            GUILayout.Label("새 항목 추가", EditorStyles.boldLabel);
            _newName = EditorGUILayout.TextField("이름", _newName);

            EditorGUILayout.BeginHorizontal();
            _newAsset = EditorGUILayout.TextField("SO 경로", _newAsset);
            if (GUILayout.Button("…", GUILayout.Width(25)))
            {
                string p = EditorUtility.OpenFilePanel("SO 선택", Application.dataPath, "asset");
                if (!string.IsNullOrEmpty(p))
                    _newAsset = p.Replace(Application.dataPath + "/", "");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _newExcel = EditorGUILayout.TextField("Excel 경로", _newExcel);
            if (GUILayout.Button("…", GUILayout.Width(25)))
            {
                string p = EditorUtility.OpenFilePanel("Excel 선택", Application.dataPath, "xlsx");
                if (!string.IsNullOrEmpty(p))
                    _newExcel = p.Replace(Application.dataPath + "/", "");
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("추가", GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(_newName) || string.IsNullOrEmpty(_newAsset) || string.IsNullOrEmpty(_newExcel))
                {
                    EditorUtility.DisplayDialog("에러", "모든 필드를 입력해주세요.", "확인");
                }
                else
                {
                    _itemsProp.InsertArrayElementAtIndex(_itemsProp.arraySize);
                    var newItem = _itemsProp.GetArrayElementAtIndex(_itemsProp.arraySize - 1);
                    newItem.FindPropertyRelative("Name").stringValue = _newName;
                    newItem.FindPropertyRelative("AssetPath").stringValue = _newAsset;
                    newItem.FindPropertyRelative("ExcelPath").stringValue = _newExcel;
                    newItem.FindPropertyRelative("AutoSync").boolValue = true;
                    newItem.FindPropertyRelative("TwoWaySync").boolValue = true;
                    _newName = _newAsset = _newExcel = "";
                }
            }
        }

        /// <summary>
        /// 일괄 동기화 섹션 그리기
        /// </summary>
        private void DrawBulkSyncSection()
        {
            GUILayout.Label("일괄 동기화", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            var syncUtility = DIContainer.Instance.Resolve<ISyncUtility>();
            if (syncUtility != null)
            {
                if (GUILayout.Button("모든 SO → Excel", GUILayout.Height(30)))
                    syncUtility.ExportAllSOToExcel();
                if (GUILayout.Button("모든 Excel → SO", GUILayout.Height(30)))
                    syncUtility.ImportAllExcelToSO();
            }
            else
            {
                EditorGUILayout.HelpBox("동기화 유틸리티를 찾을 수 없습니다.", MessageType.Warning);
            }

            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}