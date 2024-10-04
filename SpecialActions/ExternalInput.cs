using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class ExternalInput : ISpecialAction
    {
        // place holder consider dynamically constructing a form from task.Input when the SpecialAction is ExternalInput
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {            
            return task;
        }
    }
}
