using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public enum ActionMapType
{
    Player = 0,
    UI = 1
}

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;

    private UIManager _uiManager;

    private async void OnEnable()
    {
        if(_inputReader == null)
        {
            _inputReader = await Addressables.LoadAssetAsync<InputReader>("InputReader").Task;            
        }

        _uiManager.OnOpenFirstPopUpUI += HandleOpenPopUpUI;
        _uiManager.OnClearPopUpUI += HandleClearPopUpUI;    
    }

    private void OnDisable()
    {
        _uiManager.OnOpenFirstPopUpUI -= HandleOpenPopUpUI;
        _uiManager.OnClearPopUpUI -= HandleClearPopUpUI;
    }

    public void ChangeActionMap(ActionMapType type)
    {
        switch (type)
        {
            case ActionMapType.Player:
                _inputReader.DisableUIActions();
                _inputReader.EnablePlayerActions();
                break; 
            case ActionMapType.UI:
                _inputReader.DisablePlayerActions();
                _inputReader.EnableUIActions();
                break;
        }
    }

    public void HandleOpenPopUpUI()
    {
        ChangeActionMap(ActionMapType.UI);
    }

    public void HandleClearPopUpUI() 
    {
        ChangeActionMap(ActionMapType.Player);
    }
}
