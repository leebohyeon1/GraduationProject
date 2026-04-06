using UnityEngine;
using UnityEngine.Playables;

public class CutSceneTrigger : MonoBehaviour
{
    public PlayableDirector Director;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Director.Play();
        }
    }
}
