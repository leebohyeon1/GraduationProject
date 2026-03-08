using System.Collections.Generic;
using UnityEngine;

public class SimpleGamePlayTagTrigger : MonoBehaviour
{
    [SerializeField] private List<GamePlayTagSO> _gamePlayTags;   

    private void OnTriggerEnter(Collider other)
    {
        foreach(var  tag in _gamePlayTags)
        {
            GamePlayTagManager.Instance.AddTag(tag);
        }
        
    }
}
