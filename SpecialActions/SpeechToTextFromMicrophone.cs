using NAudio.Wave;
using Newtonsoft.Json;
using skill_composer.Models;
using skill_composer.Helper;
using Flurl.Http;
using System.Collections.Concurrent;
using Task = System.Threading.Tasks.Task;
using System.Diagnostics;

namespace skill_composer.SpecialActions
{
    public class SpeechToTextFromMicrophone : ISpecialAction
    {
        private const int RecordingInterval = 1; // in seconds
        private const int ConcatenationInterval = 2; // in seconds
        private static readonly string[] SupportedFileExtensions = { ".mp3" };

        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill, Settings settings)
        {
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();
            string selectedDevice = SelectAudioDevice();

            //var tempFiles = new ConcurrentBag<string>();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                var processingTask = StartProcessingAsync(outputDirectory, settings, cancellationToken);
                var recordingTask = StartRecordingAsync(outputDirectory, selectedDevice,  cancellationToken);              

                // Wait for both tasks to complete
                await Task.WhenAll(recordingTask, processingTask);

                // Merge and transcribe final files
                //string finalText = await MergeAndTranscribeAsync(tempFiles, settings);
                //task.Output = finalText;

                //// Cleanup temporary files
                //foreach (var file in tempFiles)
                //{
                //    File.Delete(file);
                //}
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Recording or processing was canceled.");
            }

