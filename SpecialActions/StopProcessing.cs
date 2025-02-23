using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class StopProcessing : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (!string.IsNullOrEmpty(task.Input) && task.Input.Trim().ToLower() == "stop")
            {
                task.HaltProcessing = true;
            }

            return task;
        }
    }
}