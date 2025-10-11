using UnityEditor;
using UnityEngine;

namespace Console
{
    public class CommandQuit : ConsoleCommand
    {
        public override string Name { get; protected set; }
        public override string Command { get; protected set; }
        public override string Description { get; protected set; }
        public override string Help { get; protected set; }

        public CommandQuit() 
        {
            Name = "Quit";
            Command = "quit";
            Description = "Quits the application";
            Help = "Use this command with no arguements to force Unity to quit!";

            AddCommandToConsole();
        }

        public override void RunCommand(string[] args)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif  
        }

        public static CommandQuit CreateCommand()
        {
            return new CommandQuit();
        }
    }

}
