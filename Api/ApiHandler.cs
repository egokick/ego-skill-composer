using Flurl.Http;
using Polly;
using skill_composer.Models;
using Task = System.Threading.Tasks.Task;

namespace skill_composer.Helper
{
    public class ApiHandler
    {
        private static Settings _settings;

        public ApiHandler(Settings settings)
        {
            _settings = settings;
        }

        public async Task<string> GetAIResponse(string userInput, string filePath = null, Skill skill = null, bool isVerifierMode = false)
        {
            Image image = null;

            if (filePath is not null && (filePath.EndsWith(".jpg") || filePath.EndsWith(".png")))
            {
                image = new Image()
                {
                    FilePath = filePath
                };
            }

            var apiUrl = _settings.AiUrl;

            //if (!string.IsNullOrEmpty(_settings.OpenAiApiVersion))
            //{
            //    apiUrl = apiUrl.AppendQueryParam("api-version", _settings.OpenAiApiVersion);
            //}

            var _model = string.IsNullOrEmpty(skill?.OpenAiModel) ? _settings.OpenAiModel : skill?.OpenAiModel;

            var messages = new List<object>();

            if (image is not null)
            {
                if (!string.IsNullOrEmpty(image.Url))
                {
                    messages.Add(new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = userInput },
                            new { type = "image_url", image_url = new { url = image.Url } }
                        }
                    });
                }
                else
                {
                    messages.Add(new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = userInput },
                            new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{image.Base64EncodedImage}" } }
                        }
                    });
                }
            }
            else
            {
                messages.Add(new { role = "user", content = userInput });
            }

            var requestData = new
            {
                max_tokens = 4096,
                model = _model,
                messages = messages.ToArray(),
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
                        .WithHeader("use-case", "automated servicenow labelling that replaces 3 teams of people")
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