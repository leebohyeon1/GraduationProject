using UnityEngine;

public class InteractButton : MonoBehaviour
{
    [SerializeField] private InteractableObject _interactable;
    [SerializeField] private GameObject _buttonImage;    


    private void OnEnable()
    {
        _buttonImage.SetActive(false);
        _interactable.OnPlayerScan += HandlePlayerScan;
    }

    private void OnDisable()
    {
        _interactable.OnPlayerScan -= HandlePlayerScan;
    }

    private void HandlePlayerScan(bool isScan)
    {
        _buttonImage.SetActive(isScan);
    }
}
