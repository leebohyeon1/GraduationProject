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
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputReader _inputReader;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private async void Start()
    {
        if(_inputReader == null)
        {
            _inputReader = await Addressables.LoadAssetAsync<InputReader>("InputReader").Task;            
        }

        UIManager.Instance.OnOpenFirstPopUpUI += HandleOpenPopUpUI;
        UIManager.Instance.OnClearPopUpUI += HandleClearPopUpUI;    
    }

    private void OnDestroy()
    {
        UIManager.Instance.OnOpenFirstPopUpUI -= HandleOpenPopUpUI;
        UIManager.Instance.OnClearPopUpUI -= HandleClearPopUpUI;
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
