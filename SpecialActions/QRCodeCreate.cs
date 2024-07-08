using QRCoder;
using skill_composer.Models;
using skill_composer.Helper;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Generates a QR code from the provided input string in the task.Input.
    /// The task.Input can be a mailto URI, URL, or phone number.
    /// The generated QR code will encode the input string.
    /// </summary>
    public class QRCodeCreate : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            if (string.IsNullOrEmpty(task.Input))
            {
                task.HaltProcessing = true;
                return task;
            }

            var formattedQrString = task.Input;

            if (task.Input.Contains("mailto"))
            {
                formattedQrString = FormatMailTo(task.Input);
            }

            var qrCodeData = GenerateQRCode(task.Input);
            var qrCodeBase64 = Convert.ToBase64String(qrCodeData);

            var fileName = new string(Enumerable.Range(0, 10).Select(_ => (char)new Random().Next('a', 'z' + 1)).ToArray()) + ".png";

            var outputFilePath = Path.Combine(FilePathHelper.GetDataOutputDirectory(), fileName);

            task.FilePath = outputFilePath;

            var fileContent = $"data:image/png;base64,{qrCodeBase64}";

            File.WriteAllText(outputFilePath, fileContent);

            return task;
        }


        private byte[] GenerateQRCode(string input)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(input, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }

        public static string FormatMailTo(string template)
        {
            // Pattern to match {{Output[x]}}
            string pattern = @"{{Output\[(\d+)\]}}";

            // Matches in the template
            var matches = Regex.Matches(template, pattern);

            // Dictionary to store unique matches and their replacements
            Dictionary<string, string> replacements = new Dictionary<string, string>();

            foreach (Match match in matches)
            {
                string placeholder = match.Value;
                int index = int.Parse(match.Groups[1].Value);

                // Retrieve the corresponding value (for example, this could be from a list or another source)
                string value = GetOutputValue(index);

                // Escape the value
                string escapedValue = Uri.EscapeDataString(value);

                // Store the replacement
                replacements[placeholder] = escapedValue;
            }

            // Replace all placeholders with their escaped values
            foreach (var replacement in replacements)
            {
                template = template.Replace(replacement.Key, replacement.Value);
            }

            return template;
        }

        // Example method to retrieve values based on index
        public static string GetOutputValue(int index)
        {
            // Simulate values, replace this with actual retrieval logic
            string[] outputs = { "example@example.com", "Hello, World!", "This is the body of the email." };
            if (index >= 0 && index < outputs.Length)
            {
                return outputs[index];
            }
            return string.Empty;
        }

    }
}
