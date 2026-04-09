using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 몬스터를 지면에 배치하거나, 구역을 브러시로 색칠하여 GroupAi를 설정하는 에디터 윈도우입니다.
/// </summary>
public class MonsterPlacementEditor : EditorWindow
{
    private enum EditMode { PaintZone, PlaceMonster }
    private EditMode _currentMode = EditMode.PlaceMonster;

    [Header("Monster Placement")]
    private List<GameObject> _monsterPrefabs = new List<GameObject>();
    private Vector2 _scrollPos;
    private int _selectedPrefabIndex = -1;
    
    [Header("Zone Painting")]
    private GroupAiZone _selectedZone;
    private float _brushRadius = 1.5f;
    private bool _isErasing = false;

    [Header("Global Settings")]
    private int _groundLayerIndex = 0;
    private bool _isEditorActive = false;

    [MenuItem("Tools/Monster Placement Editor")]
    public static void ShowWindow()
    {
        GetWindow<MonsterPlacementEditor>("Monster Editor");
    }

    private void OnEnable()
    {
        RefreshPrefabList();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void RefreshPrefabList()
    {
        _monsterPrefabs.Clear();
        string path = "Assets/_GraduationProject/03_Prefabs/01_Character/Monster/";
        if (!Directory.Exists(Application.dataPath.Replace("Assets", "") + path)) return;

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { path });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null && prefab.GetComponent<Enemy>() != null)
                _monsterPrefabs.Add(prefab);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        _currentMode = (EditMode)GUILayout.Toolbar((int)_currentMode, new string[] { "🎨 Paint Zones", "👾 Place Monsters" }, GUILayout.Height(30));
        
        EditorGUILayout.Space(10);
        _groundLayerIndex = EditorGUILayout.LayerField("Ground Layer", _groundLayerIndex);

        GroupAiZone.showAllZones = EditorGUILayout.Toggle("Show All Zones (Scene)", GroupAiZone.showAllZones);

        
        GUI.backgroundColor = _isEditorActive ? Color.green : Color.white;
        if (GUILayout.Button(_isEditorActive ? "EDITOR ACTIVE (ESC to Stop)" : "START EDITING", GUILayout.Height(40)))
        {
            _isEditorActive = !_isEditorActive;
            if (_isEditorActive) FocusSceneView();
        }
        GUI.backgroundColor = Color.white;

