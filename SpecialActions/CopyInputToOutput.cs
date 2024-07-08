using skill_composer.Models;
using skill_composer.Helper;

namespace skill_composer.SpecialActions
{
    public class CopyInputToOutput : ISpecialAction
    { 
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            if (!string.IsNullOrEmpty(task.Input))
            {
                task.Output = task.Input;
            }
            
            return task;
        }
    }
}
