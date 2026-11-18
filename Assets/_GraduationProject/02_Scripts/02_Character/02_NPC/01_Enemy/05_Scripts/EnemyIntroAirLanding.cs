using UnityEngine;
using UnityEngine.Playables;

/// <summary>Installs F's intro before Enemy initialization caches it, preserving any scene-authored intro.</summary>
[DefaultExecutionOrder(-11000)]
public sealed class EnemyIntroAirLanding : MonoBehaviour
{
    public string stateName = "Base Layer.Spawn_f";
    public float activationDistance = 18f;
    public float minimumDuration = 3.4f;

    private void Awake()
    {
        var intro = GetComponent<EnemySpawnIntro>();
        if (!intro)
        {
            intro = gameObject.AddComponent<EnemySpawnIntro>();
            intro.stateName = stateName;
            intro.activationDistance = activationDistance;
            intro.minimumDuration = minimumDuration;
        }
        // Side2_SpawnMonster is on the encounter parent and is explicitly bound to this Animator.
        // Zone7 has no such binding and retains its existing proximity-driven Spawn_f path.
        var animator = GetComponent<Animator>();
        foreach (var director in GetComponentsInParent<PlayableDirector>(true))
        {
            if (!director.playableAsset) continue;
            foreach (var output in director.playableAsset.outputs)
            {
                if (director.GetGenericBinding(output.sourceObject) != animator) continue;
                intro.BindTimeline(director);
                return;
            }
        }
    }
}
