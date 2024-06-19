using System.Text; 
using Flurl.Http;
using Newtonsoft.Json;
using skill_composer.Helper;
using skill_composer.Models;
using System.Text.RegularExpressions;
using Flurl.Util;
using Polly;
using Task = System.Threading.Tasks.Task;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Wave; // Make sure to include this namespace
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Net.WebSockets;
using skill_composer.SpecialActions;

namespace skill_composer
{
    internal class Program
    {
        public static Settings _settings = new Settings();

        static void Main(string[] args)
        {            
            var settingsJson = File.ReadAllText(FilePathHelper.GetSettingsFile());

            var settings = JsonConvert.DeserializeObject<Settings>(settingsJson);           

            _settings = FilePathHelper.InitializeSettings(settings);

            while (true)
            {
                var skillFilePath = FilePathHelper.GetSkillFile();

                var skillsJson = File.ReadAllText(skillFilePath);

                var skillSet = JsonConvert.DeserializeObject<SkillSet>(skillsJson);

                Console.WriteLine("");
                Console.WriteLine("1 - AI Skill Generation");
                Console.WriteLine("2 - Load/Debug Previous Output");

                var selection1 = Console.ReadLine();
                int.TryParse(selection1, out int s1);

                switch (s1)
                {
                    case 1:
                        AISkillGeneration(skillSet);
                        break;
                    case 2:
                        DebugPreviousOutput(skillSet);
                        break;
                }                  
            }
        }

        public static void AISkillGeneration(SkillSet skillSet)
        {
            Console.WriteLine("Select Skill by number: ");
            Console.WriteLine("");

            skillSet.Skills = skillSet.Skills.OrderBy(x => x.SkillName).ToList();

            int j = 0;
            foreach (var skill in skillSet.Skills)
            {
                j++;
                Console.WriteLine($"{j} - {skill.SkillName}");
            }

            var selection2 = Console.ReadLine();
            int.TryParse(selection2, out int si);

            // This is used to overwrite 
            var skillTemplate = skillSet.Skills[si - 1];
            Console.WriteLine("Selected skill: " + skillTemplate.SkillName);
            Console.WriteLine(skillTemplate?.Description ?? "");

            Skill selectedSkill = new Skill() { RepeatCount = skillTemplate.RepeatCount };

            while (true)
            {
                selectedSkill = SetUserResponseFromPreviousIteration(selectedSkill, skillTemplate);
                selectedSkill.RepeatCount--;

                for (int i = 0; i < selectedSkill.Tasks.Count; i++)
                {
                    var task = selectedSkill.Tasks[i];

                    PrintTaskName(task);

                    task = ProcessTask(task, selectedSkill).Result;

                    if (task.HaltProcessing)
                    {
                        selectedSkill.RepeatCount = 0;
                        break;
                    }
                    
                    PrintTaskOutput(task);
                }

                if (!selectedSkill.DisableFileLogging) FilePathHelper.WriteSkillToFile(selectedSkill);
                
                if (selectedSkill.RepeatCount <= 0) break;
            }
        }

