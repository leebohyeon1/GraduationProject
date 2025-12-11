using UnityEngine;
using BehaviorTree;

public class Task_SetBlackBoard : Node
{
    public string key;
    public enum ValueType
    {
        Integer,
        Float,
        Boolean,
        Vector3,
        String
    }
    public ValueType valueType;
    public int intValue;
    public float floatValue;
    public bool boolValue;
    public Vector3 vector3Value;
    public string stringValue;
    public override Node Clone()
    {
        var node = Instantiate(this);
        node.key = this.key;
        node.valueType = this.valueType;
        node.intValue = this.intValue;
        node.floatValue = this.floatValue;
        node.boolValue = this.boolValue;
        node.vector3Value = this.vector3Value;
        node.stringValue = this.stringValue;
        return node;
    }
    public override void OnEnter()
    {
        switch (valueType)
        {
            case ValueType.Integer:
                brain.blackboard.SetValue(key, intValue);
                break;
            case ValueType.Float:
                brain.blackboard.SetValue(key, floatValue);
                break;
            case ValueType.Boolean:
                brain.blackboard.SetValue(key, boolValue);
                break;
            case ValueType.Vector3:
                brain.blackboard.SetValue(key, vector3Value);
                break;
            case ValueType.String:
                brain.blackboard.SetValue(key, stringValue);
                break;
            default:
                break;
        }
    }
    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }


}