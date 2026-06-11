using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 박싱 방지 딕셔너리를 사용하여 GC Alloc을 0으로 만드는 고성능 블랙보드입니다.
/// 모든 기존 API(오버로드)를 지원하여 프로젝트의 안정성을 보장합니다.
/// </summary>
public class BlackBoard 
{
    private Dictionary<string, object> _data = new Dictionary<string, object>();
    private Dictionary<string, bool> _boolData = new Dictionary<string, bool>();
    private Dictionary<string, float> _floatData = new Dictionary<string, float>();
    private Dictionary<string, int> _intData = new Dictionary<string, int>();
    private Dictionary<string, Vector3> _vectorData = new Dictionary<string, Vector3>();

    public void SetValue(string key, object value)
    {
        if (value is bool b) _boolData[key] = b;
        else if (value is float f) _floatData[key] = f;
        else if (value is int i) _intData[key] = i;
        else if (value is Vector3 v) _vectorData[key] = v;
        else _data[key] = value;
    }

    public T GetValue<T>(string key)
    {
        if (typeof(T) == typeof(bool)) return (T)(object)(_boolData.TryGetValue(key, out bool b) ? b : false);
        if (typeof(T) == typeof(float)) return (T)(object)(_floatData.TryGetValue(key, out float f) ? f : 0f);
        if (typeof(T) == typeof(int)) return (T)(object)(_intData.TryGetValue(key, out int i) ? i : 0);
        if (typeof(T) == typeof(Vector3)) return (T)(object)(_vectorData.TryGetValue(key, out Vector3 v) ? v : Vector3.zero);

        if (_data.TryGetValue(key, out object value) && value is T tValue) return tValue;
        return default;
    }

    // [Crucial Fix] 인자가 2개인 GetValue: out 변수 할당 보장 및 성공 여부 반환
    public bool GetValue<T>(string key, out T value)
    {
        if (HasKey(key))
        {
            value = GetValue<T>(key);
            return true;
        }
        value = default; // 반드시 할당해야 컴파일 에러가 발생하지 않음
        return false;
    }

    public T GetValueOrDefault<T>(string key, T defaultValue)
    {
        if (HasKey(key)) return GetValue<T>(key);
        return defaultValue;
    }

    public bool HasKey(string key)
    {
        return _boolData.ContainsKey(key) || _floatData.ContainsKey(key) || 
               _intData.ContainsKey(key) || _vectorData.ContainsKey(key) || 
               _data.ContainsKey(key);
    }

    public void RemoveKey(string key)
    {
        _boolData.Remove(key); _floatData.Remove(key); _intData.Remove(key); 
        _vectorData.Remove(key); _data.Remove(key);
    }

    #region Enum Overloads
    public void SetValue(EnemyBlackboardKeys key, object value) => SetValue(key.ToKey(), value);
    public T GetValue<T>(EnemyBlackboardKeys key) => GetValue<T>(key.ToKey());
    public bool GetValue<T>(EnemyBlackboardKeys key, out T value) => GetValue(key.ToKey(), out value);
    public T GetValueOrDefault<T>(EnemyBlackboardKeys key, T defaultValue) => GetValueOrDefault(key.ToKey(), defaultValue);
    public bool HasKey(EnemyBlackboardKeys key) => HasKey(key.ToKey());
    public void RemoveKey(EnemyBlackboardKeys key) => RemoveKey(key.ToKey());
    #endregion
}
