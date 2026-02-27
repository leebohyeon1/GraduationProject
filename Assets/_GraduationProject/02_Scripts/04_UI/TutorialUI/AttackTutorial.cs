using UnityEngine;

public class AttackTutorial : MonoBehaviour
{
    private bool _isTutorial = false;
    public GameObject tutorialUI;

    public InputReaderSO inputReader;

    private void OnTriggerEnter(Collider other)
    {
        if(_isTutorial)
        {
            return;
        }

        if(other.TryGetComponent(out PlayerController player))
        {
            _isTutorial = true;

            inputReader.InteractHoldEvent += CloseTutorial;
            tutorialUI.SetActive(true);
        }
    }

    public void CloseTutorial()
    {
        tutorialUI.SetActive(false);
        inputReader.InteractHoldEvent -= CloseTutorial;
    }
}
