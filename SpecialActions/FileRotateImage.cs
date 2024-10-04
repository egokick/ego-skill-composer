using System.Drawing;
using System.Drawing.Imaging;
using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class ImageRotate : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            // Extract numbers from task.Input
            if (task.Output.ToLower().Contains("rotate"))
            {
                using (var image = System.Drawing.Image.FromFile(task.FilePath))
                {
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    image.Save(task.FilePath, ImageFormat.Jpeg); 
                }
            } 

            return task;
        }
    }
}
