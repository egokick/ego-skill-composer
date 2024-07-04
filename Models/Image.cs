using System;
using System.IO;

namespace skill_composer.Models
{
    public class Image
    {
        public string FilePath { get; set; }
        public string Url { get; set; }

        public string Base64EncodedImage
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath))
                {
                    try
                    {
                        byte[] imageBytes = File.ReadAllBytes(FilePath);
                        return Convert.ToBase64String(imageBytes);
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (e.g., log error, rethrow, return default value, etc.)
                        Console.WriteLine($"Error reading file: {ex.Message}");
                        throw; // or return null; or handle as appropriate
                    }
                }
                return null; // or handle as appropriate
            }
        }
    }
}
