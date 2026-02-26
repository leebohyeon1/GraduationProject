using MoreMountains.Feedbacks;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] MMF_Player _goToAFeedback;
    [SerializeField] MMF_Player _goToBFeedback;

    private bool _isAtA = true;

    private void Start()
    {
        _goToAFeedback.PlayFeedbacks();
        _isAtA = true;
    }

    public void Move()
    {
        if (_isAtA)
        {
            _goToBFeedback.PlayFeedbacks();
            _isAtA = false;
        }
        else
        {
            _goToAFeedback.PlayFeedbacks();
            _isAtA = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        other.transform.SetParent(transform);
    }

    private void OnTriggerExit(Collider other)
    {
        other.transform.SetParent(null);
    }
}
