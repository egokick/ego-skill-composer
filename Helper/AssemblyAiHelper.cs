﻿using Flurl.Http;
using Newtonsoft.Json;
using skill_composer.Models;
using System.Diagnostics; 

namespace skill_composer.Helper
{
    public static class AssemblyAiHelper
    {
        public static async Task<string> GetAssemblyAiWebsocketTemporaryToken(int expiresIn)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var requestUri = "https://api.assemblyai.com/v2/realtime/token";
            var responseString = await requestUri
                .WithHeader("authorization", Settings.AssemblyAIApiKey)
                .PostJsonAsync(new { expires_in = expiresIn })
                .ReceiveString();

            var tokenResponse = JsonConvert.DeserializeObject<TemporaryTokenResponse>(responseString);

            stopwatch.Stop(); 

            return tokenResponse?.token ?? throw new InvalidOperationException("Failed to get the temporary token.");
        }

    }
}
