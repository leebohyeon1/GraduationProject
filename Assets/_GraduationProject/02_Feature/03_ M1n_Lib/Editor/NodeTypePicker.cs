using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

// 이 네임스페이스가 없다면 BehaviorTree를 포함하도록 수정하세요.
using BehaviorTree;

public class NodeTypePicker : EditorWindow
{
    private Action<Type> _onTypeSelected;
    private Vector2 _scrollPosition;
    private Type[] _nodeTypes;

    public static void Show(Action<Type> onTypeSelected)
    {
        NodeTypePicker window = GetWindow<NodeTypePicker>(true, "Add New Node", true);
        window._onTypeSelected = onTypeSelected;
        window.minSize = new Vector2(250, 300);
        window.maxSize = new Vector2(250, 300);
    }

    private void OnEnable()
    {
        // Node를 상속받는 모든 구체적인 클래스 타입을 찾습니다.
        _nodeTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(Node)) && !type.IsAbstract)
            .OrderBy(type => type.Name)
            .ToArray();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select Node Type to Create", EditorStyles.boldLabel);
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        foreach (Type type in _nodeTypes)
        {
            if (GUILayout.Button(type.Name))
            {
                _onTypeSelected?.Invoke(type);
                this.Close();
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
}


