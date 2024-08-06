using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public interface ISpecialAction
    {
        Task<Models.Task> Execute(Models.Task task, Skill selectedSkill);
     
    }
}