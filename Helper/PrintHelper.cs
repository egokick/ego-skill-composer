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
            Console.WriteLine("");

            int i = 0;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Loading");
            while (i < 80)
            {
                Console.Write("."); 
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
            // sort skills
            skillSet.Skills = skillSet.Skills.OrderBy(x => x.SkillName).ToList();
            int pageSize = 15;
            int total = skillSet.Skills.Count;
            int totalPages = (total + pageSize - 1) / pageSize;
            int currentPage = 1;
            int selectionIndex = -1;

            // Print a couple blank lines to reserve our region.
            Console.WriteLine();
            Console.WriteLine();
            int regionStart = Console.CursorTop;
            int regionHeight = pageSize + 4; // fixed region height: pageSize lines + info lines
                                             // Ensure region fits in the buffer.
            int bufferHeight = Console.BufferHeight;
            if (regionStart + regionHeight > bufferHeight)
                regionStart = Math.Max(0, bufferHeight - regionHeight);

            while (true)
            {
                // Clear only our designated region.
                ClearRegion(regionStart, regionHeight);
                Console.SetCursorPosition(0, regionStart);

                int start = (currentPage - 1) * pageSize;
                int end = Math.Min(start + pageSize, total);
                for (int i = start; i < end; i++)
                {
                    Console.WriteLine($"{i + 1} - {skillSet.Skills[i].SkillName}");
                }
                Console.WriteLine();
                Console.WriteLine($"Page {currentPage} of {totalPages}");
                Console.WriteLine("Enter number to select skill or press RIGHT arrow for next page, LEFT arrow for previous page.");

                string inputBuffer = "";
                bool pageChanged = false;
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (string.IsNullOrEmpty(inputBuffer) && key.Key == ConsoleKey.RightArrow)
                    {
                        currentPage = (currentPage % totalPages) + 1;
                        pageChanged = true;
                        break;
                    }
                    else if (string.IsNullOrEmpty(inputBuffer) && key.Key == ConsoleKey.LeftArrow)
                    {
                        currentPage = (currentPage > 1) ? currentPage - 1 : totalPages;
                        pageChanged = true;
                        break;
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        if (!string.IsNullOrEmpty(inputBuffer))
                        {
                            if (int.TryParse(inputBuffer, out int num) && num >= start + 1 && num <= end)
                            {
                                selectionIndex = num - 1;
                                break;
                            }
                            else
                            {
                                Console.SetCursorPosition(0, regionStart + regionHeight);
                                Console.WriteLine("Invalid selection. Press any key to retry.");
                                Console.ReadKey(true);
                                pageChanged = true;
                                break;
                            }
                        }
                    }
                    else if (char.IsDigit(key.KeyChar))
                    {
                        inputBuffer += key.KeyChar;
                        Console.Write(key.KeyChar);
                    }
                    else if (key.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
                    {
                        inputBuffer = inputBuffer.Substring(0, inputBuffer.Length - 1);
                        Console.Write("\b \b");
                    }
                }

                if (pageChanged)
                    continue;
                if (selectionIndex >= 0 && selectionIndex < total)
                    break;
            }

            var selected = skillSet.Skills[selectionIndex];
            Console.WriteLine("\nSelected skill: " + selected.SkillName);
            Console.WriteLine(selected?.Description ?? "");
            return selected;
        }

        private static void ClearRegion(int startLine, int height)
        {
            int width = Console.WindowWidth;
            int availableLines = Console.BufferHeight - startLine;
            int effectiveHeight = Math.Min(height, availableLines);
            for (int i = 0; i < effectiveHeight; i++)
            {
                Console.SetCursorPosition(0, startLine + i);
                Console.Write(new string(' ', width));
            }
            Console.SetCursorPosition(0, startLine);
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
