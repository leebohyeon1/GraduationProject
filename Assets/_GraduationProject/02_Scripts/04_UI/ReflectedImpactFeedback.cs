using UnityEngine;
using MoreMountains.Feedbacks;
public static class ReflectedImpactFeedback
{
    private static float _last = -1f;
    public static void Play()
    {
        if (Time.unscaledTime - _last < .3f || Time.timeScale <= 0f) return;
        _last = Time.unscaledTime;
        // MMTimeManager subtracts a frame before applying the request. Keep at least one visible slow frame.
        float duration = Mathf.Max(.18f, Time.unscaledDeltaTime * 2f);
        MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, .25f, duration, false, 0f, false);
    }
}
