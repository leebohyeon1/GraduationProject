using BH_Lib.DI;
using BH_Lib.Log;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

namespace DeveloperConsole.Commands
{
    [Register]
    public class DeveloperConsoleBehaviour : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private string _prefix = string.Empty;
        [SerializeField] private ConsoleCommand[] _commands = new ConsoleCommand[0];

        [Header("UI")]
        [SerializeField] private GameObject _uiCanvas = null;
        [SerializeField] private TMP_InputField _inputField = null;

        private float _pausedTimeScale;
        
        private static DeveloperConsoleBehaviour _instance;

        private DeveloperConsole _developerConsole;
        private DeveloperConsole DeveloperConsole
        {
            get
            {
                if(_developerConsole != null)
                {
                    return _developerConsole;
                }

                return _developerConsole = new DeveloperConsole(_prefix, _commands);
            }
        }

        private void OnEnable()
        {
            _inputReader.ToggleConsoleEvent += OnToggleConsole;
        }

        private void OnDisable()
        {
            _inputReader.ToggleConsoleEvent -= OnToggleConsole;
        }

        public void OnToggleConsole()
        {
            Log.Print("콘솔 토글");

            if (_uiCanvas.activeSelf)
            {
                Time.timeScale = _pausedTimeScale;
                _uiCanvas.SetActive(false);
            }
            else
            {
                _pausedTimeScale = Time.timeScale;
                Time.timeScale = 0;
                _uiCanvas.SetActive(true);
                _inputField.ActivateInputField();
            }
        }

        public void ProcessCommand(string inputValue)
        {
            DeveloperConsole.ProcessCommand(inputValue);

            _inputField.text = string.Empty;
        }


    }
}
