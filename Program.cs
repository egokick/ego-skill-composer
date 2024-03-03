using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using skill_composer.Helper;
using skill_composer.Models;
using System.Text.RegularExpressions;
using Flurl.Util;
using Polly;
using Task = System.Threading.Tasks.Task;


namespace skill_composer
{
    internal class Program
    {
        public static Settings _settings = new Settings();

        static void Main(string[] args)
        {
            // Set the settings file 
            var settingsJson = File.ReadAllText(PathHelper.GetSettingsFile());
            var settings = JsonConvert.DeserializeObject<Settings>(settingsJson);
            _settings = settings;

            while (true)
            {
                var skillFilePath = PathHelper.GetSkillFile();

                var skillsJson = File.ReadAllText(skillFilePath);

                var skillSet = JsonConvert.DeserializeObject<SkillSet>(skillsJson); 

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

            Skill selectedSkill = new Skill() { RepeatCount = skillTemplate.RepeatCount };

            while (true)
            {
                selectedSkill = SetUserResponseFromPreviousIteration(selectedSkill, skillTemplate);
                selectedSkill.RepeatCount--;

                for (int i = 0; i < selectedSkill.Tasks.Count; i++)
                {
                    var task = selectedSkill.Tasks[i];

                    task = ProcessTask(task, selectedSkill);

                    if (task.HaltProcessing)
                    {
                        selectedSkill.RepeatCount = 0;
                        break;
                    }
                    
                    PrintAIResponse(task);
                }

                if (!selectedSkill.DisableFileLogging) WriteToFile(selectedSkill);
                
                if (selectedSkill.RepeatCount <= 0) break;
            }
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


        public static void WriteToFile(Skill selectedSkill)
        {
            if (!selectedSkill.AppendFileLogging)
            {
                // Write output to a new file
                var filePath = GetNewFilePathForSkill(selectedSkill.SkillName);
                File.WriteAllText(filePath, JsonConvert.SerializeObject(selectedSkill, Formatting.Indented));
            }
            else // Append output to a single file
            {
                var filePath = PathHelper.GetFilePathForSkill(selectedSkill.SkillName);
                var newContent = JsonConvert.SerializeObject(selectedSkill, Formatting.Indented);

                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    // Prepare the new content to be appended correctly, without removing any characters from newContent
                    newContent = "," + Environment.NewLine + newContent; // Add a comma and newline for readability before the new JSON object

                    // Remove the last character of the file (closing square bracket) before appending
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        if (stream.Length > 1)
                        {
                            stream.SetLength(stream.Length - 1); // Remove the last character (']')
                        }
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.BaseStream.Seek(0, SeekOrigin.End); // Go to the end of the file
                            writer.Write(newContent); // Append the new content, keeping the opening curly brace
                            writer.Write("]"); // Re-add the closing square bracket to properly close the JSON array
                        }
                    }
                }
                else
                {
                    // File doesn't exist or is empty, start a new JSON array
                    newContent = "[" + newContent + "]";
                    File.WriteAllText(filePath, newContent);
                }
            }
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



        // 1 Replace the input fields if there are any {{Output[1}} replacements
        // 2 Ask User the Input if type is "User" 
        // 3 Ask AI The Input if type is "AI"
        // 4 Process special actions
        public static Models.Task ProcessTask(Models.Task task, Skill selectedSkill)
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
                task.Output = GetAIResponse(task.Input, selectedSkill).Result;                 
            }

            // 3. Process special actions

            if (task.SpecialAction == "ReadFile")
            {
                if (string.IsNullOrEmpty(task.Input))
                {
                    task.FilePath = PathHelper.GetDataInputFilePath(); // Gets any file in the input folder

                    // no more files to process
                    if (string.IsNullOrEmpty(task.FilePath))
                    {
                        // so stop processing for this skill
                        task.HaltProcessing = true;
                        return task;
                    }
                }
                else
                {
                    task.FilePath = task.Input;
                }

                var fileName = Path.GetFileName(task.FilePath);
                var fileContent = File.ReadAllText(task.FilePath);

                task.Output = $"Title: {fileName} {fileContent}";
            }

