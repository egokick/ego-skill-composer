using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class WebUrlDownload : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (string.IsNullOrEmpty(task.Input))
            {
                task.Output = "Task input is empty. Please provide the URL to download.";
                return task;
            }

            var inputParts = task.Input.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (inputParts.Length == 0)
            {
                task.Output = "Invalid input format. Please provide the URL and optionally any headers or cookies.";
                return task;
            }

            var url = inputParts[0].Trim();

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) || !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                task.Output = "Invalid URL format. Please provide a valid URL.";
                return task;
            }

            using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
            using (var client = new HttpClient(handler))
            {
                // Set default headers to spoof a browser
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

                // Parse additional headers and cookies if provided
                for (int i = 1; i < inputParts.Length; i++)
                {
                    var part = inputParts[i].Trim();
                    if (part.StartsWith("header:", StringComparison.OrdinalIgnoreCase))
                    {
                        var headerParts = part.Substring(7).Split(new[] { ':' }, 2);
                        if (headerParts.Length == 2)
                        {
                            client.DefaultRequestHeaders.Add(headerParts[0].Trim(), headerParts[1].Trim());
                        }
                    }
                    else if (part.StartsWith("cookie:", StringComparison.OrdinalIgnoreCase))
                    {
                        var cookieParts = part.Substring(7).Split(new[] { '=' }, 2);
                        if (cookieParts.Length == 2)
                        {
                            handler.CookieContainer.Add(new Uri(url), new Cookie(cookieParts[0].Trim(), cookieParts[1].Trim()));
                        }
                    }
                    else if (part.StartsWith("token:", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = part.Substring(6).Trim();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }

                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var responseContent = response.Content;
                    var pageContent = await responseContent.ReadAsStringAsync();

                    if (responseContent.Headers.ContentEncoding.Contains("gzip"))
                    {
                        using (var decompressedStream = new GZipStream(await responseContent.ReadAsStreamAsync(), CompressionMode.Decompress))
                        using (var reader = new StreamReader(decompressedStream))
                        {
                            pageContent = await reader.ReadToEndAsync();
                        }
                    }

                    task.Output = pageContent;
                }
                catch (Exception ex)
                {
                    task.Output = $"Error downloading URL: {ex.Message}";
                }
            }

            return task;
        }
    }
}
