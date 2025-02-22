using Newtonsoft.Json;
using skill_composer.Models;

namespace skill_composer.Helper
{ 
    public static class FilePathHelper
    {
        private static string _skillComposerDirectory;

        /// <summary>
        /// Returns the root directory for the twilio-core project.
        /// </summary>
        /// <returns></returns>
        public static string GetRootDirectory()
        {
            if (!string.IsNullOrEmpty(_skillComposerDirectory)) return _skillComposerDirectory;

            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Check if the current directory contains a folder named "skill-composer"
            var skillComposerDir = Path.Combine(path, "skill-composer");
            if (Directory.Exists(skillComposerDir))
            {
                var programExists = File.Exists(Path.Combine(skillComposerDir, "skill-composer.csproj")) && !skillComposerDir.ToLower().Contains("test");

                if (programExists)
                {
                    _skillComposerDirectory = skillComposerDir;
                    return _skillComposerDirectory;
                }
            }

            // If not found, move up the directory tree
            var dir = Directory.GetParent(path).FullName;

            for (int i = 0; i < 15; i++)
            {                
                var programExists = File.Exists(Path.Combine(dir, "skill-composer.csproj")) && !skillComposerDir.ToLower().Contains("test");

                if (programExists)
                {
                    _skillComposerDirectory = dir;
                    return _skillComposerDirectory;
                }
              
                dir = Directory.GetParent(dir).FullName;
            }

            throw new Exception("Failed to find root folder \"skill-composer\", the root folder must contain a 'skill-composer.csproj' file");
        }


        public static string GetDataInputFilePath()
        {
            var rootDirectory = GetRootDirectory();
            var dataInputPath = Path.Combine(rootDirectory, "data", "input");

            if (!Directory.Exists(dataInputPath))
            {
                Directory.CreateDirectory(dataInputPath);
            }

            foreach (var file in Directory.EnumerateFiles(dataInputPath))
            {
                // check if the file exists
                if (File.Exists(file))
                {
                    return file; // Returns the first file found
                }
            }
            // empty string returned to halt processing
            return "";  
        }

        public static string GetDataInputDirectory()
        {
            var rootDirectory = GetRootDirectory();
            var dataInputDirectory = Path.Combine(rootDirectory, "data", "input");

            if (!Directory.Exists(dataInputDirectory))
            {
                Directory.CreateDirectory(dataInputDirectory);
            }
            
            return dataInputDirectory;
        }

        public static string MoveFileToOutputDirectory(string fullFilePath)
        {
            var outputDirectory = GetDataOutputDirectory();
            string outputFilePath;

            if (fullFilePath.Contains("input"))
            {
                // Replace "input" with "output" in the file path
                outputFilePath = fullFilePath.Replace("input", "output");
            }
            else
            {
                // If "input" is not in the path, use the file name and combine it with the output directory
                var fileName = Path.GetFileName(fullFilePath);
                outputFilePath = Path.Combine(outputDirectory, fileName);
            }

            // Ensure the output file path is within the output directory
            if (!outputFilePath.StartsWith(outputDirectory, StringComparison.OrdinalIgnoreCase))
            {
                var fileName = Path.GetFileName(fullFilePath);
                outputFilePath = Path.Combine(outputDirectory, fileName);
            }

            // Get the directory name from the new output file path
            var outputFileDirectory = Path.GetDirectoryName(outputFilePath);

            if (outputFileDirectory == null)
            {
                throw new InvalidOperationException("Unable to determine output directory.");
            }

            // Ensure the output directory exists, including all necessary subdirectories
            if (!Directory.Exists(outputFileDirectory))
            {
                Directory.CreateDirectory(outputFileDirectory);
            }

            // If a file with the same name exists, delete it to allow overwriting
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            // Move the file
            File.Move(fullFilePath, outputFilePath);

            return outputFilePath;
        }

        public static string GetDataOutputDirectory()
        {
            var rootDirectory = GetRootDirectory();
            var dataOutputPath = Path.Combine(rootDirectory, "data", "output");
            
            if (!Directory.Exists(dataOutputPath))
            {
                Directory.CreateDirectory(dataOutputPath);
            }

            return dataOutputPath;
        }

