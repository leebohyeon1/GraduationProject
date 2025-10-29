using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Console
{
    public class CommandSceneLoad : ConsoleCommand
    {
        public override string Name { get; protected set; }
        public override string Command { get; protected set; }
        public override string Description { get; protected set; }
        public override string Help { get; protected set; }

        public CommandSceneLoad()
        {
            Name = "SceneLoad";
            Command = "scene-load";
            Description = "Load Scene by Index";
            Help = "Usage: scene-load / scene-load [Index]\n" +
                "'scene-load' loads only the current scene.";

            AddCommandToConsole();
        }

        public override void RunCommand(string[] args)
        {
            if (args.Length == 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
            else
            {
                // 인자 2개 이상일 때: scene 인덱스 시도
                if (int.TryParse(args[1].Trim(), out int sceneIndex))
                {
                    SceneManager.LoadScene(sceneIndex);
                }
                else
                {
                    DeveloperConsole.AddStaticMessageToConsole($"{Help}");
                }
            }

        }

        public static CommandSceneLoad CreateCommand()
        {
            return new CommandSceneLoad();
        }
    }

}
