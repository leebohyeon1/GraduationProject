using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Console
{
    public class CommandSpawn : ConsoleCommand
    {
        public override string Name { get; protected set; }
        public override string Command { get; protected set; }
        public override string Description { get; protected set; }
        public override string Help { get; protected set; }

        public CommandSpawn()
        {
            Name = "Spawn";
            Command = "Spawn";
            Description = "Spawns a specified number of monsters of a given type.";
            Help = "Usage: Spawn <MonsterType> [Count] \n" +
                "MonsterType - Brave, Coward, Cunning \n" +
                "Count - Number of monsters to spawn (default is 1)";

            AddCommandToConsole();
        }

        public override async void RunCommand(string[] args)
        {

            if (args.Length == 0)
            {
                DeveloperConsole.AddStaticMessageToConsole($"{Help}");
                return;
            }
            if (args.Length % 2 != 0)
            {
                DeveloperConsole.AddStaticMessageToConsole("Error: Monster name and count must be in pairs.");
                DeveloperConsole.AddStaticMessageToConsole("Example: spawn Brave 2 Cunning 1");
                return;
            }

            try
            {
                var spawnController = FindFirstObjectByType<MonsterSpawnController>();
                if(spawnController == null)
                {
                    spawnController = new GameObject().AddComponent<MonsterSpawnController>();
                }

                for (int i = 0; i < args.Length; i += 2)
                {
                    string monsterName = args[i];
                    string countArg = args[i + 1];
                    var locations = await Addressables.LoadResourceLocationsAsync(monsterName, typeof(GameObject)).Task;
                    if (locations.Count == 0)
                    {
                        DeveloperConsole.AddStaticMessageToConsole($"Error: '{monsterName}' is not a valid monster type.");
                        continue;
                    }

                    if (!int.TryParse(countArg, out int count) || count < 1)
                    {
                        DeveloperConsole.AddStaticMessageToConsole($"'{countArg}' is not a valid count (must be a positive number).");
                        return;
                    }
                    if (spawnController == null)
                    {
                        Debug.LogError("CommandSpawn: spawnController가 null입니다! [Inject] 실패!");
                        DeveloperConsole.AddStaticMessageToConsole("Error: Spawn controller is not initialized.");
                        return; // try 블록 실행 전에 즉시 종료
                    }
                    await spawnController.SpawnEnemies(monsterName, count);
                    DeveloperConsole.AddStaticMessageToConsole($"Spawned {count} {monsterName} monster(s).");
                }
            }
            catch (Exception e)
            {
                DeveloperConsole.AddStaticMessageToConsole($"Error executing Spawn command: {e.Message}");
            }


        }
        public static CommandSpawn CreateCommand()
        {
            return new CommandSpawn();
        }
    }

}