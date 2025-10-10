using UnityEngine;

namespace DeveloperConsole.Commands
{
    public interface IConsoleCommand
    {
        string CommandWord { get; }
        bool Process(string[] args);
    }

}