            if (task.SpecialAction == "RenameFile")
            {
                var fileName = Path.GetFileName(task.FilePath);
                var directory = Path.GetDirectoryName(task.FilePath);
                
                // replace commas with hyphens 
                var labels = task.Input.Replace(" ", "").Replace(",", "-");

                fileName = labels + " " + fileName;

                // Use Regex to strip non-alphanumeric characters from the fileName
                fileName = Regex.Replace(fileName, "[^-a-zA-Z0-9. ]", "");

                var newFilePath = Path.Combine(directory, fileName);

                File.Move(task.FilePath, newFilePath);

                // Set the new filepath so that it can be correctly referenced in subsequent task steps
                task.FilePath = newFilePath;
            }
            
            if (task.SpecialAction == "MoveFileToOutput")
            {
                // Moves the file to the output folder and assigns FilePath to the new filePath
                task.FilePath = PathHelper.MoveFileToOutputDirectory(task.Input);
            }

            if (task.SpecialAction == "MoveOutputFilesToInputFolder")
            {
                var outputDirectory = PathHelper.GetDataOutputDirectory();
                var inputDirectory = PathHelper.GetDataInputDirectory();
                
                // Get all files in the output directory
                var outputFiles = Directory.GetFiles(outputDirectory);

                foreach (var outputFile in outputFiles)
                {
                    // Generate the path for the file in the input directory
                    var fileName = Path.GetFileName(outputFile);
                    var inputFile = Path.Combine(inputDirectory, fileName);

                    // Move the file to the input directory
                    // This will overwrite the file in the output directory if it already exists
                    File.Move(outputFile, inputFile, true);
                }
            }

            if (task.SpecialAction == "GetUniqueLabelsFromInputFiles")
            {
                var inputDirectory = PathHelper.GetDataInputDirectory();
                // Ensure the directory exists to avoid exceptions
                if (Directory.Exists(inputDirectory))
                {
                    var allFiles = Directory.GetFiles(inputDirectory);

                    var labelGroups = allFiles
                        .SelectMany(file => Path.GetFileName(file).Split('-')) // Split each filename by '-'
                        .GroupBy(label => label) // Group by label
                        .Where(group => group.Count() > 1) // Filter groups by those that have more than one occurrence
                        .Select(group => group.Key) // Select the label from the group
                        .ToList(); // Convert to List

                    task.Output = string.Join(",", labelGroups);
                }
                else
                {
                    Console.WriteLine($"GetUniqueLabelsFromInputFiles -> No files in the input directory"); 
                }
            }

