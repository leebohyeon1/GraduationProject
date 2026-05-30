using UnityEngine;
using UnityEngine.Playables;

public class TimelineReversePlayer : MonoBehaviour
{
    public PlayableDirector director;
    public float speed = 1f;
    private bool playingReverse;

    void Update()
    {
        if (!playingReverse || director == null) return;

        director.time -= Time.deltaTime * speed;
        if (director.time <= 0)
        {
            director.time = 0;
            playingReverse = false;
        }

        director.Evaluate();
    }

    public void PlayReverse()
    {
        if (director == null) return;
        director.time = director.duration;
        director.Evaluate();
        playingReverse = true;
    }
}