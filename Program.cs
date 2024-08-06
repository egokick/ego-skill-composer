using System.Text;  
using Newtonsoft.Json;
using skill_composer.Helper;
using skill_composer.Models;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task; 

namespace skill_composer
{
    internal class Program
    {
        public static Settings _settings = new Settings();
        public static ApiHandler api = null;

        static void Main(string[] args)
        {
            PrintHelper.PrintLogo();

            _settings = SettingsHelper.Load();

            api = new ApiHandler();

            while (true)
            {
                var skillFilePath = FilePathHelper.GetSkillFilePath();

                var skillsJson = File.ReadAllText(skillFilePath);

                var skillSet = JsonConvert.DeserializeObject<SkillSet>(skillsJson);

                PrintHelper.PrintIntroduction();

                var skill = PrintHelper.SelectSkill(skillSet);

                ProcessSkill(skill);                             
            }
        }

        public static void ProcessSkill(Skill skill)
        {
            if (api is null) api = new ApiHandler();

            Skill selectedSkill = new Skill() { RepeatCount = skill.RepeatCount };            

            while (true)
            {
                selectedSkill = SetUserResponseFromPreviousIteration(selectedSkill, skill);
                if(selectedSkill.RepeatCount != -1) selectedSkill.RepeatCount--;

                for (int i = 0; i < selectedSkill.Tasks.Count; i++)
                {
                    var task = selectedSkill.Tasks[i];

                    PrintHelper.PrintTaskName(task);

                    try
                    {
                        task = ProcessTask(task, selectedSkill).Result;
                    }
                    catch (Exception ex) 
                    {
                        task.Output += ex.Message;
                        var defaultColor = Console.ForegroundColor; 
                        Console.ForegroundColor = ConsoleColor.Red; 
                        Console.WriteLine($"Exception: {ex.Message}{ex.StackTrace}"); 
                        Console.ForegroundColor = defaultColor;
                    }

                    if (task.HaltProcessing is not null && task.HaltProcessing == true)
                    {
                        selectedSkill.RepeatCount = 0;
                        break;
                    }
                    
                    PrintHelper.PrintTaskOutput(task);
                }

                if (selectedSkill.DisableFileLogging is null || selectedSkill.DisableFileLogging == false) FilePathHelper.WriteSkillToFile(selectedSkill);
                
                if (selectedSkill.RepeatCount == 0) break;
            }
        }

        public static async Task<Models.Task> ProcessTask(Models.Task task, Skill selectedSkill)
        {
            // Inserts outputs from previous steps into the current step
            task = ReplaceInput(task, selectedSkill);

            // If there is a UserPrompt and the user has not already responded i.e. in the second iteration, we already have the user answers from the first iteration
            if (task.Mode.ToLower() == "user" && !string.IsNullOrEmpty(task.Input) && string.IsNullOrEmpty(task.Output))
            {
                Console.WriteLine(task.Input);

                task.Output = Console.ReadLine(); // ReadInputAsync().Result;
                task.Output = task.Output.TrimEnd();
            }

            if (task.Mode.ToLower() == "ai")
            {                
                task.Output = await api.GetAIResponse(userInput: task.Input, filePath: task.FilePath, skill : selectedSkill);                
            }

            if (!string.IsNullOrEmpty(task.SpecialAction))
            {
                var specialActions = task.SpecialAction.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var actionName in specialActions)
                {
                    var action = SpecialActionRegistry.GetAction(actionName.Trim());

                    task = await action.Execute(task, selectedSkill);
                }
            }

            return task;
        }

