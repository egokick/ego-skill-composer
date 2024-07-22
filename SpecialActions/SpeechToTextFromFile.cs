using Newtonsoft.Json;
using skill_composer.Models;
using System.Diagnostics;
using skill_composer.Helper;
using Flurl.Http;

namespace skill_composer.SpecialActions
{
    public class SpeechToTextFromFile : ISpecialAction
    { 
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var inputFilePath = task.Input;

            if (!string.IsNullOrEmpty(inputFilePath))
            {
                inputFilePath = FilePathHelper.GetDataInputFilePath();
            }
            
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            if (string.IsNullOrEmpty(inputFilePath))
            {
                return task;
            }
            else
            {
                var inputFileName = Path.GetFileName(inputFilePath);

                var extension = Path.GetExtension(inputFilePath);

                if (extension != ".mp3")
                {
                    // try to convert it to an mp3 with ffmpeg 
                    var mp3FileName = inputFileName.Replace(extension, ".mp3"); 

                    var mp3FilePath = Path.Combine(outputDirectory, mp3FileName);

                    ConvertToMp3(inputFilePath, mp3FilePath);
                    Console.WriteLine($"Finished conversion: {mp3FilePath}");

                    inputFilePath = mp3FilePath;
                }
                // if the file is above 25MB it must be split
                List<string> audioFiles = SplitAndTranslateAudio(inputFilePath).Result;
                List<string> translatedTexts = new List<string>();

                foreach (var filePath in audioFiles)
                {
                    string translatedText = TranslateAudioToText(filePath, settings).Result;
                    translatedTexts.Add(translatedText);
                    Console.WriteLine(translatedText);

                    // Optional: clean up if the file was a split part
                    if (filePath != inputFilePath) // This check prevents deleting the original file if it wasn't split
                    {
                        File.Delete(filePath);
                    }
                }

                // If you need to combine the texts into a single string
                string combinedTranslatedText = string.Join(" ", translatedTexts);
                task.Output = combinedTranslatedText;

                var translatedTextFileName = inputFileName.Replace(extension, ".txt");
                var translatedTextFilePath = Path.Combine(outputDirectory, translatedTextFileName);

                File.WriteAllText(translatedTextFilePath, combinedTranslatedText);

            }

            return task;
        }


        public static async Task<string> TranslateAudioToText(string filePath, Settings settings)
        {
            var jsonResponse = await "https://api.openai.com/v1/audio/translations"
                .WithHeader("Authorization", $"Bearer {settings.OpenAiKey}")
                .PostMultipartAsync(mp => mp
                    .AddFile("file", filePath)
                    .AddString("model", "whisper-1"))
                .ReceiveString();

            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
            return response?["text"] ?? "";
        }

        private static void ConvertToMp3(string originalFilePath, string mp3FilePath)
        {
            Console.WriteLine($"Converting file to mp3: {originalFilePath}");

            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{originalFilePath}\" \"{mp3FilePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // Read error output (where FFmpeg writes most of its status messages) first
                var error = process.StandardError.ReadToEnd();

                // Optionally, read the standard output if needed
                var output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg conversion failed: {error}");
                }

                Console.WriteLine("Conversion successful: " + error); // FFmpeg writes conversion success messages to StandardError as well
            }
        }
        public static async Task<List<string>> SplitAndTranslateAudio(string inputFilePath)
        {
            const int maxFileSize = 5214400; // 5MB in bytes
            List<string> filePaths = new List<string>();
            FileInfo fileInfo = new FileInfo(inputFilePath);

            if (fileInfo.Length > maxFileSize)
            {
                // Split the file
                string directoryPath = Path.GetDirectoryName(inputFilePath);
                string fileBaseName = Path.GetFileNameWithoutExtension(inputFilePath);
                string fileExtension = Path.GetExtension(inputFilePath);

                string outputPathFormat = $"{directoryPath}/{fileBaseName}_part%03d{fileExtension}";

                // Command to split the audio file into parts smaller than 25MB
                string ffmpegCmd = $"-i \"{inputFilePath}\" -f segment -segment_time 300 -c copy \"{outputPathFormat}\"";
                RunFFmpegCommand(ffmpegCmd);

                // Collect the paths of the split files
                filePaths.AddRange(Directory.GetFiles(directoryPath, $"{fileBaseName}_part*{fileExtension}"));
            }
            else
            {
                // File size is within the limit, use the original file
                filePaths.Add(inputFilePath);
            }

            return filePaths;
        }


        private static void RunFFmpegCommand(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process process = Process.Start(startInfo);
            // Asynchronously read the standard output and standard error of the process
            // This prevents the process from blocking due to the streams being full
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // Wait for the process to exit
            process.WaitForExit();

            // Now that the process has exited, ensure we have all the output.
            var output = outputTask.Result;
            var error = errorTask.Result;

            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg error: {error}");
            }
        }

    }
}