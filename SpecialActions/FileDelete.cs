using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class FileDelete : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (File.Exists(task.Input))
            {
                File.Delete(task.Input);
                task.Output = $"File deleted: {task.Input}";                
            }

            return task;
        }
    }
}
