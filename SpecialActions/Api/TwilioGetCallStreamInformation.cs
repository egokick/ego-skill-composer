using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class TwilioGetCallStreamInformation : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            task.Output = task.Input;

            return task;
        }
    }
}