            return task;
        }
     
        private static string SelectAudioDevice()
        {
            Console.WriteLine("Microphones:");
            var inputDevices = WaveInEvent.DeviceCount;
            for (int i = 0; i < inputDevices; i++)
            {
                var deviceName = WaveInEvent.GetCapabilities(i).ProductName;
                Console.WriteLine($"{i}: {deviceName}");
            } 

            Console.WriteLine("\nSelect the audio device by number:");
            int selectedDeviceIndex = int.Parse(Console.ReadLine());
            
            return selectedDeviceIndex.ToString();
        }


    private async Task<bool> StartRecordingAsync(string outputDirectory, string selectedDevice,  CancellationToken cancellationToken)
        {
            using (var waveIn = new WaveInEvent())
            {
                waveIn.DeviceNumber = int.Parse(selectedDevice);
                waveIn.WaveFormat = new WaveFormat(44100, 16, 2);

                var buffer = new ConcurrentQueue<byte[]>();
                waveIn.DataAvailable += (s, e) =>
                {
                    var bufferCopy = new byte[e.BytesRecorded];
                    Array.Copy(e.Buffer, bufferCopy, e.BytesRecorded);
                    buffer.Enqueue(bufferCopy);
                };

                waveIn.StartRecording();
                Console.WriteLine("Recording started...");

                int counter = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    string filePath = Path.Combine(outputDirectory, $"recording_{counter}.mp3");

                    using (var writer = new WaveFileWriter(filePath, waveIn.WaveFormat))
                    {
                        var recordingEnd = DateTime.Now.AddSeconds(RecordingInterval);
                        while (DateTime.Now < recordingEnd)
                        {
                            if (buffer.TryDequeue(out var data))
                            {
                                writer.Write(data, 0, data.Length);
                            }
                        }
                    }

                    //tempFiles.Add(filePath);
                    await WaitForFileRelease(filePath);
                    counter++;
                }

                waveIn.StopRecording();
                Console.WriteLine("Recording stopped.");
            }

            return true;
        }

        private static async Task WaitForFileRelease(string filePath)
        {
            while (true)
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        // If we can open the file for exclusive access, then it is not being used by another process
                        break;
                    }
                }
                catch (IOException)
                {
                    // Wait a short period before retrying
                    await Task.Delay(100);
                }
            }
        }

        private static async Task<bool> StartProcessingAsync(string outputDirectory, Settings settings, CancellationToken cancellationToken)
        {
            await Task.Delay(100);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(ConcatenationInterval * 1000, cancellationToken);

                var tempFiles = Directory.GetFiles(outputDirectory, "recording_*.mp3")
                 .OrderByDescending(f => int.Parse(Path.GetFileNameWithoutExtension(f).Split('_')[1]))
                 .Take(10)
                 .ToList();

                List<string> filesToProcess = null;

                if (tempFiles != null && tempFiles.Count > 0)
                {
                    filesToProcess = tempFiles.Take(tempFiles.Count - 1).ToList();
                }
                else
                {
                    filesToProcess = new List<string>();
                }  

                var finalTranscript = await ConcatenateAndTranscribeAsync(filesToProcess, settings);
                Console.WriteLine("==================");
                Console.WriteLine(finalTranscript);

            }

            return true;
        }

        private static async Task<string> ConcatenateAndTranscribeAsync(List<string> tempFiles, Settings settings)
        {
            if (tempFiles.Count < 2)
                return "";

            string concatenatedFilePath = Path.Combine(Path.GetDirectoryName(tempFiles[0]), "concatenated.mp3");

            // Concatenate all files in tempFiles list
            ConcatenateAudioFiles(tempFiles, concatenatedFilePath);

            string finalText = await TranslateAudioToText(concatenatedFilePath, settings);

            File.Delete(concatenatedFilePath);
            return finalText;
        } 

        private static void ConcatenateAudioFiles(IEnumerable<string> inputFilePaths, string outputFilePath)
        {
            var orderedInputFilePaths = inputFilePaths
                .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f).Split('_')[1]))
                .ToList();

            // Create a temporary directory to store converted files
            string tempDirectory = Path.Combine(Path.GetTempPath(), "ffmpeg_temp");
            Directory.CreateDirectory(tempDirectory);

            List<string> convertedFilePaths = new List<string>();
            foreach (var inputFilePath in orderedInputFilePaths)
            {
                string convertedFilePath = Path.Combine(tempDirectory, Path.GetFileName(inputFilePath));
                ConvertToCompatibleFormat(inputFilePath, convertedFilePath);
                convertedFilePaths.Add(convertedFilePath);
            }

            // Create a temporary text file to list all converted files for ffmpeg
            string tempFileListPath = Path.Combine(tempDirectory, "ffmpeg_file_list.txt");
            using (var writer = new StreamWriter(tempFileListPath))
            {
                foreach (var convertedFilePath in convertedFilePaths)
                {
                    writer.WriteLine($"file '{convertedFilePath.Replace("\\", "\\\\")}'");
                }
            }

            // Concatenate the converted files using ffmpeg
            var ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-f concat -safe 0 -i \"{tempFileListPath}\" -c copy \"{outputFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            ffmpegProcess.Start();

            // Read the standard output and error (for debugging purposes)
            string output = ffmpegProcess.StandardOutput.ReadToEnd();
            string error = ffmpegProcess.StandardError.ReadToEnd();

            ffmpegProcess.WaitForExit();

            // Clean up the temporary directory
            Directory.Delete(tempDirectory, true);

            // Optionally, log the output and error for debugging
            //Console.WriteLine(output);
            //Console.WriteLine(error);

            if (ffmpegProcess.ExitCode != 0)
            {
                throw new Exception($"ffmpeg exited with code {ffmpegProcess.ExitCode}: {error}");
            }
        }

        private static void ConvertToCompatibleFormat(string inputFilePath, string outputFilePath)
        {
            var ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{inputFilePath}\" -codec:a libmp3lame -qscale:a 2 \"{outputFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            ffmpegProcess.Start();

            // Read the standard output and error (for debugging purposes)
            string output = ffmpegProcess.StandardOutput.ReadToEnd();
            string error = ffmpegProcess.StandardError.ReadToEnd();

            ffmpegProcess.WaitForExit();

            // Optionally, log the output and error for debugging
            //Console.WriteLine(output);
            //Console.WriteLine(error);

            if (ffmpegProcess.ExitCode != 0)
            {
                throw new Exception($"ffmpeg exited with code {ffmpegProcess.ExitCode}: {error}");
            }
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

        private static async Task<string> MergeAndTranscribeAsync(ConcurrentBag<string> tempFiles, Settings settings)
        {
            var tempFilesList = tempFiles.ToList();
            string concatenatedFilePath = Path.Combine(Path.GetDirectoryName(tempFilesList[0]), "final_concatenated.mp3");

            ConcatenateAudioFiles(tempFilesList, concatenatedFilePath);

            string finalText = await TranslateAudioToText(concatenatedFilePath, settings);

            File.Delete(concatenatedFilePath);

            return finalText;
        }
    }
}
