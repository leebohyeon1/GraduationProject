using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using BehaviorTree;

namespace BehaviorTree.Editor
{
    public class BehaviorTreeGenerator
    {
        // Layout constants
        const int NODE_WIDTH = 150;
        const int NODE_HEIGHT = 100;

        // [MenuItem("Tools/AI/Generate Demo Tree")]
        public static void GenerateTree()
        {
            // TODO: Update this generator to work with the new ActionTree system.
            /*
            // 1. Create Tree Asset
            string path = "Assets/DemoBehaviorTree.asset";
            BehaviorTree tree = ScriptableObject.CreateInstance<BehaviorTree>();
            tree.name = "DemoBehaviorTree";
            AssetDatabase.CreateAsset(tree, path);

            // ... (Rest of logic needs updates for ActionTree) ...
            */
            Debug.LogWarning("Demo Tree Generation is currently disabled pending update to ActionTree system.");
        }
    }
}
