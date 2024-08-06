using skill_composer.Models;
using skill_composer.Helper;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net.WebSockets;
using System.Text;

namespace skill_composer.SpecialActions
{
    public class ConversationModeStream : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if(task.AiResponseShared is null || task.UserResponseShared is null)
            {
                throw new NullReferenceException("task.AiResponseShared and or task.UserResponseShared is null, you must construct and set these in the task property when calling the SpecialAction ConversationModeStream");
            }

            if (Program.api is null)
            {
                Program.api = new ApiHandler();
            }

            var totalTime = GetTotalTime(task.Input);
            var requireAnswer = GetRequireAnswer(task.Input);
            var systemPrompt = GetSystemPrompt(task.Input);
            var conversationHistory = GetConversationHistory(task.Input);
            var objective = GetObjective(task.Input);

            var stopwatch = Stopwatch.StartNew();
            var conversationList = new List<Dictionary<string, string>>();
             
            while (true)
            {
                if (task.cancellationToken is not null && task.cancellationToken.IsCancellationRequested) 
                {
                    break;
                }

                string reasonToContinueInObjective = "";
                // Check for timeout and append text if necessary
                if (!requireAnswer && stopwatch.Elapsed.TotalSeconds >= totalTime)
                {
                    systemPrompt += "\nIMPORTANT!. YOU ARE OUT OF TIME FOR THIS TOPIC, YOU MUST RESPOND ONLY WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                    objective += "\nIMPORTANT!. YOU ARE OUT OF TIME FOR THIS TOPIC, YOU MUST RESPOND ONLY WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                }
                else if (requireAnswer && conversationList.Count > 0)
                {
                    var checkIfTopicIsComplete = $"ConversationHistory:{conversationHistory}\n\n" +
                        $"Objective:{objective}\n" +
                        $"\n\n0)Carefully review the conversation history. 1) Respond with all the information the User has provided that is related to the Objective." +
                        $" 2) Explain if the objective has been completed with the information the user has provided." +
                        $" 3) End your explanation with the word 'true'" +
                        $" 4) If the objective has NOT been completed yet, end your explanation with 'false'" +
                        $" 5) DO NOT argue or disagree with the user!" +
                        $" 6) DO NOT ask the user to confirm more than one time!" +
                        " 7) If asking for a number, accept a response where the number is written out in words, you do not require numerical digits." +
                        " 8) If asking for a postcode, a phone number or an email address, make sure you confirm the answer with the user."; 

                    var isTopicComplete = await Program.api.GetAIResponse(checkIfTopicIsComplete);

                    string lastLine = isTopicComplete.Split('\n').Last().Trim().ToLower();

                    // Remove punctuation using Regex
                    string cleanedLastLine = Regex.Replace(lastLine, @"[^\w\s]", "");

                    // Split the cleaned line into words and get the last word
                    string lastWord = cleanedLastLine.Split(' ').Last();
                    // the answer has been provided, we can progress to the next topic.
                    if (lastWord != "false")
                    {
                        systemPrompt += "\nIMPORTANT!. The answer has been provided, RESPOND WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                        objective += "\nIMPORTANT!. The answer has been provided, RESPOND WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                    }
                    else
                    {
                        reasonToContinueInObjective = isTopicComplete;
                    }
                }

                // Construct user input with current conversation history
                var userInput = $"SystemPrompt:{systemPrompt},ReasonObjectiveIsStillPending:{reasonToContinueInObjective}\nTopic:{objective}\nConversationHistory:{conversationHistory}";

                string aiResponse;
                
                try
                {
                    aiResponse = await Program.api.GetAIResponse(userInput);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calling AI API: {ex.Message}");                    
                    throw;
                }

                if (aiResponse.Contains("{{NEXTTOPIC}}"))
                {
                    var justTheSpokenText = aiResponse.Replace("{{NEXTTOPIC}}", "");
                    if (justTheSpokenText.Length > 0) 
                    {
                        task.AiResponseShared.Enqueue(justTheSpokenText);
                        // Wait for the AI to stop talking - this is handled in the WebsocketService PlayAudio method.
                        while (task.AiResponseShared.TryPeek(out var _))
                        {
                            await System.Threading.Tasks.Task.Delay(10);
                        }

                        conversationHistory += $"\nAI: {justTheSpokenText}";
                        conversationList.Add(new Dictionary<string, string> { { "AI", justTheSpokenText }});
                    }                    
                    break;                   
                }

                // wait for the AI to stop talking before queueing another thing to say.
                while (task.AiResponseShared.TryPeek(out var _))
                {
                    await System.Threading.Tasks.Task.Delay(10);
                }

                // publishes the AiResponse so that it's available outside of this class
                task.AiResponseShared.Enqueue(aiResponse);

                // Wait for the AI to stop talking - this is handled in the WebsocketService PlayAudio method.
                while (task.AiResponseShared.TryPeek(out var _))
                {
                    await System.Threading.Tasks.Task.Delay(10);
                }

                string? userResponse = null;
                // wait for the user response, it may take a few seconds for the user to complete their sentence and the audio to be transcribed
                while (true)
                {                    
                    task.UserResponseShared.TryDequeue(out userResponse);

                    if (userResponse is null) 
                    {
                        if (!requireAnswer && stopwatch.Elapsed.TotalSeconds >= totalTime) break;
                        await System.Threading.Tasks.Task.Delay(10); 
                        continue; 
                    }
                    break;
                };
                  
                conversationHistory += $"\nAI: {aiResponse}\nUser: {userResponse}";

                conversationList.Add(new Dictionary<string, string> { { "AI", aiResponse }, { "User", userResponse } });
            }

            task.Output = JsonConvert.SerializeObject(conversationList, Formatting.Indented);

            return task;
        }

        private int GetTotalTime(string input)
        {
            var match = Regex.Match(input, @"TotalTime:(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 60;
        }

        private bool GetRequireAnswer(string input)
        {
            var match = Regex.Match(input, @"RequireAnswer:(true|false)");
            return match.Success && bool.Parse(match.Groups[1].Value);
        }

        private string GetSystemPrompt(string input)
        {
            var match = Regex.Match(input, @"SystemPrompt:(.*?)\n(ConversationHistory:|Objective:|$)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private string GetConversationHistory(string input)
        {
            var match = Regex.Match(input, @"ConversationHistory:(.*?)\nObjective:", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private string GetObjective(string input)
        {
            var match = Regex.Match(input, @"Objective:(.*)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
    }
}