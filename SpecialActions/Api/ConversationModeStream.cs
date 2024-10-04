using skill_composer.Models;
using skill_composer.Helper;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;

namespace skill_composer.SpecialActions
{
    public class ConversationModeStream : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (task.AiResponseShared is null || task.UserResponseShared is null)
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
                string reasonToContinueInObjective = "";
                string aiResponse = null;
                string userInput = null;

                if (!requireAnswer && stopwatch.Elapsed.TotalSeconds >= totalTime)
                {
                    systemPrompt += "\nIMPORTANT!. YOU ARE OUT OF TIME FOR THIS TOPIC, YOU MUST RESPOND ONLY WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                    objective += "\nIMPORTANT!. YOU ARE OUT OF TIME FOR THIS TOPIC, YOU MUST RESPOND ONLY WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                }

                if (task.UserResponseShared.Count() == 0)
                {
                    Task<string> objectiveTask = Task.Run(() => "false");

                    if (requireAnswer && conversationList.Count > 0)
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

                        // Run task without awaiting 
                        objectiveTask = Program.api.GetAIResponse(checkIfTopicIsComplete);
                    } 

                    userInput = $"SystemPrompt:{systemPrompt},ReasonObjectiveIsStillPending:{reasonToContinueInObjective}\nTopic:{objective}\nConversationHistory:{conversationHistory}";
                    
                    // Run task without awaiting
                    var aiResponseTask = Program.api.GetAIResponse(userInput); 
                    
                    // Await both API calls for time efficiency
                    _ = await Task.WhenAll(objectiveTask, aiResponseTask);

                    var isTopicComplete = objectiveTask.Result;

                    string lastLine = isTopicComplete.Split('\n').Last().Trim().ToLower();

                    string cleanedLastLine = Regex.Replace(lastLine, @"[^\w\s]", "");

                    string lastWord = cleanedLastLine.Split(' ').Last();

                    if (lastWord == "false")
                    {
                        reasonToContinueInObjective = isTopicComplete;
                    }
                    else
                    {
                        systemPrompt += "\nIMPORTANT!. The answer has been provided, RESPOND WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                        objective += "\nIMPORTANT!. The answer has been provided, RESPOND WITH '{{NEXTTOPIC}}'. IMPORTANT!";
                        userInput = $"SystemPrompt:{systemPrompt},ReasonObjectiveIsStillPending:{reasonToContinueInObjective}\nTopic:{objective}\nConversationHistory:{conversationHistory}";
                       
                        // This only runs twice when the objective has been completed - this is to reduce the latency in the response times of the AI
                        aiResponseTask = Program.api.GetAIResponse(userInput);
                    } 

                    aiResponse = aiResponseTask.Result;

                    // Check if the AI has decided this objective is completed and this SpecialAction can end
                    if (aiResponse.Contains("{{NEXTTOPIC}}"))
                    {
                        var justTheSpokenText = aiResponse.Replace("{{NEXTTOPIC}}", "");

                        if (justTheSpokenText.Length > 0)
                        {
                            task.AiResponseShared.Enqueue(justTheSpokenText);

                            while (task.AiResponseShared.TryPeek(out var _))
                            {
                                await System.Threading.Tasks.Task.Delay(1);
                            }

                            conversationHistory += $"\nAI: {justTheSpokenText}";
                            conversationList.Add(new Dictionary<string, string> { { "AI", justTheSpokenText } });
                        }
                        break;
                    }

                    task.AiResponseShared.Enqueue(aiResponse);
                } 
                       
                string? userResponse = null;

                // Wait and get the user response string
                while (true)
                {
                    if (!requireAnswer && stopwatch.Elapsed.TotalSeconds >= totalTime) break;

                    if (task.UserResponseShared.TryDequeue(out var tmp))
                    {
                        if (tmp.StartsWith("PartialTranscript:"))
                        {
                            while (task.UserResponseShared.Count() == 0)
                            {
                                await System.Threading.Tasks.Task.Delay(2);
                            }
                        }
                        else if (tmp.StartsWith("FinalTranscript:"))
                        {
                            userResponse += tmp.Substring("FinalTranscript:".Length);

                            if (task.UserResponseShared.Count() == 0)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        await System.Threading.Tasks.Task.Delay(10);
                    }
                };

                if (aiResponse is not null) conversationHistory += $"\nAI: {aiResponse}";
                if (userResponse is not null) conversationHistory += $"\nUser: {userResponse}";

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
            var conversationHistory = match.Success ? match.Groups[1].Value.Trim() : string.Empty;
            conversationHistory = conversationHistory.Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("\r\n","")                
                .Replace("    ","")
                .Replace("\\n\\n","")
                .Replace("\"AI\": null,","").Replace("\"User\": null","")
                .Replace("\"User\":", "\n\"User\":")
                .Replace("\"AI\":", "\n\"AI\":");

            return conversationHistory;
        }

        private string GetObjective(string input)
        {
            var match = Regex.Match(input, @"Objective:(.*)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
    }
}
