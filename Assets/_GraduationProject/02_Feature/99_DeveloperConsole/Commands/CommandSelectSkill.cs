using System;
using UnityEditor;
using UnityEngine;

namespace Console
{
    public class CommandSelectSkill : ConsoleCommand
    {
        public override string Name { get; protected set; }
        public override string Command { get; protected set; }
        public override string Description { get; protected set; }
        public override string Help { get; protected set; }

        public CommandSelectSkill() 
        {
            Name = "SelectSkill";
            Command = "select-skill";
            Description = "Select a skill";
            Help = "First- you must unlock the skill.\n" + 
                "Usage: select-skill <SkillType> \n" + 
                "SkillType - Flash, Boost, TimeStop";

            AddCommandToConsole();
        }

        public override void RunCommand(string[] args)
        {
            PlayerSkill skill = GameObject.FindFirstObjectByType<PlayerSkill>();
            if (skill == null)
            {
                DeveloperConsole.AddStaticMessageToConsole("PlayerSkill component could not be found.");
                return; // 메서드 실행 중단
            }

            if (args.Length == 0)
            {
                DeveloperConsole.AddStaticMessageToConsole($"{Help}");
                return;
            }


            if (!Enum.TryParse<SkillType>(args[0], true, out SkillType skillType))
            {
                DeveloperConsole.AddStaticMessageToConsole($"'{args[0]}' is not a valid skill name.");
                return;
            }

            if (!skill.SkillData.IsMainSkillsUnlock[(int)skillType])
            {
                DeveloperConsole.AddStaticMessageToConsole("=================");
                DeveloperConsole.AddStaticMessageToConsole($"{Help}");
            }

            skill.UnlockSkill(skillType);
        }

        public static CommandSelectSkill CreateCommand()
        {
            return new CommandSelectSkill();
        }
    }

}
