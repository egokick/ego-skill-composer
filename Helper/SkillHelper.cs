using Amazon.S3.Model;
using Amazon.S3;
using Newtonsoft.Json;
using skill_composer.Models;
using Amazon;

namespace skill_composer.Helper
{
    public static class SkillHelper
    {
        private static readonly string bucketName = "projectsecretary-063207042820";
        private static readonly string keyName = "apps/codedeploy/ecs/secretaryapi/skills.json";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUWest1; // Replace with your region
        private static readonly IAmazonS3 s3Client = new AmazonS3Client(bucketRegion);
        private static readonly Settings _settings = new Settings();      

        public static bool ProcessSkill(Skill skill)
        {           
            Program._settings = _settings;

            Program.ProcessSkill(skill);

            return true;
        }

        public static async Task<bool> ProcessSkillByName(string skillName)
        {
            try
            {
                // Get the latest SkillSet from S3
                SkillSet skillSet = await GetSkillsFromS3();

                // Find the skill by name
                var skillToProcess = skillSet.Skills.Find(s => s.SkillName == skillName);
                if (skillToProcess != null)
                { 
                    ProcessSkill(skillToProcess);
                    return true;
                }
                else
                {
                    Console.WriteLine("Skill with name '{0}' not found.", skillName);
                    return false;
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when processing skill", e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when processing skill", e.Message);
                throw;
            }
        }
        public static async Task<List<string>> GetSkillNames()
        {
            try
            {
                // Get the latest SkillSet from S3
                SkillSet skillSet = await GetSkillsFromS3();

                // Extract the skill names
                List<string> skillNames = skillSet.Skills.Select(s => s.SkillName).ToList();

                return skillNames;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when retrieving skill names", e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when retrieving skill names", e.Message);
                throw;
            }
        } 

        public static async Task<SkillSet> GetSkillsFromS3()
        {
            //    string json = File.ReadAllText("C:\\source\\ProjectSecretary\\secretary-api\\skills.json");
            //    SkillSet ss = JsonConvert.DeserializeObject<SkillSet>(json);
            //    return ss;

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string jsonResponse = reader.ReadToEnd();
                    SkillSet skillSet = JsonConvert.DeserializeObject<SkillSet>(jsonResponse);
                    return skillSet;
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when reading object", e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when reading object", e.Message);
                throw;
            }
        }

        public static async Task<bool> SaveSkill(Skill skill)
        {
            try
            {
                // Get the latest SkillSet from S3
                SkillSet skillSet = await GetSkillsFromS3();

                // Check if the SkillName exists and update or add the skill
                var existingSkill = skillSet.Skills.Find(s => s.SkillName == skill.SkillName);
                if (existingSkill != null)
                {
                    // Update the existing skill
                    existingSkill.Description = skill.Description;
                    existingSkill.OpenAiModel = skill.OpenAiModel;
                    existingSkill.Temperature = skill.Temperature;
                    existingSkill.RepeatCount = skill.RepeatCount;
                    existingSkill.Tasks = skill.Tasks;
                    existingSkill.DisableFileLogging = skill.DisableFileLogging;
                    existingSkill.AppendFileLogging = skill.AppendFileLogging;
                }
                else
                {
                    // Add the new skill
                    skillSet.Skills.Add(skill);
                }

                // Serialize the updated SkillSet to a JSON string
                string updatedJson = JsonConvert.SerializeObject(skillSet, Formatting.Indented);

                // Create a PutObjectRequest to upload the new JSON string to S3
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    ContentBody = updatedJson,
                    ContentType = "application/json"
                };

                // Upload the new JSON string to S3
                await s3Client.PutObjectAsync(putRequest);
                
                return true;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing object", e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing object", e.Message);
                throw;
            }
        }

        public static async Task<bool> DeleteSkill(string skillName)
        {
            try
            {
                // Get the latest SkillSet from S3
                SkillSet skillSet = await GetSkillsFromS3();

                // Find the skill to delete
                var skillToDelete = skillSet.Skills.Find(s => s.SkillName == skillName);
                if (skillToDelete != null)
                {
                    // Remove the skill from the list
                    skillSet.Skills.Remove(skillToDelete);

                    // Serialize the updated SkillSet to a JSON string
                    string updatedJson = JsonConvert.SerializeObject(skillSet, Formatting.Indented);

                    // Create a PutObjectRequest to upload the new JSON string to S3
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = keyName,
                        ContentBody = updatedJson,
                        ContentType = "application/json"
                    };

                    // Upload the new JSON string to S3
                    await s3Client.PutObjectAsync(putRequest);
                    
                    return true;
                }
                else
                {
                    Console.WriteLine("Skill with name '{0}' not found.", skillName);
                    return false;
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing object", e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing object", e.Message);
                throw;
            }
        }
        public static async Task<bool> RenameSkill(string currentSkillName, string newSkillName)
        {
            try
            {
                // Get the latest SkillSet from S3
                SkillSet skillSet = await GetSkillsFromS3();

                // Find the skill to rename
                var skillToRename = skillSet.Skills.Find(s => s.SkillName == currentSkillName);
                if (skillToRename != null)
                {
                    // Rename the skill
                    skillToRename.SkillName = newSkillName;

                    // Serialize the updated SkillSet to a JSON string
                    string updatedJson = JsonConvert.SerializeObject(skillSet, Formatting.Indented);

                    // Create a PutObjectRequest to upload the new JSON string to S3
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = keyName,
                        ContentBody = updatedJson,
                        ContentType = "application/json"
                    };

                    // Upload the new JSON string to S3
                    await s3Client.PutObjectAsync(putRequest);

                    return true;
                }
                else
                {
                    Console.WriteLine("Skill with name '{0}' not found.", currentSkillName);
                    return false;
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing object", e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing object", e.Message);
                throw;
            }
        }

        public static async Task<Skill> GetSkillByName(string skillName)
        {
            try
            {

                // Get the latest SkillSet from S3
                SkillSet skillSet = await GetSkillsFromS3();

                // Find the skill by name
                var skill = skillSet.Skills.Find(s => s.SkillName == skillName);
                if (skill != null)
                {
                    return skill;
                }
                else
                {
                    Console.WriteLine("Skill with name '{0}' not found.", skillName);
                    return null;
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when retrieving skill", e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when retrieving skill", e.Message);
                throw;
            }
        }


    }
}
