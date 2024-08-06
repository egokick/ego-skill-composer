using skill_composer.Models;
using skill_composer.Helper;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace skill_composer.SpecialActions
{
    public class ConversationMode : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if(Program.api is null)
            {
                Program.api = new ApiHandler();
            }

            var totalTime = GetTotalTime(task.Input);
            var requireAnswer = GetRequireAnswer(task.Input);
            var systemPrompt = GetSystemPrompt(task.Input);
            var conversationHistory = GetConversationHistory(task.Input);
            var topic = GetTopic(task.Input);

            var stopwatch = Stopwatch.StartNew();
            var conversationList = new List<Dictionary<string, string>>();

            while (true)
            {
                // Check for timeout and append text if necessary
                if (!requireAnswer && stopwatch.Elapsed.TotalSeconds >= totalTime)
                {
                    systemPrompt += "\nIMPORTANT!. YOU ARE OUT OF TIME FOR THIS TOPIC, YOU MUST RESPOND ONLY WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                    topic += "\nIMPORTANT!. YOU ARE OUT OF TIME FOR THIS TOPIC, YOU MUST RESPOND ONLY WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                }
                else if (requireAnswer && conversationList.Count > 0)
                {
                    var checkIfTopicIsComplete = $"SystemPrompt:{systemPrompt}\nConversationHistory:{conversationHistory}\n\n" +
                        $"Topic:{topic}\n" +
                        $"\n\nReview the previous conversation and topic, if the information requested in the topic has been provided by the User" +
                        $" respond with an explanation of why the information has been provided by the user, remember it's just if the current specific topic has been answered, end your explanation with the word 'true'. Else explain why not and end your resposne with the text 'false'.";
                        
                    var topicIsComplete = await Program.api.GetAIResponse(checkIfTopicIsComplete);

                    string lastLine = topicIsComplete.Split('\n').Last().Trim().ToLower();

                    // Remove punctuation using Regex
                    string cleanedLastLine = Regex.Replace(lastLine, @"[^\w\s]", "");

                    // Split the cleaned line into words and get the last word
                    string lastWord = cleanedLastLine.Split(' ').Last();
                    // the answer has been provided, we can progress to the next topic.
                    if (lastWord != "false")
                    {
                        systemPrompt += "\nIMPORTANT!. The answer has been provided, RESPOND WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                        topic += "\nIMPORTANT!. The answer has been provided, RESPOND WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                    }
                }

                // Construct user input with current conversation history
                var userInput = $"SystemPrompt:{systemPrompt}\nConversationHistory:{conversationHistory}\nTopic:{topic}\n";                
               
                string aiResponse;
                try
                {
                    aiResponse = await Program.api.GetAIResponse(userInput);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calling AI API: {ex.Message}");
                    break;
                }

                if (aiResponse.Contains("{{NEXTTOPIC}}"))
                {
                    aiResponse = aiResponse.Replace("{{NEXTTOPIC}}", "");
                    conversationHistory += $"\nAI: {aiResponse}";
                    conversationList.Add(new Dictionary<string, string> { { "AI", aiResponse } });
                    Console.WriteLine($"AI: {aiResponse}");
                    break;
                }

                Console.WriteLine($"AI: {aiResponse}");

                var userResponse = Console.ReadLine();

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
            var match = Regex.Match(input, @"SystemPrompt:(.*?)\n(ConversationHistory:|Topic:|$)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private string GetConversationHistory(string input)
        {
            var match = Regex.Match(input, @"ConversationHistory:(.*?)\nTopic:", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private string GetTopic(string input)
        {
            var match = Regex.Match(input, @"Topic:(.*)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
    }
}