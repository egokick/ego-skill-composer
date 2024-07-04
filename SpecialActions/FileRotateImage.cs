using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using skill_composer.Models;
using skill_composer.Helper;
using static System.Drawing.Image;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace skill_composer.SpecialActions
{
    public class ImageRotate : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
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