        if (_currentMode == EditMode.PaintZone)
            DrawZonePaintingGUI();
        else
            DrawMonsterPlacementGUI();
    }

    private void DrawZonePaintingGUI()
    {
        EditorGUILayout.LabelField("Zone Painting Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        _selectedZone = (GroupAiZone)EditorGUILayout.ObjectField("Active Zone", _selectedZone, typeof(GroupAiZone), true);
        if (GUILayout.Button("New Zone", GUILayout.Width(80))) CreateNewZone();
        EditorGUILayout.EndHorizontal();
        
        if (_selectedZone == null)
        {
            EditorGUILayout.HelpBox("Select a GroupAiZone above or click 'New Zone' to start painting.", MessageType.Info);
            return;
        }

        _brushRadius = EditorGUILayout.Slider("Brush Radius", _brushRadius, 0.5f, 10f);
        _isErasing = EditorGUILayout.Toggle("Eraser Mode (Hold Shift)", _isErasing);
        
        if (GUILayout.Button("Clear All Cells In Zone"))
        {
            if (EditorUtility.DisplayDialog("Clear Zone", "Are you sure you want to clear all painted cells?", "Yes", "No"))
            {
                Undo.RecordObject(_selectedZone, "Clear Zone Cells");
                _selectedZone.paintedCells.Clear();
                _selectedZone.RefreshCache();
            }
        }
    }

    private void DrawMonsterPlacementGUI()
    {
        EditorGUILayout.LabelField("Select Monster Prefab", EditorStyles.boldLabel);
        if (GUILayout.Button("Refresh List")) RefreshPrefabList();
        
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        int columns = Mathf.Max(1, (int)(position.width / 110));
        for (int i = 0; i < _monsterPrefabs.Count; i += columns)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < columns; j++)
            {
                int index = i + j;
                if (index >= _monsterPrefabs.Count) break;

                GameObject prefab = _monsterPrefabs[index];
                GUI.backgroundColor = (index == _selectedPrefabIndex) ? new Color(0, 0.5f, 1f, 1f) : Color.white;
                if (GUILayout.Button(new GUIContent(AssetPreview.GetAssetPreview(prefab), prefab.name), GUILayout.Width(100), GUILayout.Height(100)))
                    _selectedPrefabIndex = index;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        GUI.backgroundColor = Color.white;
    }

    private void CreateNewZone()
    {
        int zoneCount = FindObjectsByType<GroupAiZone>(FindObjectsSortMode.None).Length + 1;
        GameObject go = new GameObject("MonsterZone_" + zoneCount);
        GroupAiZone zone = go.AddComponent<GroupAiZone>();
        
        // 지면 레이어 자동 설정
        zone.groundLayer = 1 << _groundLayerIndex;
        zone.yOffset = 0.05f;

        // 고유한 색상 생성 (HSV 활용)
        float hue = (zoneCount * 0.381966f) % 1.0f;
        zone.zoneColor = Color.HSVToRGB(hue, 0.7f, 0.8f);
        zone.zoneColor.a = 0.4f;

        zone.zoneName = "Zone " + zoneCount;

        GameObject groupGo = new GameObject(go.name + "_GroupAI");
        zone.targetGroupAi = groupGo.AddComponent<GroupAi>();
        groupGo.transform.SetParent(go.transform);
        _selectedZone = zone;
        Undo.RegisterCreatedObjectUndo(go, "Create Monster Zone");
    }

    private void FocusSceneView() => SceneView.lastActiveSceneView?.Focus();

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!_isEditorActive) return;

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            _isEditorActive = false;
            Repaint();
            return;
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1 << _groundLayerIndex))
        {
            if (_currentMode == EditMode.PaintZone)
                HandleZonePainting(hit, e);
            else
                HandleMonsterPlacement(hit, e);
        }

        if (e.type == EventType.MouseMove) sceneView.Repaint();
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private void HandleZonePainting(RaycastHit hit, Event e)
    {
        if (_selectedZone == null) return;

        bool currentErasing = _isErasing || e.shift;
        Handles.color = currentErasing ? Color.red : _selectedZone.zoneColor;
        Handles.DrawWireDisc(hit.point, hit.normal, _brushRadius);

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && !e.alt)
        {
            Undo.RecordObject(_selectedZone, "Paint Zone");
            
            // 실시간으로 씬 내 모든 존을 찾아서 겹침 방지 준비
            var allZones = FindObjectsByType<GroupAiZone>(FindObjectsSortMode.None);
            
            float step = _selectedZone.cellSize;
            for (float x = -_brushRadius; x <= _brushRadius; x += step)
            {
                for (float z = -_brushRadius; z <= _brushRadius; z += step)
                {
                    Vector3 worldPos = hit.point + new Vector3(x, 0, z);
                    if ((worldPos - hit.point).sqrMagnitude <= _brushRadius * _brushRadius)
                    {
                        if (currentErasing)
                        {
                            _selectedZone.RemoveCell(worldPos);
                        }
                        else
                        {
                            // 다른 존들에서 이 좌표 지우기
                            Vector2Int gridPos = new Vector2Int(
                                Mathf.FloorToInt(worldPos.x / _selectedZone.cellSize),
                                Mathf.FloorToInt(worldPos.z / _selectedZone.cellSize)
                            );

                            foreach (var zone in allZones)
                            {
                                if (zone != _selectedZone)
                                {
                                    if (zone.InternalRemoveCell(gridPos))
                                    {
                                        Undo.RecordObject(zone, "Remove Overlap");
                                        EditorUtility.SetDirty(zone);
                                    }
                                }
                            }
                            _selectedZone.AddCell(worldPos);
                        }
                    }
                }
            }
            EditorUtility.SetDirty(_selectedZone);
            e.Use();
        }
    }
    private void HandleMonsterPlacement(RaycastHit hit, Event e)
    {
        if (_selectedPrefabIndex < 0) return;

        Handles.color = Color.cyan;
        Handles.DrawWireDisc(hit.point, hit.normal, 0.5f);
        Handles.Label(hit.point + Vector3.up, $"Place: {_monsterPrefabs[_selectedPrefabIndex].name}");

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            PlaceMonster(hit.point);
            e.Use();
        }
    }

    private void PlaceMonster(Vector3 position)
    {
        GameObject monster = (GameObject)PrefabUtility.InstantiatePrefab(_monsterPrefabs[_selectedPrefabIndex]);
        monster.transform.position = position;

        // 지면의 위치에 따른 자동 그룹 할당
        GroupAiZone foundZone = FindObjectsByType<GroupAiZone>(FindObjectsSortMode.None)
            .FirstOrDefault(z => z.IsInZone(position));

        if (foundZone != null && foundZone.targetGroupAi != null)
        {
            EnemyInitializer init = monster.GetComponent<EnemyInitializer>() ?? monster.GetComponentInChildren<EnemyInitializer>();
            if (init != null)
            {
                SerializedObject so = new SerializedObject(init);
                so.FindProperty("_targetGroupAi").objectReferenceValue = foundZone.targetGroupAi;
                so.ApplyModifiedProperties();
                monster.transform.SetParent(foundZone.transform);
                Debug.Log($"[MonsterEditor] Placed {monster.name} and assigned to {foundZone.zoneName}");
            }
        }

        Undo.RegisterCreatedObjectUndo(monster, "Place Monster");
    }
}
