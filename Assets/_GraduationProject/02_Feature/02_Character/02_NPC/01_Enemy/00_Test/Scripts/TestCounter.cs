using BH_Lib.Log;
using UnityEngine;

public class TestCounter : MonoBehaviour, ICounterable
{
    public bool IsCounterable => true;

    public void ExecuteCounterEffect()
    {
        Log.Print(Color.red, "테스트 적 카운터");
    }
}