            if(task.SpecialAction == "ConcatenateFilesByLabel") 
            {
                var labels = task.Input.Split(',').ToList();
                labels.RemoveAll(label => string.IsNullOrEmpty(label) || label == ".");
                var dataInputDirectory = PathHelper.GetDataInputDirectory();
                var outputDirectory = PathHelper.GetDataOutputDirectory();

                foreach (var label in labels)
                {
                    var labelTrimmed = label.Trim(); // Ensure no leading or trailing spaces
                    var filesWithLabel = Directory.GetFiles(dataInputDirectory, "*" + labelTrimmed + "*");

                    var concatenatedContent = filesWithLabel.SelectMany(file => File.ReadAllLines(file)).ToArray();

                    var outputFile = Path.Combine(outputDirectory, labelTrimmed + ".txt");
                    File.WriteAllLines(outputFile, concatenatedContent);
                    
                    task.FilePath = outputFile;

                    Console.WriteLine($"Created {outputFile} with concatenated content of {filesWithLabel.Length} files.");
                }
            }
            return task;
        }

        public static List<string> GetSavedOutputFilePaths(SkillSet skillSet)
        {
            var savedOutputFiles = new List<string>();

            foreach (var skill in skillSet.Skills)
            {
                var filePaths = GetSavedFilePathsBySkillName(skill.SkillName);
                if (filePaths != null && filePaths.Count > 0)
                {
                    savedOutputFiles.AddRange(filePaths);
                }
            }

            return savedOutputFiles;
        }

        private static void DebugPreviousOutput(SkillSet skillSet)
        {
            var savedOutputFiles = GetSavedOutputFilePaths(skillSet);

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
                        selectedTask = ProcessTask(selectedTask, selectedSkill);
                        PrintAIResponse(selectedTask);
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

        public static void PrintAIResponse(Models.Task task)
        {
            if (task.Mode != "AI") return;
            
            if (string.IsNullOrEmpty(task.Input)) return;
            
            if (task.HaltProcessing)
            {
                Console.WriteLine("No more files to process in input folder");
                return;
            }

            var currentForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine("==========================");
            Console.WriteLine(task.Name);
            Console.ForegroundColor = currentForegroundColor;
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

        public static List<string> GetSavedFilePathsBySkillName(string skillName)
        {
            var directory = PathHelper.GetRootDirectory(); // Assuming this gets your target directory.
            var searchPattern = $"{skillName}*.json"; // Matches skillName.json, skillName1.json, etc.
            var filePaths = Directory.GetFiles(directory, searchPattern);

            return filePaths.ToList();
        }

        // for saving the output of a run
        public static string GetNewFilePathForSkill(string skillName)
        {
            var filePath = Path.Combine(PathHelper.GetRootDirectory(), $"{skillName}.json");
            int k = 0;
            string baseFilePath = filePath;
            string directory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string baseName = fileNameWithoutExtension.Replace(skillName, skillName);

            while (File.Exists(filePath))
            {
                k++;
                filePath = Path.Combine(directory, $"{baseName}{k}{extension}");
            }

            return filePath;            
        }


        public static async Task<string> GetAIResponse(string userInput, Skill skill = null)
        {
            var apiUrl = "https://api.openai.com/v1/chat/completions";

            var requestData = new
            {
                model = string.IsNullOrEmpty(skill?.OpenAiModel) ? _settings.OpenAiModel : skill?.OpenAiModel,
                messages = new[]
                {
                new { role = "user", content = userInput }
            },
                temperature = skill == null ? 0.1 : skill.Temperature
            };

            // Define retry policy using Polly
            var retryPolicy = Policy
                .Handle<FlurlHttpException>(ex => ex.Call.Response.StatusCode == 429 || (ex.Call.Response.StatusCode >= 500 && ex.Call.Response.StatusCode <= 599))
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds} seconds due to: {exception.Message}");
                    });

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

                // headers include rate limiting info
                // LogResponseHeaders(response.Headers);

                // Deserialize the response body to access the AI response
                var openAiResponse = await response.GetJsonAsync<OpenAiResponse>();

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

            return null;
        }

        public static void LogResponseHeaders(IReadOnlyNameValueList<string> headers)
        { 
            var rateLimitHeaders = new string[]
            {
                    "x-ratelimit-limit-requests",
                    "x-ratelimit-limit-tokens",
                    "x-ratelimit-remaining-requests",
                    "x-ratelimit-remaining-tokens",
                    "x-ratelimit-reset-requests",
                    "x-ratelimit-reset-tokens"
            };

            // Iterate through each header we are interested in
            foreach (var headerKey in rateLimitHeaders)
            {
                var headerValue = headers.FirstOrDefault(h => h.Name.Equals(headerKey, StringComparison.OrdinalIgnoreCase));

                // Check if the header was found by comparing the default value of the header's value type
                // Since 'headerValue' is a tuple, it will not be null. Instead, check if the 'Value' property is not null or empty.
                if (!string.IsNullOrEmpty(headerValue.Value))
                {
                    // debug rate limiting
                    Console.WriteLine($"{headerKey}: {headerValue.Value}");
                }
            } 
        }
    }
}
