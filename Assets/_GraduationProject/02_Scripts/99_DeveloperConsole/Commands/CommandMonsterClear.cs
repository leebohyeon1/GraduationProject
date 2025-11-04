using System;
using UnityEngine;

namespace Console
{
    public class CommandMonsterClear : ConsoleCommand
    {
        public override string Name { get; protected set; }
        public override string Command { get; protected set; }
        public override string Description { get; protected set; }
        public override string Help { get; protected set; }

        public CommandMonsterClear()
        {
            Name = "Monster";
            Command = "monster_clear";
            Description = "Monster Clear";
            Help = "Usage: Monster Clear";

            AddCommandToConsole();
        }

        public override void RunCommand(string[] args)
        {
            Enemy[] allEnemies = GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            if (allEnemies.Length == 0)
            {
                DeveloperConsole.AddStaticMessageToConsole("씬에서 몬스터를 찾을 수 없습니다.");
                return;
            }

            foreach (Enemy enemy in allEnemies)
            {
                GameObject.Destroy(enemy.gameObject);
            }
        }
        public static CommandMonsterClear CreateCommand()
        {
            return new CommandMonsterClear();
        }
    }
}