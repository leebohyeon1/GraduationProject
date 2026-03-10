using System;
using UnityEngine;

[Serializable]
public class BlackBoardUtils
{
    // ============================================================
    // 필드들 (인스펙터에서 설정 가능)
    // ============================================================
    
    /// <summary>BlackBoard 키 입력 방식</summary>
    public InputType inputType = InputType.Enum;
    
    /// <summary>Enum으로 키 선택 (InputType.Enum 일 때)</summary>
    public EnemyBlackboardKeys enumKey;
    
    /// <summary>string 키 직접 입력 (InputType.String 일 때)</summary>
    public string stringKey;
    
    /// <summary>BlackBoard 값 타입</summary>
    public ValueType valueType = ValueType.Boolean;
    
    /// <summary>int 값</summary>
    public int intValue;
    
    /// <summary>float 값</summary>
    public float floatValue;
    
    /// <summary>bool 값</summary>
    public bool boolValue;
    
    /// <summary>Vector3 값</summary>
    public Vector3 vector3Value;
    
    /// <summary>string 값</summary>
    public string stringValue;
    
    /// <summary>Transform 값 (GameObject에서 가져옴)</summary>
    public GameObject transformObjectValue;
    
    // ============================================================
    // Enum 정의들
    // ============================================================
    
    /// <summary>BlackBoard 키 입력 방식</summary>
    public enum InputType
    {
        /// <summary>string 직접 입력</summary>
        String,
        
        /// <summary>Enum으로 선택</summary>
        Enum
    }
    
    /// <summary>BlackBoard 값 타입</summary>
    public enum ValueType
    {
        Integer,
        Float,
        Boolean,
        Vector3,
        String,
        Transform
    }
    public string GetActualKey()
    {
        switch (inputType)
        {
            case InputType.Enum:
                return enumKey.ToKey();
                    
            case InputType.String:
                return stringKey;
                
            default:
                return stringKey;
        }
    }
    public void SetValue(BlackBoard blackboard, BlackBoardUtils utils)
    {
        if (blackboard == null)
        {
            // Debug.LogWarning("[BlackBoardUtils] BlackBoard is null!");
            return;
        }
        
        string actualKey = utils.GetActualKey();
        
        switch (utils.valueType)
        {
            case ValueType.Integer:
                blackboard.SetValue(actualKey, utils.intValue);
                break;
                
            case ValueType.Float:
                blackboard.SetValue(actualKey, utils.floatValue);
                break;
                
            case ValueType.Boolean:
                blackboard.SetValue(actualKey, utils.boolValue);
                break;
                
            case ValueType.Vector3:
                blackboard.SetValue(actualKey, utils.vector3Value);
                break;
                
            case ValueType.String:
                blackboard.SetValue(actualKey, utils.stringValue);
                break;
                
            case ValueType.Transform:
                if (utils.transformObjectValue != null)
                {
                    blackboard.SetValue(actualKey, utils.transformObjectValue.transform);
                }
                break;
                
            default:
                // Debug.LogWarning($"[BlackBoardUtils] Unknown ValueType: {utils.valueType}");
                break;
        }
    }
}