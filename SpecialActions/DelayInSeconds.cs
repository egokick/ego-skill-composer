using skill_composer.Models; 
using Task = System.Threading.Tasks.Task;

namespace skill_composer.SpecialActions
{
    public class DelayInSeconds : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            // Check if the input is valid and contains a number
            if (!string.IsNullOrEmpty(task.Input) && int.TryParse(task.Input, out int seconds))
            {
                // Wait for the specified number of seconds
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                task.Output = $"Waited for {seconds} seconds successfully.";
            }
            else
            {
                task.Output = "Invalid input. Please provide a valid number of seconds.";
            }

            return task;
        }
    }
}
