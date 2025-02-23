using Newtonsoft.Json;
using skill_composer.Models;
using skill_composer.Helper;
using Task = System.Threading.Tasks.Task;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Implements an “agent pilot” mode that continuously consults the AI while still accepting user input.
    /// The mode runs until the user types "stop". User instructions are appended to the original goal.
    /// The agent loads the Skills.json file and provides a list of all available skill names and their descriptions
    /// in the context sent to the AI. If the AI response includes a command formatted as "InvokeSkill: <SkillName>",
    /// the agent will load that skill from the Skills.json file, provide its content for context, and for any task in
    /// that skill with Mode "User", ask the AI the question specified in its Input, assign the response to that task's Output,
    /// then execute the skill and feed its output back to the AgentPilot loop.
    /// Additionally, a separate thread periodically calls the AI to summarize the most recent action and conversation.
    /// </summary>
    public class AgentPilot : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            // Load available skills list at the start
            string availableSkillsContext = "";
            try
            {
                string skillFilePath = FilePathHelper.GetSkillFilePath();
                string skillsJson = System.IO.File.ReadAllText(skillFilePath);
                SkillSet skillSet = JsonConvert.DeserializeObject<SkillSet>(skillsJson);
                if (skillSet != null && skillSet.Skills != null && skillSet.Skills.Count > 0)
                {
                    availableSkillsContext = string.Join("\n", skillSet.Skills.Select(s =>
                        $"Skill Name: {s.SkillName}\nDescription: {s.Description}\n"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading skills list: {ex.Message}");
            }

            // Initialize state variables
            int callCount = 0;
            List<string> conversationHistory = new List<string>();
            string originalGoal = task.Input;  // The initial goal provided in the task's Input
            string previousSkillOutput = string.Empty;

            Console.WriteLine("Agent Pilot mode started. Type new instructions to append or 'stop' to exit.");

            // Start a background task that calls the AI to generate a summary of the last action and recent conversation
            CancellationTokenSource summaryCts = new CancellationTokenSource();
            Task summaryTask = Task.Run(async () =>
            {
                while (!summaryCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Wait 5 seconds between summary calls
                        await Task.Delay(5000, summaryCts.Token);
                        string conversationSnippet = GetLastConversationSnippet(conversationHistory, 5);
                        string summaryPrompt = $"Please summarize the last action taken by the AI and what has happened so far in Agent Pilot mode. " +
                                               $"Conversation snippet: {conversationSnippet}";
                        string aiSummary = await Program.api.GetAIResponse(summaryPrompt);
                        Console.WriteLine($"\n[AI Summary]: {aiSummary}\n");
                    }
                    catch (TaskCanceledException) { break; }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during summarization: {ex.Message}");
                    }
                }
            }, summaryCts.Token);

            // Main loop for Agent Pilot mode
            while (true)
            {
                // Check for user input without blocking the loop
                if (Console.KeyAvailable)
                {
                    string userInput = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(userInput))
                    {
                        if (userInput.Trim().ToLower() == "stop")
                        {
                            Console.WriteLine("User requested to stop Agent Pilot mode.");
                            break;
                        }
                        else
                        {
                            // Append new instructions to the original goal
                            originalGoal += " " + userInput;
                            conversationHistory.Add($"User appended instructions: {userInput}");
                        }
                    }
                }

                // Build the prompt for the AI call, including available skills list
                string summarizedHistory = SummarizeHistory(conversationHistory);
                string prompt = $"Call Count: {callCount}\n" +
                                $"Goal: {originalGoal}\n" +
                                $"Available Skills:\n{availableSkillsContext}\n" +
                                $"Conversation History: {summarizedHistory}\n" +
                                $"Previous Skill Output: {previousSkillOutput}\n" +
                                "Determine the next action to achieve the goal. " +
                                "If you want to invoke another skill, respond with 'InvokeSkill: <SkillName>' at the beginning of your response. " +
                                "When you invoke a skill, the agent will load that skill from the Skills.json file, " +
                                "provide you with its content, and for any task in that skill with Mode 'User', " +
                                "the agent will ask you the question specified in its 'Input', use your response as that task's output, " +
                                "then execute the skill and return its output back to you. " +
                                "If you do not wish to invoke a skill, simply provide your next plan.";

                // Call the AI using only the prompt
                string aiResponse = await Program.api.GetAIResponse(prompt);
                callCount++;
                conversationHistory.Add($"AI Call {callCount}: {aiResponse}");

                // Display the AI response
                Console.WriteLine($"\n[Agent Pilot] AI Response:\n{aiResponse}\n");

                // Check if the AI response includes a command to invoke another skill
                if (aiResponse.IndexOf("InvokeSkill:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Extract the skill name from the response
                    int idx = aiResponse.IndexOf("InvokeSkill:", StringComparison.OrdinalIgnoreCase);
                    string commandPart = aiResponse.Substring(idx);
                    string[] parts = commandPart.Split(new char[] { ':' }, 2);
                    if (parts.Length > 1)
                    {
                        string skillName = parts[1].Trim();
                        // Remove any trailing punctuation
                        skillName = skillName.TrimEnd('.', '!', '?');
                        Console.WriteLine($"Invoking skill: {skillName}");

                        try
                        {
                            // Load the skills file using FilePathHelper.GetSkillFilePath
                            string skillFilePath = FilePathHelper.GetSkillFilePath();
                            string skillsJson = System.IO.File.ReadAllText(skillFilePath);
                            SkillSet skillSet = JsonConvert.DeserializeObject<SkillSet>(skillsJson);
                            Skill? invokedSkill = skillSet?.Skills?.FirstOrDefault(
                                s => string.Equals(s.SkillName, skillName, StringComparison.OrdinalIgnoreCase));

                            if (invokedSkill != null)
                            {
                                // Log the content of the invoked skill for context
                                string skillContent = JsonConvert.SerializeObject(invokedSkill, Formatting.Indented);
                                conversationHistory.Add($"Invoked skill '{skillName}' content: {skillContent}");
                                Console.WriteLine($"Loaded skill '{skillName}'.");

                                // For every task in the invoked skill that is in "User" mode,
                                // ask the AI the question from the task's Input and set the response as task.Output.
                                foreach (var t in invokedSkill.Tasks.Where(t =>
                                    t.Mode.Equals("User", StringComparison.OrdinalIgnoreCase)))
                                {
                                    Console.WriteLine($"For skill '{skillName}', asking AI for task '{t.Name}': {t.Input}");
                                    string responseForTask = await Program.api.GetAIResponse(t.Input);
                                    t.Output = responseForTask;
                                    conversationHistory.Add($"AI provided response for invoked skill '{skillName}' task '{t.Name}': {responseForTask}");
                                    Console.WriteLine($"Response for task '{t.Name}': {responseForTask}");
                                }

                                // Execute the invoked skill and retrieve its output
                                string invokedOutput = Program.ProcessSkill(invokedSkill);
                                previousSkillOutput = invokedOutput;
                                conversationHistory.Add($"Invoked skill '{skillName}' executed with output: {invokedOutput}");
                                Console.WriteLine($"Output from invoked skill '{skillName}': {invokedOutput}");
                            }
                            else
                            {
                                Console.WriteLine($"Skill '{skillName}' not found in the skills file.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error invoking skill '{skillName}': {ex.Message}");
                        }
                    }
                    else
                    {
                        // If no valid skill invocation was found, simply use the AI response as output.
                        previousSkillOutput = aiResponse;
                    }
                }
                else
                {
                    // No skill invocation; simply carry forward the AI response.
                    previousSkillOutput = aiResponse;
                }

                // Brief pause before the next iteration
                await System.Threading.Tasks.Task.Delay(1000);
            }

            // Stop the background summary task
            summaryCts.Cancel();
            try { await summaryTask; } catch (TaskCanceledException) { }

            task.Output = "Agent Pilot mode ended.";
            return task;
        }

        /// <summary>
        /// Returns a summary of the last up to 10 conversation history entries.
        /// </summary>
        private string SummarizeHistory(List<string> conversationHistory)
        {
            int count = conversationHistory.Count;
            if (count > 10)
            {
                return string.Join(" | ", conversationHistory.Skip(count - 10));
            }
            return string.Join(" | ", conversationHistory);
        }

        /// <summary>
        /// Extracts a one-sentence summary from the given text.
        /// </summary>
        private string GetOneSentenceSummary(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (sentences.Length > 0)
            {
                return sentences[0].Trim() + ".";
            }
            return text;
        }

        /// <summary>
        /// Returns a snippet consisting of the last 'count' entries from the conversation history.
        /// </summary>
        private string GetLastConversationSnippet(List<string> conversationHistory, int count)
        {
            if (conversationHistory.Count == 0)
                return "";
            var snippet = conversationHistory.Skip(Math.Max(0, conversationHistory.Count - count));
            return string.Join(" | ", snippet);
        }
    }
}
