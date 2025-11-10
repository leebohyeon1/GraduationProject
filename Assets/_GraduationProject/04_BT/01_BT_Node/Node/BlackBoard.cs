using System.Collections.Generic;
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
    public bool HasKey(string key)
    {
        return _data.ContainsKey(key);
    }   
}
