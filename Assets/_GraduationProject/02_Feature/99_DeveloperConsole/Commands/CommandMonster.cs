using System;
using UnityEngine;

namespace Console
{
    public class CommandMonster : ConsoleCommand
    {
        public override string Name { get; protected set; }
        public override string Command { get; protected set; }
        public override string Description { get; protected set; }
        public override string Help { get; protected set; }

        public CommandMonster()
        {
            Name = "Monster";
            Command = "monster_init";
            Description = "Monster Initialize";
            Help = "Usage: Monster Init";

            AddCommandToConsole();
        }

        public override void RunCommand(string[] args)
        {
            Enemy[] allEnemies = GameObject.FindObjectsOfType<Enemy>();
            if (allEnemies.Length == 0)
            {
                DeveloperConsole.AddStaticMessageToConsole("씬에서 몬스터를 찾을 수 없습니다.");
                return;
            }

            int initializedCount = 0;

            foreach (Enemy enemy in allEnemies)
            {
                enemy.Init();
                initializedCount++;
                
            }

            DeveloperConsole.AddStaticMessageToConsole($"{initializedCount}마리의 몬스터 AI를 초기화했습니다.");
        }
        public static CommandMonster CreateCommand()
        {
            return new CommandMonster();
        }
    }
}