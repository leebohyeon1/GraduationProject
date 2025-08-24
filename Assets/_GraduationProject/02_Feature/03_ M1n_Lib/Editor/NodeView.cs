// --- START OF FILE NodeView.cs ---

using UnityEditor.Experimental.GraphView;
using UnityEngine;
using BehaviorTree;
using UnityEditor;
using UnityEngine.UIElements;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public BehaviorTree.Node node;
    public Port input;
    public Port output;

    public NodeView(BehaviorTree.Node node) : base()
    {
        this.node = node;
        this.title = node.name;
        this.viewDataKey = node.GetInstanceID().ToString();

        SetPosition(new Rect(node.position, new Vector2(150, 100)));
        
        CreateInputPorts();
        CreateOutputPorts();
    }

    // ★★★ 핵심 1: 노드가 선택되었을 때 호출되는 내장 함수(override) ★★★
    public override void OnSelected()
    {
        base.OnSelected();
        // 이 NodeView에 해당하는 ScriptableObject(node)를
        // Unity 에디터의 현재 선택 객체로 지정합니다.
        // 이 코드 한 줄로 인스펙터 창이 자동으로 갱신됩니다.
        Selection.activeObject = node;
    }
    
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        evt.menu.AppendSeparator();
        evt.menu.AppendAction("이름 바꾸기", OnRenameAction);
    }
    
    private void OnRenameAction(DropdownMenuAction action) { OpenRenameEditor(); }
    
    private void OpenRenameEditor()
    {
        var titleLabel = titleContainer.Q<Label>();
        var textField = new TextField { value = title, isDelayed = true };
        titleLabel.style.display = DisplayStyle.None;
        titleContainer.Insert(0, textField);
        textField.Focus();
        textField.SelectAll();
        textField.RegisterValueChangedCallback(e => OnRename(e.newValue, titleLabel, textField));
        textField.RegisterCallback<FocusOutEvent>(e => OnRename(textField.value, titleLabel, textField));
    }

    private void OnRename(string newName, Label titleLabel, TextField textField)
    {
        if (string.IsNullOrEmpty(newName) || textField.parent == null)
        {
            if (textField.parent != null) CloseRenameEditor(titleLabel, textField);
            return;
        }
        this.node.name = newName;
        this.title = newName;
        EditorUtility.SetDirty(this.node);
        AssetDatabase.SaveAssets();
        CloseRenameEditor(titleLabel, textField);
    }

    private void CloseRenameEditor(Label titleLabel, TextField textField)
    {
        textField.parent.Remove(textField);
        titleLabel.style.display = DisplayStyle.Flex;
    }
    
    private void CreateInputPorts()
    {
        input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        input.portName = "Parent";
        inputContainer.Add(input);
    }
    
    private void CreateOutputPorts()
    {
        if (node is CompositeNode)
        {
            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            output.portName = "Children";
        }
        else if (node is Decorator_Inverter)
        {
            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            output.portName = "Child";
        }
        if (output != null) outputContainer.Add(output);
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        if (node != null)
        {
            node.position = new Vector2(newPos.xMin, newPos.yMin);
            EditorUtility.SetDirty(node);
        }
    }
}
// --- END OF FILE NodeView.cs ---