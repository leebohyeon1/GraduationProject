using System;
using UnityEngine;

public class MenuUIComponent : MonoBehaviour, IDisposable
{
    [SerializeField] private string _componentName;
    public string ComponentName => _componentName;



    public virtual void Initialize(MenuUI menu)
    {

    }

    public virtual void OnOpen()
    {

    }

    public virtual void Dispose()
    {
      
    }

}
