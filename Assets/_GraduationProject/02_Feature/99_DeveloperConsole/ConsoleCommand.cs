using UnityEngine;


namespace DeveloperConsole.Commands
{
    public abstract class ConsoleCommand : ScriptableObject, IConsoleCommand
    {
        [SerializeField] private string _commandWord = string.Empty;

        public string CommandWord => _commandWord;

        public abstract bool Process(string[] args);
    }
}