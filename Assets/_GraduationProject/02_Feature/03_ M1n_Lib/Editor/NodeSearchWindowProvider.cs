// --- START OF FILE NodeSearchWindowProvider.cs ---

using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using BehaviorTree;

public class NodeSearchWindowProvider : ScriptableObject, ISearchWindowProvider
{
    private BehaviorTreeView _graphView;

    public void Initialize(BehaviorTreeView graphView)
    {
        _graphView = graphView;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
        };

        var nodeTypes = TypeCache.GetTypesDerivedFrom<BehaviorTree.Node>()
            .Where(type => !type.IsAbstract);

        foreach (var type in nodeTypes.OrderBy(t => t.Name))
        {
            tree.Add(new SearchTreeEntry(new GUIContent(ObjectNames.NicifyVariableName(type.Name)))
            {
                userData = type,
                level = 1,
            });
        }
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        var type = searchTreeEntry.userData as Type;
        if (type != null)
        {
            _graphView.CreateNode(type, context.screenMousePosition);
            return true;
        }
        return false;
    }
}
// --- END OF FILE NodeSearchWindowProvider.cs ---