        public static string GetSkillFilePath()
        {
            var skillFilePath = GetRootDirectory();
            skillFilePath = Path.Combine(skillFilePath, "skills.json");
            
            if(File.Exists(skillFilePath))
            {
                return skillFilePath;
            }
            throw new Exception($"Failed to find \"skills.json\", in the folder {GetRootDirectory()}");
        }

        public static string GetSettingsFile()
        {
            var skillFilePath = GetRootDirectory();
            skillFilePath = Path.Combine(skillFilePath, "appsettings.json");

            if (File.Exists(skillFilePath))
            {
                return skillFilePath;
            }
            else
            {
                Settings.CreateDefaultSettingsFile(skillFilePath);

                if (File.Exists(skillFilePath)) return skillFilePath;
            }
            
            throw new Exception($"Failed to find \"appsettings.json\", in the folder {GetRootDirectory()}");            
        }


        public static string GetBuildDirectory()
        {
            return Path.Combine(GetRootDirectory(), "build");
        }

        public static string GetSkillsFileByName(string fileName, bool isBuild = true)
        {
            var skillsDirectory = Path.Combine(GetTaskRouterDirectory(isBuild), "skill");
            if (!Directory.Exists(skillsDirectory)) throw new Exception($"ERROR GetSkillsFilePath directory {skillsDirectory} does not exist");

            var skillsFilePath = Path.Combine(skillsDirectory, $"{fileName}");
            if (!File.Exists(skillsFilePath)) throw new Exception($"ERROR GetSkillsFilePath file {skillsFilePath} does not exist");

            return skillsFilePath;
        }
        public static string GetSkillsFilePath(string region, bool isBuild = true)
        {
            var skillsDirectory = Path.Combine(GetTaskRouterDirectory(isBuild), "skill");
            if (!Directory.Exists(skillsDirectory)) throw new Exception($"ERROR GetSkillsFilePath directory {skillsDirectory} does not exist");

            var skillsFilePath = Path.Combine(skillsDirectory, $"skills-{region}.json");
            if (!File.Exists(skillsFilePath)) throw new Exception($"ERROR GetSkillsFilePath file {skillsFilePath} does not exist");

            return skillsFilePath;
        }

        /// <summary>
        /// Returns the complete path to the config directory.
        /// </summary>
        /// <returns></returns>
        public static string GetConfigDirectory()
        {
            var config = Path.Combine(GetRootDirectory(), "config");
            if (Directory.Exists(config)) return config;
            throw new Exception($"Failed to find config folder {config}");
        }

        /// <summary>
        /// Returns the complete path to the features directory.
        /// </summary>
        /// <returns></returns>
        public static string GetFeaturesDirectory(bool isBuild = true)
        {
            var features = "";
            if (isBuild)
            {
                features = GetBuildDirectory();
                if (!Directory.Exists(features)) throw new Exception($"Failed to find features folder {features}");
                return features;
            }

            features = Path.Combine(GetRootDirectory(), "features");
            if (!Directory.Exists(features)) throw new Exception($"Failed to find features folder {features}");
            return features;
        }
        /// <summary>
        /// Returns the complete path to the shared replacements file        
        /// </summary>
        /// <returns></returns>
        public static string GetSharedReplacementsFilePath(bool isBuild = true)
        {
            var sharedReplacementsFilePath = Path.Combine(GetFeaturesDirectory(isBuild), "sharedreplacements.json");
            return sharedReplacementsFilePath;
        }

        public static string GetSharedEnvironmentReplacementFilePath(string environment, bool isBuild = true)
        {
            var sharedEnvironmentReplacementsFilePath = Path.Combine(GetFeaturesDirectory(isBuild), $"{environment}.sharedreplacements.json");
            return sharedEnvironmentReplacementsFilePath;
        }

        /// <summary>
        /// Returns the complete path to the task-router directory.
        /// </summary>
        /// <returns></returns>
        public static string GetTaskRouterDirectory(bool isBuild = true)
        {
            var taskRouter = Path.Combine(GetFeaturesDirectory(isBuild), "task-router");
            if (!Directory.Exists(taskRouter)) throw new Exception($"Failed to find task-router folder {taskRouter}");
            return taskRouter;
        }

