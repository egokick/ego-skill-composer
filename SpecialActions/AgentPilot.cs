using Newtonsoft.Json;
using skill_composer.Models;
using skill_composer.Helper;
using Task = System.Threading.Tasks.Task;
using System.IO;
using System.Linq;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Implements an “agent pilot” mode that continuously consults the AI while still accepting user input.
    /// The mode runs until the user types "stop". User instructions are appended to the original goal.
    /// The agent loads the Skills.json file and provides a list of all available skill names and their descriptions,
    /// as well as the content of the SpecialActionRegistry file, in the context sent to the AI.
    /// If the AI response includes a command formatted as "InvokeSkill: <SkillName>", the agent will load that skill
    /// from the Skills.json file, provide you with its content, and for any task in that skill with Mode "User",
    /// ask the AI the question specified in its Input, assign the response to that task's Output,
    /// then execute the skill and feed its output back to the AgentPilot loop.
    /// Additionally, whenever the conversation history is updated, the AI is called to provide a one-sentence summary.
    /// 
    /// > **Additional Context:**  
    /// > The code for any existing SpecialAction can be read by calling a skill with the SpecialAction
    /// > `FilePathsGetByDirectory` (to retrieve the file paths) followed by a task with the SpecialAction `FilesRead`
    /// > (to read the file content). This makes it possible to inspect or audit any SpecialAction's implementation.
    /// </summary>
    public class AgentPilot : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var generalKnowledge = "If you are going to create a new skill, you should plan out what you want to do and write the code for the SpecialActions, you can then call the existing skill '_SpecialAction_Design' to create that SpecialAction";
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

            // Load the content of SpecialActionRegistry.cs file to include in the context.
            string specialActionRegistryContent = "";
            try
            {
                string registryPath = @"C:\source\ego-skill-composer\Helper\SpecialActionRegistry.cs";
                if (File.Exists(registryPath))
                {
                    specialActionRegistryContent = File.ReadAllText(registryPath);
                }
                else
                {
                    specialActionRegistryContent = "SpecialActionRegistry.cs not found at " + registryPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading SpecialActionRegistry.cs: {ex.Message}");
            }

            // Use task.Output as the initial goal (set by the user) rather than task.Input.
            string originalGoal = task.Output?.Trim() ?? "";
            // If no initial goal is provided, repeatedly prompt until one is received.
            while (string.IsNullOrWhiteSpace(originalGoal))
            {
                Console.WriteLine(task.Input); // "Enter your initial goal for the Agent Pilot mode:"
                originalGoal = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(originalGoal))
                {
                    Console.WriteLine("You must enter a nonempty goal.");
                }
                else if (originalGoal.ToLower() == "stop")
                {
                    Console.WriteLine("User requested to stop Agent Pilot mode.");
                    task.Output = "Agent Pilot mode ended.";
                    return task;
                }
            }
            // Record the initial goal in conversation history.
            var conversationHistory = new System.Collections.Generic.List<string>
            {
                $"User set initial goal: {originalGoal}"
            };

            int callCount = 0;
            // Store the complete output from the last invoked skill.
            string fullPreviousSkillOutput = string.Empty;
            // For console display, we may show a one-sentence summary.
            string previousSkillSummary = string.Empty;
            // Track conversation history count for summarization.
            int lastSummarizedCount = conversationHistory.Count;

            Console.WriteLine("Agent Pilot mode started. Type new instructions to append or 'stop' to exit.");

            // Helper method to update summary if conversationHistory has new entries.
            async Task UpdateSummaryIfNeeded()
            {
                if (conversationHistory.Count > lastSummarizedCount)
                {
                    string conversationSnippet = GetLastConversationSnippet(conversationHistory, 5);
                    string summaryPrompt = $"Please summarize the last action taken by the AI and what has happened so far in Agent Pilot mode. Keep the summary brief, 1 sentence maximum. " +
                                           $"Conversation snippet: {conversationSnippet}";
                    string aiSummary = await Program.api.GetAIResponse(summaryPrompt);
                    previousSkillSummary = GetOneSentenceSummary(aiSummary);
                    Console.WriteLine($"[AI Summary]: {previousSkillSummary}");
                    lastSummarizedCount = conversationHistory.Count;
                }
            }

            // Main loop for Agent Pilot mode.
            while (true)
            {
                // Check for user input without blocking.
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
                            originalGoal += " " + userInput;
                            conversationHistory.Add($"User appended instructions: {userInput}");
                            await UpdateSummaryIfNeeded();
                        }
                    }
                }

                // Build the prompt for the AI call.
                // Here we include the complete output of the last invoked skill without summarization.
                string summarizedHistory = SummarizeHistory(conversationHistory);
                string prompt = $"API AI Call Count: {callCount}\n" +
                                $"General Info: {generalKnowledge}\n" +
                                $"Available Skills:\n{availableSkillsContext}\n" +
                                $"SpecialActionRegistry Content:\n{specialActionRegistryContent}\n" +
                                $"Conversation History: {summarizedHistory}\n" +
                                $"Previous Skill Full Output: {fullPreviousSkillOutput}\n" +
                                "Determine the next action to achieve the goal. " +
                                "If you want to invoke another skill, respond with 'InvokeSkill: <SkillName>' at the beginning of your response. " +
                                "When you invoke a skill, the agent will load that skill from the Skills.json file, " +
                                "provide you with its content, and for any task in that skill with Mode 'User', " +
                                "the agent will ask you the question specified in its 'Input', use your response as that task's output, " +
                                "then execute the skill and return its output back to you. " +
                                "If you do not wish to invoke a skill, simply provide your next plan.\n" +
                                $"Goal: {originalGoal}";

                // Call the AI.
                string aiResponse = await Program.api.GetAIResponse(prompt);
                callCount++;
                conversationHistory.Add($"AI Call {callCount}: {aiResponse}");
                await UpdateSummaryIfNeeded();

                // Check for skill invocation.
                if (aiResponse.IndexOf("InvokeSkill:", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    int idx = aiResponse.IndexOf("InvokeSkill:", System.StringComparison.OrdinalIgnoreCase);
                    string commandPart = aiResponse.Substring(idx);
                    string[] parts = commandPart.Split(new char[] { ':' }, 2);
                    if (parts.Length > 1)
                    {
                        string skillName = parts[1].Trim().TrimEnd('.', '!', '?');
                        try
                        {
                            string skillFilePath = FilePathHelper.GetSkillFilePath();
                            string skillsJson = System.IO.File.ReadAllText(skillFilePath);
                            SkillSet skillSet = JsonConvert.DeserializeObject<SkillSet>(skillsJson);
                            Skill? invokedSkill = skillSet?.Skills?.FirstOrDefault(
                                s => string.Equals(s.SkillName, skillName, System.StringComparison.OrdinalIgnoreCase));

                            if (invokedSkill != null)
                            {
                                string skillContent = JsonConvert.SerializeObject(invokedSkill, Formatting.Indented);
                                conversationHistory.Add($"Invoked skill '{skillName}' content: {skillContent}");
                                await UpdateSummaryIfNeeded();

                                foreach (var t in invokedSkill.Tasks.Where(t => t.Mode.Equals("User", System.StringComparison.OrdinalIgnoreCase)))
                                {
                                    // Build a context for the task call including available skills, conversation history, goal, full previous output and registry content.
                                    string taskContext = $"Available Skills:\n{availableSkillsContext}\n" +
                                                         $"General Info: {generalKnowledge}\n" +
                                                         $"SpecialActionRegistry Content:\n{specialActionRegistryContent}\n" +
                                                         $"Conversation History: {summarizedHistory}\n" +
                                                         $"Original Goal: {originalGoal}\n" +
                                                         $"Previous Skill Full Output: {fullPreviousSkillOutput}\n" +
                                                         $"Task Input: {t.Input}";
                                    string responseForTask = await Program.api.GetAIResponse(taskContext);
                                    t.Output = responseForTask;
                                    conversationHistory.Add($"AI provided response for invoked skill '{skillName}' task '{t.Name}': {responseForTask}");
                                    await UpdateSummaryIfNeeded();
                                }

                                // Prevent console logging; the AI will summarize it.
                                invokedSkill.Tasks.ForEach(x => x.PrintOutput = false);
                                string invokedOutput = Program.ProcessSkill(invokedSkill);
                                // Update the complete output variable with the full result.
                                fullPreviousSkillOutput = invokedOutput;
                                conversationHistory.Add($"Invoked skill '{skillName}' executed with output: {invokedOutput}");
                                await UpdateSummaryIfNeeded();
                            }
                            else
                            {
                                Console.WriteLine($"Skill '{skillName}' not found in the skills file.");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine($"Error invoking skill '{skillName}': {ex.Message}");
                        }
                    }
                    else
                    {
                        fullPreviousSkillOutput = aiResponse;
                    }
                }
                else
                {
                    fullPreviousSkillOutput = aiResponse;
                }

                await Task.Delay(1000);
            }

            task.Output = "Agent Pilot mode ended.";
            return task;
        }

        private string SummarizeHistory(System.Collections.Generic.List<string> conversationHistory)
        {
            int count = conversationHistory.Count;
            if (count > 10)
            {
                return string.Join(" | ", conversationHistory.Skip(count - 10));
            }
            return string.Join(" | ", conversationHistory);
        }

        private string GetOneSentenceSummary(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            var sentences = text.Split(new[] { '.', '!', '?' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (sentences.Length > 0)
            {
                return sentences[0].Trim() + ".";
            }
            return text;
        }

        private string GetLastConversationSnippet(System.Collections.Generic.List<string> conversationHistory, int count)
        {
            if (conversationHistory.Count == 0)
                return "";
            var snippet = conversationHistory.Skip(System.Math.Max(0, conversationHistory.Count - count));
            return string.Join(" | ", snippet);
        }
    }
}
