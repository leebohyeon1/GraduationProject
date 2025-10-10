using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Console
{
    public abstract class ConsoleCommand
    {
        public abstract string Name { get; protected set; }
        public abstract string Command { get; protected set; }
        public abstract string Description { get; protected set; }
        public abstract string Help { get; protected set; }

        public void AddCommandToConsole()
        {
            string addMessage = " command has been added to the console";

            DeveloperConsole.AddCommandsToConsole(Command, this);
            DeveloperConsole.AddStaticMessageToConsole(Name +  addMessage); 
        }

        public abstract void RunCommand();
    }

    public class DeveloperConsole : MonoBehaviour
    {
        public static DeveloperConsole Instance { get; private set; }

        [SerializeField] private InputReader _inputReader;
        public static Dictionary<string, ConsoleCommand> Commands { get; private set; }

        [Header("UI")]
        public Canvas ConsoleCanvas;
        public TMP_Text ConsoleText;
        public TMP_Text InputText;
        public TMP_InputField ConsoleInputField;

        private float _pausedTimeScale;

        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

           Commands = new Dictionary<string, ConsoleCommand>();
        }

        private void OnEnable()
        {
            _inputReader.ToggleConsoleEvent += OnToggleConsole;
            _inputReader.EnterEvent += OnEnter;
        }

        private void Start()
        {
            ConsoleCanvas.gameObject.SetActive(false);
            CreateCommands();
        }

        private void OnDisable()
        {
            _inputReader.ToggleConsoleEvent -= OnToggleConsole;
            _inputReader.EnterEvent -= OnEnter;
        }
        
        #region Input
        private void OnToggleConsole()
        {
            if (ConsoleCanvas.gameObject.activeSelf)
            {
                Time.timeScale = _pausedTimeScale;
                ConsoleCanvas.gameObject.SetActive(false);
            }
            else
            {
                _pausedTimeScale = Time.timeScale;
                Time.timeScale = 0;
                ConsoleCanvas.gameObject.SetActive(true);
                ConsoleInputField.ActivateInputField();
            }
        }

        private void OnEnter()
        {
            AddMessageToConsole(InputText.text);
            ParseInput(InputText.text);
        }
        #endregion

        private void CreateCommands()
        {
            CommandQuit.CreateCommand();
        }

        public static void AddCommandsToConsole(string name, ConsoleCommand command)
        {
            if(!Commands.ContainsKey(name))
            {
                Commands.Add(name, command);
            }
        }

        private void AddMessageToConsole(string msg)
        {
            ConsoleText.text += msg + "\n";
        }

        public static void AddStaticMessageToConsole(string msg)
        {
            DeveloperConsole.Instance.AddMessageToConsole(msg);
        }

        private void ParseInput(string input)
        {
            string trimmedInput = input.Trim();
            if (string.IsNullOrEmpty(trimmedInput))
            {
                AddMessageToConsole("Command not recognized.");
                return;
            }

            string commandName = trimmedInput;
            int spaceIndex = trimmedInput.IndexOf(' ');
            if (spaceIndex != -1)
            {
                commandName = trimmedInput.Substring(0, spaceIndex);
            }
            
            // Aggressively remove any non-printable/non-ASCII characters
            commandName = Regex.Replace(commandName, @"[^ -~]", "");

            if (!Commands.ContainsKey(commandName))
            {
                AddMessageToConsole("Command not recognized.");
            }
            else
            {
                Commands[commandName].RunCommand();
            }
        }
    }
}
