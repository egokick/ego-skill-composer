using skill_composer.Models;
using skill_composer.Helper;
using System.Text;
using Flurl.Http;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;

namespace skill_composer.SpecialActions
{
    public class TextToSpeech : ISpecialAction
    { 
        private static DateTime? _rateLimitResetTime = null;

        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var voiceModel = task.SpecialAction.Replace("TextToSpeech", "");
            if (string.IsNullOrEmpty(voiceModel))
            {
                voiceModel = "???";
            }
            var dataInputDirectory = FilePathHelper.GetDataInputDirectory();
            var outputDirectory = FilePathHelper.GetDataOutputDirectory();

            // send to openai for text to speech conversion: task.Input
            Console.WriteLine("Converting text to audio...");

            // Split the input into sentences.
            var sentences = Regex.Split(task.Input, @"(?<=[\.!\?])\s+");

            int fileIndex = 0;
            StringBuilder chunkBuilder = new StringBuilder();
            List<string> chunks = new List<string>();

            foreach (var sentence in sentences)
            {
                if (chunkBuilder.Length + sentence.Length > 4096)
                {
                    chunks.Add(chunkBuilder.ToString());
                    chunkBuilder.Clear();
                }

                chunkBuilder.Append(sentence).Append(" ");
            }

            // Add the last chunk if it's not empty.
            if (chunkBuilder.Length > 0)
            {
                chunks.Add(chunkBuilder.ToString());
            }

            // Convert each chunk to audio and save it.
            foreach (var chunk in chunks)
            {
                var audioBytes = ConvertTextToAudio(chunk, voiceModel).Result; // Assuming this is an async method

                var outputFilePath = Path.Combine(outputDirectory, $"audio_{++fileIndex}.mp3");
                File.WriteAllBytes(outputFilePath, audioBytes);

                Console.WriteLine($"Created {outputFilePath}.");
            }

            return task;
        }

        private static void CheckAndSetRateLimiting(IFlurlResponse response)
        {
            // Access headers directly from the response
            var headers = response.Headers;

            // Attempt to find the headers by name
            var remainingRequestsHeader = headers.FirstOrDefault(h => h.Name == "x-ratelimit-remaining-requests");
            var resetAfterHeader = headers.FirstOrDefault(h => h.Name == "x-ratelimit-reset-requests");

            // Check if the headers have non-empty values to determine if they were found
            if (!string.IsNullOrEmpty(remainingRequestsHeader.Value) && int.TryParse(remainingRequestsHeader.Value, out var remaining) && remaining == 0)
            {
                if (!string.IsNullOrEmpty(resetAfterHeader.Value) && resetAfterHeader.Value.EndsWith("s"))
                {
                    var seconds = int.Parse(resetAfterHeader.Value.TrimEnd('s'));
                    _rateLimitResetTime = DateTime.Now.AddSeconds(seconds);
                }
            }
        }
        private static async Task<byte[]> ConvertTextToAudio(string inputText, string voiceModel)
        {
            var apiUrl = "https://api.openai.com/v1/audio/speech";

            var requestBody = new
            {
                model = "tts-1-hd",
                input = inputText,
                voice = string.IsNullOrEmpty(voiceModel) ? "alloy" : voiceModel
            };

            // Check if we need to respect the rate limit before sending the request
            if (_rateLimitResetTime.HasValue && DateTime.Now < _rateLimitResetTime)
            {
                var delay = (_rateLimitResetTime.Value - DateTime.Now).Milliseconds;
                if (delay > 0)
                {
                    Console.WriteLine($"Rate limit exceeded. Waiting for {delay} ms");
                    await Task.Delay(delay);
                }
            }

            try
            {
                // First, send the request but don't immediately read the response body
                var response = await apiUrl
                    .WithHeader("Authorization", $"Bearer {Settings.OpenAiKey}")
                    .WithHeader("Content-Type", "application/json")
                    .WithTimeout(600) // Sets the timeout to 6 minutes
                    .PostJsonAsync(requestBody);

                // Log the headers and check rate limiting
                CheckAndSetRateLimiting(response);

                // Then, read the response as byte array
                var responseBytes = await response.GetBytesAsync();

                return responseBytes;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
                if (ex.Call.Response != null)
                {
                    var errorBody = await ex.GetResponseStringAsync();
                    Console.WriteLine("Error response body: " + errorBody + $" Status Code: {ex.Call.Response.StatusCode}");
                }
                else
                {
                    Console.WriteLine("Error: No response received");
                }
            }

            return new byte[0];
        }


    }
}