        public static Skill SetUserResponseFromPreviousIteration(Skill skill, Skill templateSkill)
        {
            // Clone the templateSkill and update specific properties
            var newSkill = new Skill
            {
                SkillName = templateSkill.SkillName,
                Temperature = templateSkill.Temperature,
                OpenAiModel = templateSkill.OpenAiModel,
                DisableFileLogging = templateSkill.DisableFileLogging,
                RepeatCount = skill.RepeatCount,
                AppendFileLogging = templateSkill.AppendFileLogging,
                // Create a new list of tasks, updating UserResponse from the original skill where applicable
                Tasks = templateSkill.Tasks.Select(t =>
                {
                    var correspondingTask = skill.Tasks?.FirstOrDefault(st => st.Number == t.Number);

                    // Determine the Input based on the Mode
                    string output = (t.Mode.ToLower() == "user" && correspondingTask != null) ? correspondingTask.Output : t.Output;

                    return new Models.Task
                    {
                        // Copy all properties from the template task and update based on the logic provided
                        Number = t.Number,
                        BestOf = t.BestOf,
                        Name = t.Name,
                        Input = t.Input, 
                        Output = output, // Updated based on the Mode
                        Mode = t.Mode,
                        SpecialAction = t.SpecialAction,
                        FilePath = t.FilePath,
                        PrintOutput = t.PrintOutput, 
                        AiResponseShared = t.AiResponseShared,
                        UserResponseShared = t.UserResponseShared,
                        cancellationToken = t.cancellationToken                       
                    };
                }).ToList()
            };

            return newSkill;
        }

        static async Task<string> ReadInputAsync()
        {
            var inputBuilder = new StringBuilder();

            // Synchronously read the first line to ensure immediate input
            string firstLine = Console.ReadLine();
            inputBuilder.AppendLine(firstLine);

            while (true)
            {
                var inputTask = Task.Run(() => Console.ReadLine());
                var completedTask = await Task.WhenAny(inputTask, Task.Delay(1000));
                if (completedTask == inputTask)
                {
                    // Input was received
                    var input = await inputTask; // Ensure we get the result from the inputTask
                    inputBuilder.AppendLine(input); // Append all lines, including empty ones
                }
                else
                {
                    // Check if the input task completed just after the delay, before breaking
                    if (inputTask.IsCompleted)
                    {
                        var lastInput = await inputTask;
                        inputBuilder.AppendLine(lastInput);
                    }
                    // Timeout occurred (no input for 1000 ms), end input reading
                    break;
                }
            }

            return inputBuilder.ToString(); // Use TrimEnd to clean up any trailing new lines
        }

        public static Models.Task ReplaceInput(Models.Task task, Skill selectedSkill)
        {
            var outputRegex = new Regex(@"\{\{Output\[(\d+)\]\}\}");
            var filePathRegex = new Regex(@"\{\{FilePath\[(\d+)\]\}\}");
            var inputRegex = new Regex(@"\{\{Input\[(\d+)\]\}\}");
            var dateRegex = new Regex(@"\{\{Date\}\}");

            if (!string.IsNullOrEmpty(task.Input))
            {
                task.Input = outputRegex.Replace(task.Input, match =>
                {
                    int index = int.Parse(match.Groups[1].Value) - 1;
                    if (index >= 0 && index < selectedSkill.Tasks.Count)
                    {
                        return selectedSkill.Tasks[index].Output;
                    }
                    else
                    {
                        return "";
                    }
                });

                // Add FilePath to Input
                task.Input = filePathRegex.Replace(task.Input, match =>
                {
                    int index = int.Parse(match.Groups[1].Value) - 1;
                    if (index >= 0 && index < selectedSkill.Tasks.Count)
                    {
                        return selectedSkill.Tasks[index].FilePath;
                    }
                    else
                    {
                        return "";
                    }
                });

                // Add Input replacement
                task.Input = inputRegex.Replace(task.Input, match =>
                {
                    int index = int.Parse(match.Groups[1].Value) - 1;
                    if (index >= 0 && index < selectedSkill.Tasks.Count)
                    {
                        return selectedSkill.Tasks[index].Input;
                    }
                    else
                    {
                        return "";
                    }
                }); 

                // Add Date replacement
                task.Input = dateRegex.Replace(task.Input, DateTime.Now.ToString("yyyy-MM-dd")); 
            }

            if (!string.IsNullOrEmpty(task.FilePath))
            {
                // Add FilePath from previous steps to this steps filePath
                task.FilePath = filePathRegex.Replace(task.FilePath, match =>
                {
                    int index = int.Parse(match.Groups[1].Value) - 1;
                    if (index >= 0 && index < selectedSkill.Tasks.Count)
                    {
                        return selectedSkill.Tasks[index].FilePath;
                    }
                    else
                    {
                        return "";
                    }
                });
            }

            return task;
        }
    }
}
