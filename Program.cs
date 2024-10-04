using Newtonsoft.Json;
using skill_composer.Helper;
using skill_composer.Models;
using System.Text.RegularExpressions;

namespace skill_composer
{
    internal class Program
    {        
        public static ApiHandler api = null;

        static void Main(string[] args)
        {
            PrintHelper.PrintLogo();

            var filePath = FilePathHelper.GetSettingsFile();

            Settings.LoadSettings(filePath);
            
            api = new ApiHandler();

            while (true)
            {
                var skillFilePath = FilePathHelper.GetSkillFilePath();

                var skillsJson = File.ReadAllText(skillFilePath);

                var skillSet = JsonConvert.DeserializeObject<SkillSet>(skillsJson);

                PrintHelper.PrintIntroduction();

                var skill = PrintHelper.SelectSkill(skillSet);

                _ = ProcessSkill(skill);                             
            }
        }

        public static string ProcessSkill(Skill skill)
        {
            var output = "";
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

                    output = task.Output;

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

            return output;
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

        public static Models.Task ReplaceInput(Models.Task task, Skill selectedSkill)
        {
            var outputRegex = new Regex(@"\{\{Output\[(\d+)\]\}\}", RegexOptions.IgnoreCase);
            var outputPropertyRegex = new Regex(@"\{\{Output\[(\d+)\]\.(\w+)\}\}", RegexOptions.IgnoreCase);
            var filePathRegex = new Regex(@"\{\{FilePath\[(\d+)\]\}\}", RegexOptions.IgnoreCase);
            var inputRegex = new Regex(@"\{\{Input\[(\d+)\]\}\}", RegexOptions.IgnoreCase);
            var dateRegex = new Regex(@"\{\{Date\}\}", RegexOptions.IgnoreCase);

            if (!string.IsNullOrEmpty(task.Input))
            {
                // Process {{Output[n].Property}} first
                task.Input = outputPropertyRegex.Replace(task.Input, match =>
                {
                    int index = int.Parse(match.Groups[1].Value) - 1;
                    string propertyName = match.Groups[2].Value;

                    if (index >= 0 && index < selectedSkill.Tasks.Count)
                    {
                        string outputJson = selectedSkill.Tasks[index].Output;
                        try
                        {
                            var jsonObject = System.Text.Json.JsonDocument.Parse(outputJson).RootElement;
                            if (jsonObject.TryGetProperty(propertyName, out var propertyValue))
                            {
                                return propertyValue.ToString();
                            }
                        }
                        catch (System.Text.Json.JsonException)
                        {
                            // If the output is not valid JSON, return the original match
                            return match.Value;
                        }
                    }

                    return match.Value;
                });

                // Process {{Output[n]}} (without property access)
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
                // Add FilePath from previous steps to this step's filePath
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

    }
}