using UnityEngine;

public class SettingComponent : MonoBehaviour
{
    [SerializeField] private string _settingName;
    public string SettingName => _settingName;  
}
