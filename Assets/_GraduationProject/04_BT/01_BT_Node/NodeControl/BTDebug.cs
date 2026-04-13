public static class BTDebug
{
    [System.Diagnostics.Conditional("BT_DEBUG_LOG")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }
}
