using skill_composer.Models;
using System.Text;

namespace skill_composer.Helper
{
    public static class PrintHelper
    {
        public static void PrintLogo()
        {
            Console.OutputEncoding = Encoding.UTF8;

            var defaultColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(AsciiArt.Logo);
            Console.ForegroundColor = defaultColor;
            Console.WriteLine();
        }

        public static void PrintIntroduction()
        {
            var defaultColor = Console.ForegroundColor;

            Console.WriteLine("Each skill uses AI to achieve a task, enter the number of the skill to run it");
            Console.WriteLine("各スキルはAIを使ってタスクを達成します。実行するスキルの番号を入力してください。");
            Console.WriteLine("Cada habilidad utiliza IA para realizar una tarea, ingresa el número de la habilidad para ejecutarla.");
            Console.WriteLine("每项技能都使用人工智能来完成任务，请输入技能的编号以运行它。");
            Console.WriteLine("");

            int i = 0;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Loading");
            while (i < 80)
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(4);
                i++;
            }
            // Move the cursor back to the start of the line
            Console.SetCursorPosition(0, Console.CursorTop);

            // Clear the line by overwriting it with spaces. Ensure not to exceed the console width.
            Console.Write(new string(' ', Console.WindowWidth - 1));

            // Move the cursor back to the start of the line again
            Console.SetCursorPosition(0, Console.CursorTop);

            // Write the final message
            Console.Write("........................................................................................Ok!");


            Console.ForegroundColor = defaultColor;
        }

        public static Skill SelectSkill(SkillSet skillSet)
        {
            Console.WriteLine("");

            skillSet.Skills = skillSet.Skills.OrderBy(x => x.SkillName).ToList();

            int j = 0;
            foreach (var skill in skillSet.Skills)
            {
                j++;
                Console.WriteLine($"{j} - {skill.SkillName}");
            }

            var selection2 = "";
            int si = 0;

            while (si == 0)
            {
                selection2 = Console.ReadLine();
                int.TryParse(selection2, out si);
            }

            // This is used to overwrite 
            var skillTemplate = skillSet.Skills[si - 1];
            Console.WriteLine("Selected skill: " + skillTemplate.SkillName);
            Console.WriteLine(skillTemplate?.Description ?? "");

            return skillTemplate;
        }

        public static void PrintTaskOutput(Models.Task task)
        {
            if (task.PrintOutput is not null && task.PrintOutput == false) return;

            if (task.Mode.ToLower() == "user") return; // don't print the text the user just entered
            //if (string.IsNullOrEmpty(task.Input)) return;

            if (task.HaltProcessing is not null && task.HaltProcessing == true)
            {
                Console.WriteLine("No more files to process in input folder");
                return;
            }

            Console.WriteLine(task.Output);
            var currentForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine("=========================="); 
            Console.ForegroundColor = currentForegroundColor;
        }
        public static void PrintTaskName(Models.Task task)
        {
            var currentForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine("==========================");
            Console.WriteLine(task.Name);
            Console.ForegroundColor = currentForegroundColor;
        }

    }
}
