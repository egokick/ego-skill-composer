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
        public static string GetTestInput() 
        {
            return "mailto:egokick@gmail.com?subject=you are a winrar&body=you did extremely well on the test!";
        }

        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
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

            File.WriteAllBytes(outputFilePath, qrCodeData);
            var imageBase64String = $"data:image/png;base64,{qrCodeBase64}";
            task.Output = imageBase64String;

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
         
            // Find the position of the query part (starts with '?')
            int queryIndex = template.IndexOf('?');
            if (queryIndex < 0)
            {
                // No query part, just return the template as it is
                return template;
            }

            // Extract the base mailto part and the query part
            string baseMailto = template.Substring(0, queryIndex);
            string queryPart = template.Substring(queryIndex + 1);

            // Split the query part into key-value pairs
            string[] queryParams = queryPart.Split('&');
            Dictionary<string, string> escapedParams = new Dictionary<string, string>();

            foreach (string param in queryParams)
            {
                string[] keyValue = param.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0];
                    string value = keyValue[1];

                    // Escape the value
                    string escapedValue = Uri.EscapeDataString(value);
                    escapedParams[key] = escapedValue;
                }
            }

            // Reconstruct the query part with escaped values
            List<string> escapedQueryParams = new List<string>();
            foreach (var kvp in escapedParams)
            {
                escapedQueryParams.Add($"{kvp.Key}={kvp.Value}");
            }
            string escapedQueryPart = string.Join("&", escapedQueryParams);

            // Return the reconstructed mailto string
            return $"{baseMailto}?{escapedQueryPart}";
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
