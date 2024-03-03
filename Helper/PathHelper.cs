using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace skill_composer.Helper
{
 
    public static class PathHelper
    {
        private static string _skillComposerDirectory;

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
            var fileName = Path.GetFileName(fullFilePath);

            var outputDirectory = GetDataOutputDirectory();

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Combine the output directory path and the sanitized file name
            var outputFilePath = Path.Combine(outputDirectory, fileName);

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

        /// <summary>
        /// Returns the root directory for the twilio-core project.
        /// </summary>
        /// <returns></returns>
        public static string GetRootDirectory()
        {
            if (!string.IsNullOrEmpty(_skillComposerDirectory)) return _skillComposerDirectory;

            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Check if this directory contains features and config folder, if it doesn't go up a directory and look again
            var programExists = File.Exists(Path.Combine(path, "Program.cs"));
            
            if (programExists)
            {
                _skillComposerDirectory = path;
                return _skillComposerDirectory;
            }
            var dir = Directory.GetParent(path).FullName;

            for (int i = 0; i < 15; i++)
            {
                programExists = File.Exists(Path.Combine(dir, "Program.cs")); 
                if (programExists)
                {
                    _skillComposerDirectory = dir;
                    return _skillComposerDirectory;
                }
                dir = Directory.GetParent(dir).FullName;
            }
            throw new Exception("Failed to find root folder \"skill-composer\", the root folder must contain a 'Program.cs' file");
        }

        public static string GetSkillFile()
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
            var variablesFile = Path.Combine(PathHelper.GetFeaturesDirectory(isBuild), $"variables.{region}{environment}.json");
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

        public static string GetFilePathForSkill(string skillName)
        {
            var filePath = Path.Combine(PathHelper.GetRootDirectory(), $"{skillName}.json");
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
