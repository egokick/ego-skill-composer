using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class CopyInputToOutput : ISpecialAction
    { 
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (!string.IsNullOrEmpty(task.Input))
            {
                task.Output = task.Input;
            }
            
            return task;
        }
    }
}
