using Amazon.S3;
using Amazon.S3.Model;
using skill_composer.Models;
using skill_composer.Helper;
using System;
using System.IO;
using System.Threading.Tasks;

namespace skill_composer.SpecialActions
{
    public class FileS3Upload : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (string.IsNullOrEmpty(task.Input))
            {
                task.Output = "Task input is empty. Please provide the S3 bucket name, S3 bucket key, and file path.";
                return task;
            }

            var inputs = task.Input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (inputs.Length != 3)
            {
                task.Output = "Invalid input format. Please provide S3 bucket name, S3 bucket key, and file path separated by commas.";
                return task;
            }

            var awsS3BucketName = inputs[0].Trim();
            var s3BucketKey = inputs[1].Trim();
            var filePath = inputs[2].Trim();

            if (string.IsNullOrEmpty(awsS3BucketName) || string.IsNullOrEmpty(s3BucketKey) || string.IsNullOrEmpty(filePath))
            {
                task.Output = "S3 bucket name, S3 bucket key, or file path is empty.";
                return task;
            }

            if (!File.Exists(filePath))
            {
                task.Output = $"File not found: {filePath}";
                return task;
            }

            var s3Client = new AmazonS3Client();

            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = awsS3BucketName,
                    Key = s3BucketKey,
                    FilePath = filePath,
                    ContentType = "application/octet-stream"
                };

                var response = await s3Client.PutObjectAsync(putRequest);

                task.Output = $"File uploaded successfully to S3 bucket '{awsS3BucketName}' with key '{s3BucketKey}'.";
            }
            catch (Exception ex)
            {
                task.Output = $"Error uploading file to S3: {ex.Message}";
            }

            return task;
        }
    }
}
