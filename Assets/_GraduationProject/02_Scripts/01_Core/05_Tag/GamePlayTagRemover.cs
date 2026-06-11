using System.Collections.Generic;
using UnityEngine;

public class GamePlayTagRemover : MonoBehaviour
{
    [SerializeField] private List<GamePlayTagSO> _removeTags;

    public void Remove()
    {
        foreach (GamePlayTagSO tag in _removeTags)
        {
            GamePlayTagManager.Instance.RemoveTag(tag);
        }
    }

}
