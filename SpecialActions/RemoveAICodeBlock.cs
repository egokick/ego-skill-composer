using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class RemoveAICodeBlock : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var lines = task.Input.Split(new[] { "\n" }, StringSplitOptions.None);

            var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("```")).ToArray();

            task.Output = string.Join("\n", filteredLines);

            return task;
        }
    }
}