        public static async Task<Models.Task> ProcessTask(Models.Task task, Skill selectedSkill)
        {
            // Inserts outputs from previous steps into the current step
            task = ReplaceInput(task, selectedSkill);

            // If there is a UserPrompt and the user has not already responded i.e. in the second iteration, we already have the user answers from the first iteration
            if (task.Mode == "User" && !string.IsNullOrEmpty(task.Input) && string.IsNullOrEmpty(task.Output))
            {
                Console.WriteLine(task.Input);

                task.Output = ReadInputAsync().Result;
            }

            if (task.Mode == "AI")
            {
                task.Output = await GetAIResponse(task.Input, selectedSkill);

                while (true)
                {
                    var verifierQuestion = $"Your task is only to determine if the output is complete reply 'YES'. If there is missing output reply with a brief descriptiption of what you think is missing, ignore duplicate content\n\nBEGININPUT:\n {task.Input}\nENDINPUT\nBEGINOUTPUT:\n{task.Output}\nENDOUTPUT";
                    verifierQuestion += "\n\nYour task is only to determine if the output is complete reply 'YES'. If there is missing output reply with a brief descriptiption of what you think is missing, ignore duplicate content";

                    var isOutputComplete = await GetAIResponse(verifierQuestion, selectedSkill, isVerifierMode: true);
                    
                    if (!isOutputComplete.ToLower().Contains("yes"))
                    {
                        var revisedInput = $"PREVIOUS INPUT: {task.Input}\n\nPREVIOUS OUTPUT: {task.Output}\n\nEVALUATION: {isOutputComplete}\nPlease continue responding from exactly where you left off:";
                        
                        var newResponse = await GetAIResponse(revisedInput, selectedSkill);
                        task.Output += newResponse;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // 3. Process special actions 
            if (!string.IsNullOrEmpty(task.SpecialAction))
            {                
                var action = SpecialActionRegistry.GetAction(task.SpecialAction);
                
                task = await action.ExecuteAsync(task, selectedSkill, _settings);
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
                    string output = (t.Mode == "User" && correspondingTask != null) ? correspondingTask.Output : t.Output;

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
                        FilePath = t.FilePath
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
      

        private static void DebugPreviousOutput(SkillSet skillSet)
        {
            var savedOutputFiles = FilePathHelper.GetSavedOutputFilePaths(skillSet);

            if (savedOutputFiles.Count == 0)
            {
                Console.WriteLine("No Previous saved file outputs... returning to AI Skill Generation mode");
                return;
            }

            Console.WriteLine("Select saved file to debug: ");

            int x = 0;
            foreach (var fp in savedOutputFiles)
            {
                x++;
                var fileName = Path.GetFileName(fp);
                Console.WriteLine($"{x} - {fileName}");
            }

            var selection2 = Console.ReadLine();
            int.TryParse(selection2, out int si2);

            var selectedFile = savedOutputFiles[si2 - 1];

            Console.WriteLine($"Selected File: {selectedFile}");
            var skillText = File.ReadAllText(selectedFile);
            var selectedSkill = JsonConvert.DeserializeObject<Skill>(skillText);

            Console.WriteLine("Select Task Name to View:");

            int z = 0;
            foreach (var skill in selectedSkill.Tasks)
            {
                z++;
                Console.WriteLine($"{z} {skill.Name}");
            }

            var selection3 = Console.ReadLine();
            int.TryParse(selection3, out int si3);

            while (true)
            {
                var selectedTask = selectedSkill.Tasks[si3 - 1];
                Console.WriteLine($"Name: {selectedTask.Name}");
                Console.WriteLine($"Mode: {selectedTask.Mode}");
                Console.WriteLine($"Input: {selectedTask.Input}");
                Console.WriteLine($"Output: {selectedTask.Output}");                

                Console.WriteLine();

                Console.WriteLine("1 - Regenerate AI Response");
                Console.WriteLine("2 - View AI Prompt Interpolated");
                Console.WriteLine("3 - Edit AIPrompt");
                Console.WriteLine("4 - Edit UserPrompt");
                Console.WriteLine("5 - Edit UserResponse");
                Console.WriteLine("0 - Exit Debug Mode");

                var selection4 = Console.ReadLine();
                int.TryParse(selection4, out int si4);

                switch (si4)
                {
                    case 1:
                        selectedTask = ProcessTask(selectedTask, selectedSkill).Result;
                        PrintTaskOutput(selectedTask);
                        break;
                    case 2:
                        selectedTask = ReplaceInput(selectedTask, selectedSkill);
                        Console.WriteLine($"Input: {selectedTask.Input}");
                        break;
                    case 3:
                        Console.WriteLine("todo");
                        break;
                    case 4:
                        Console.WriteLine("todo");
                        break;
                    case 0:
                        return;
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }
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


        public static void PrintTaskOutput(Models.Task task)
        {
            if (task.Mode != "AI") return;
            
            if (string.IsNullOrEmpty(task.Input)) return;
            
            if (task.HaltProcessing)
            {
                Console.WriteLine("No more files to process in input folder");
                return;
            }

            Console.WriteLine(task.Output);
        }

        public static Models.Task ReplaceInput(Models.Task task, Skill selectedSkill)
        {
            var outputRegex = new Regex(@"\{\{Output\[(\d+)\]\}\}");
            var filePathRegex = new Regex(@"\{\{FilePath\[(\d+)\]\}\}");

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

        public static async Task<string> GetAIResponse(string userInput, Skill skill = null, bool isVerifierMode = false)
        {
            var apiUrl = "https://api.openai.com/v1/chat/completions";
            
            var _model = string.IsNullOrEmpty(skill?.OpenAiModel) ? _settings.OpenAiModel : skill?.OpenAiModel;
            if (isVerifierMode) _model = _settings.OpenaAIVerifierModel;

            var requestData = new
            {
                max_tokens = 4096,
                model = _model,
                messages = new[] { new { role = "user", content = userInput } },
                temperature = skill == null ? 0.0 : skill.Temperature
            };

            // Define retry policy using Polly
            var retryPolicy = Policy
                .Handle<FlurlHttpException>(ex => ex.Call.Response.StatusCode == 429 || (ex.Call.Response.StatusCode >= 500 && ex.Call.Response.StatusCode <= 599))
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds} seconds due to: {exception.Message}");
                    });

            // Create a cancellation token source
            var cts = new CancellationTokenSource();
            var timeoutTask = Task.Run(async () =>
            {
                int delayInSeconds = 1;
                await Task.Delay(TimeSpan.FromSeconds(1));
                Console.Write("AI thinking");
                while (!cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    Console.Write(".");
                    delayInSeconds++;
                }
            }, cts.Token);

            try
            {
                // Execute the HTTP request with retry policy
                var response = await retryPolicy.ExecuteAsync(() =>
                    apiUrl
                        .WithHeader("Content-Type", "application/json")
                        .WithHeader("Authorization", $"Bearer {_settings.OpenAiKey}")
                        .WithTimeout(600) // Sets the timeout to 6 minutes
                        .PostJsonAsync(requestData)
                );

                // Deserialize the response body to access the AI response
                var openAiResponse = await response.GetJsonAsync<OpenAiResponse>();

                // Cancel the timeout task as we got a response
                cts.Cancel();
                await timeoutTask; // Ensure the timeout task has cleaned up properly

                return openAiResponse.Choices.FirstOrDefault()?.Message.Content;
            }
            catch (FlurlHttpException fex)
            {
                if (fex.Call.Response != null)
                {
                    Console.WriteLine($"HTTP Error to {apiUrl}");
                    Console.WriteLine($"Status Code: {fex.Call.Response.StatusCode}");
                    Console.WriteLine($"Response Body: {await fex.GetResponseStringAsync()}");
                }
                else
                {
                    Console.WriteLine($"HTTP Request Failed - {apiUrl}");
                    Console.WriteLine(fex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Ensure the cancellation and waiting for the timeout task
                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                    await timeoutTask; // Ensure the timeout task has cleaned up properly
                }
            }

            return null;
        }


    }
}
