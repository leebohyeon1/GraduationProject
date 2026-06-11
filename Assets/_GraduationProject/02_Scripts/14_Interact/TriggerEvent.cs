using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    public UnityEvent ColliderEnter, ColliderExit;
    public UnityEvent TriggerEnter, TriggerExit;

    private void OnCollisionEnter(Collision collision)
    {
        ColliderEnter.Invoke();
    }

    private void OnCollisionExit(Collision collision)
    {
        ColliderExit.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerExit.Invoke();
    }
}
