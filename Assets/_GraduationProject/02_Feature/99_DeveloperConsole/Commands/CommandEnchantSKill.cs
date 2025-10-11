using System;
using UnityEditor;
using UnityEngine;

namespace Console
{
    public class CommandEnchantSkill : ConsoleCommand
    {
        public override string Name { get; protected set; }
        public override string Command { get; protected set; }
        public override string Description { get; protected set; }
        public override string Help { get; protected set; }

        public CommandEnchantSkill()
        {
            Name = "EnchantSkill";
            Command = "enchant-skill";
            Description = "Enchants a specified skill.";
            Help = "Usage: enchant-skill <SkillType> [Level] \n" +
                "SkillType - Flash, Boost, TimeStop \n" + 
                "Level - 0 ~ 2";

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

            // 2. 명령어 인자 개수 확인
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

            if (args.Length > 1)
            {
                // 5. Use int.TryParse to safely check if the argument is a valid number.
                if (!int.TryParse(args[1].Trim(), out int skillLevel))
                {
                    DeveloperConsole.AddStaticMessageToConsole($"'{args[1]}' is not a valid level (number).");
                    return;
                }

                // Only enchant the skill if parsing was successful.
                skill.EnchantSkill(skillType, skillLevel);
                DeveloperConsole.AddStaticMessageToConsole($"Enchanted skill {skillType} to level {skillLevel}.");
            }
            else
            {
                // This block runs if no level argument is provided.
                skill.EnchantSkill(skillType);
                DeveloperConsole.AddStaticMessageToConsole($"Enchanted skill {skillType} with default value.");
            }

        }

        public static CommandEnchantSkill CreateCommand()
        {
            return new CommandEnchantSkill();
        }
    }

}
