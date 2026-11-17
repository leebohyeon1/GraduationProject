using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public sealed class KeepPlayerHUDDuringTimeline : MonoBehaviour
{
    private static readonly HashSet<KeepPlayerHUDDuringTimeline> Active = new();
    private PlayableDirector _director;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Active.Clear();
    public static bool IsActive
    {
        get
        {
            foreach (var item in Active)
                if (item && item._director && item._director.state == PlayState.Playing) return true;
            return false;
        }
    }
    private void OnEnable() { _director = GetComponent<PlayableDirector>(); Active.Add(this); }
    private void OnDisable() => Active.Remove(this);
}
