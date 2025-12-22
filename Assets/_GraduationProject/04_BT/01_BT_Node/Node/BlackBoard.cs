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
