using BH_Lib.Log;
using UnityEngine;

public class TestCounter : MonoBehaviour, ICounterable
{
    public bool IsCounterable => true;

    public void ExecuteCounterEffect()
    {
       Log.PrintColor(Color.red, "Counter Effect Executed");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
