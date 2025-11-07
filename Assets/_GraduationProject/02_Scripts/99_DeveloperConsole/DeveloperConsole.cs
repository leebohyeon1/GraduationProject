using BH_Lib.Log;
using System.Collections.Generic;
using System.Linq;
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

        public abstract void RunCommand(string[] args);
    }

    public class DeveloperConsole : MonoBehaviour
    {
        public static DeveloperConsole Instance { get; private set; }

        [SerializeField] private InputReader _inputReader;
        public static Dictionary<string, ConsoleCommand> Commands { get; private set; }

        private System.Text.StringBuilder consoleContent = new System.Text.StringBuilder();
        private System.Text.StringBuilder relatedSearveTermsContent = new System.Text.StringBuilder();

        [Header("UI")]
        public Canvas ConsoleCanvas;
        public TMP_Text ConsoleText;
        public TMP_Text InputText;

        public TMP_InputField ConsoleInputField;
        public GameObject RelatedServeTermsConsole;
        public TMP_Text RelatedSearveTerms;

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
                // Closing Console
                Time.timeScale = _pausedTimeScale;
                ConsoleCanvas.gameObject.SetActive(false);
            }
            else
            {
                // Opening Console
                _pausedTimeScale = Time.timeScale;
                Time.timeScale = 1;
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
            CommandClear.CreateCommand();
            CommandSceneLoad.CreateCommand();

            CommandSpawn.CreateCommand();
            CommandMonsterInit.CreateCommand();
            CommandMonsterClear.CreateCommand();
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
            consoleContent.AppendLine(msg);
            ConsoleText.text = consoleContent.ToString();
        }

        public static void AddStaticMessageToConsole(string msg)
        {
            Instance.AddMessageToConsole(msg);
        }

        public void ClearConsole()
        {
            consoleContent.Clear();
            consoleContent.AppendLine("=== Clear Console ===");
            ConsoleText.text = consoleContent.ToString();
        }

        private void ParseInput(string input)
        {
            string sanitizedInput = input.Trim().Replace("\u200B", "");
            string[] commandSplitInput = Regex.Split(sanitizedInput, @"\s+");

            if (commandSplitInput.Length == 0 || string.IsNullOrEmpty(commandSplitInput[0]))
            {
                AddMessageToConsole("=== Command not recognized ===");
                return;
            }

            if (!Commands.ContainsKey(commandSplitInput[0]))
            {
                AddMessageToConsole("=== Command not recognized ===");
            }
            else
            {
                List<string> args = commandSplitInput.ToList();

                args.RemoveAt(0);

                if(args.Contains("-help"))
                {
                    AddMessageToConsole("=================================");
                    AddMessageToConsole(Commands[commandSplitInput[0]].Description);
                    AddMessageToConsole("------------------");
                    AddMessageToConsole(Commands[commandSplitInput[0]].Help);
                    AddMessageToConsole("=================================\n");

                    return;
                }

                Commands[commandSplitInput[0]].RunCommand(args.ToArray());
            }
        }

        public void ViewRelatedSearveTerms(string input)
        {
            relatedSearveTermsContent.Clear();

            if (string.IsNullOrEmpty(input))
            {
                RelatedSearveTerms.text = consoleContent.ToString();
                RelatedServeTermsConsole.SetActive(false);   
                return;
            }

            string sanitizedInput = input.Trim().Replace("\u200B", "");
            string[] commandSplitInput = Regex.Split(sanitizedInput, @"\s+");

            foreach (KeyValuePair<string, ConsoleCommand> command in Commands)
            {
                if (command.Key.Contains(commandSplitInput[0]))
                {
                    relatedSearveTermsContent.AppendLine(command.Key);
                }
            }

            RelatedSearveTerms.text = relatedSearveTermsContent.ToString();
            
            if(!RelatedServeTermsConsole.activeSelf)
            {
                RelatedServeTermsConsole.SetActive(true);
            }
        }
    }
}
