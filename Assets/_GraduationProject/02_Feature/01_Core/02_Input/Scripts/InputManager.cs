using UnityEngine;
using BH_Lib.DI;
using BH_Lib.AssetManager;
using System.Threading.Tasks;

public enum ActionMapType
{
    Player = 0,
    UI = 1
}

[Register(LifetimeScope.Singleton)]
public class InputManager : DIMonoBehaviour
{
    [SerializeField] private InputReader _inputReader;

    [Inject] private AssetManager _assetManager;
    [Inject] private UIManager _uiManager;

    protected override async void OnEnable()
    {
        base.OnEnable();

        if(_inputReader == null)
        {
            _inputReader = await _assetManager.LoadAssetAsync<InputReader>("InputReader", gameObject);            
        }

        _uiManager.OnOpenPopUpUI += HandleOpenPopUpUI;
        _uiManager.OnClearPopUpUI += HandleClearPopUpUI;    
    }

    private void OnDisable()
    {
        _uiManager.OnOpenPopUpUI -= HandleOpenPopUpUI;
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