        /// <summary>
        /// Returns the complete path to the task-router event callback file.
        /// </summary>
        /// <returns></returns>
        public static string GetTaskRouterEventCallbackFile(bool isBuild = true)
        {
            var taskRouterEventCallback = Path.Combine(GetTaskRouterDirectory(isBuild), "event-callback-urls.json");
            if (!File.Exists(taskRouterEventCallback)) throw new Exception($"Failed to find task-router event callback file {taskRouterEventCallback}");
            return taskRouterEventCallback;
        }

        /// <summary>
        /// Get variables file for the environment
        /// </summary>
        /// <param name="environment"></param>
        /// /// <param name="region"></param>
        /// <returns></returns>
        public static string GetVariablesFile(string environment, string region, bool isBuild = true)
        {
            var variablesFile = Path.Combine(FilePathHelper.GetFeaturesDirectory(isBuild), $"variables.{region}{environment}.json");
            return variablesFile;
        }

        /// <summary>
        /// Get the dependencies file (defined in the features directory)
        /// </summary>
        /// <returns></returns>
        public static string GetDependenciesFile(bool isBuild = true)
        {
            var dependencies = Path.Combine(GetFeaturesDirectory(isBuild), "dependencies.json");
            if (!File.Exists(dependencies)) throw new Exception($"Failed to find dependencies file {dependencies}");
            return dependencies;
        } 

        public static void WriteSkillToFile(Skill selectedSkill)
        {
            if (selectedSkill.AppendFileLogging is null || selectedSkill.AppendFileLogging == false)
            {
                // Write output to a new file
                var filePath = GetNewFilePathForSkill(selectedSkill.SkillName);
                File.WriteAllText(filePath, JsonConvert.SerializeObject(selectedSkill, Formatting.Indented));
            }
            else // Append output to a single file
            {
                var filePath = FilePathHelper.GetFilePathForSkill(selectedSkill.SkillName);
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

        // for saving the output of a run
        private static string GetNewFilePathForSkill(string skillName)
        {
            var filePath = Path.Combine(FilePathHelper.GetRootDirectory(), $"{skillName}.json");
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

        public static List<string> GetSavedOutputFilePaths(SkillSet skillSet)
        {
            var savedOutputFiles = new List<string>();

            foreach (var skill in skillSet.Skills)
            {
                var filePaths = FilePathHelper.GetSavedFilePathsBySkillName(skill.SkillName);
                if (filePaths != null && filePaths.Count > 0)
                {
                    savedOutputFiles.AddRange(filePaths);
                }
            }

            return savedOutputFiles;
        }

        public static List<string> GetSavedFilePathsBySkillName(string skillName)
        {
            var directory = FilePathHelper.GetRootDirectory(); // Assuming this gets your target directory.
            var searchPattern = $"{skillName}*.json"; // Matches skillName.json, skillName1.json, etc.
            var filePaths = Directory.GetFiles(directory, searchPattern);

            return filePaths.ToList();
        }

        public static string GetFilePathForSkill(string skillName)
        {
            var filePath = Path.Combine(FilePathHelper.GetRootDirectory(), $"{skillName}.json");
            int k = 0;
            string baseFilePath = filePath;
            string directory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string baseName = fileNameWithoutExtension.Replace(skillName, skillName);

            k++;
            filePath = Path.Combine(directory, $"{baseName}{k}{extension}");

            return filePath;
        }

        /// <summary>
        /// Get the feature folder by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetFeatureFolderByName(string name, bool isBuild = true)
        {
            var path = "";
            if (isBuild)
            {
                path = GetBuildDirectory();
                path = Path.Combine(path, name);
                if (!Directory.Exists(path)) throw new Exception($"ERROR GetFeatureFolderByName folder {path} does not exist");
                return path;
            }

            path = Path.Combine(GetRootDirectory(), "features");
            if (!Directory.Exists(path)) throw new Exception($"ERROR GetFeatureFolderByName folder {path} does not exist");
            path = Path.Combine(path, name);
            if (!Directory.Exists(path)) throw new Exception($"ERROR GetFeatureFolderByName folder {path} does not exist");
            return path;
        }
    }
}
