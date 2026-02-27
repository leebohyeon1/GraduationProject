using UnityEngine;

public class HelpTutorial : MonoBehaviour
{
    public GameObject tutorialUI;

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out PlayerController player))
        {
            tutorialUI.SetActive(false);
        }
    }
}
