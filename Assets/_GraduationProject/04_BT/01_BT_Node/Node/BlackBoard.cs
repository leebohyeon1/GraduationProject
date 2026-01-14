using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BlackBoard 
{
    private Dictionary<string, object> _data = new Dictionary<string, object>();

    public void SetValue(string key, object value)
    {
        _data[key] = value;
    }
    public T GetValue<T>(string key)
    {
        if (_data.TryGetValue(key, out object value))
        {
            if (value is T)
            {
                return (T)value;
            }
        }
        return default(T);
    }
    public bool GetValue<T>(string key, out T value)
    {
        if (_data.TryGetValue(key, out object rawValue) && rawValue is T)
        {
            value = (T)rawValue;
            return true;
        }
        value = default(T);
        return false;
    }
    public bool HasKey(string key)
    {
        return _data.ContainsKey(key);
    }
    #region 블랙보드 전용 Enum 키 버전
    public void SetValue(EnemyBlackboardKeys key, object value)
{
    SetValue(key.ToKey(), value);  // string으로 변환해서 호출
}
/// <summary>Enum 키로 값 가져오기</summary>
public T GetValue<T>(EnemyBlackboardKeys key)
{
    return GetValue<T>(key.ToKey());  // string으로 변환해서 호출
}
/// <summary>Enum 키로 값 가져오기 (out 버전)</summary>
public bool GetValue<T>(EnemyBlackboardKeys key, out T value)
{
    return GetValue(key.ToKey(), out value);  // string으로 변환해서 호출
}
/// <summary>Enum 키가 존재하는지 확인</summary>
public bool HasKey(EnemyBlackboardKeys key)
{
    return HasKey(key.ToKey());  // string으로 변환해서 호출
}
#endregion

    public void LogAllValues()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("--- BlackBoard Contents ---");

        if (_data.Count == 0)
        {
            sb.AppendLine(" (Empty)");
        }
        else
        {
            foreach (var pair in _data)
            {
                sb.AppendLine($" - {pair.Key}: {pair.Value ?? "null"}");
            }
        }
        
        sb.AppendLine("--------------------------------");
        
        Debug.Log(sb.ToString());
    }

}
