using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Authentication;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using skill_composer.Models;
using Task = System.Threading.Tasks.Task;

namespace skill_composer.SpecialActions
{
    public class EmailSend : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (string.IsNullOrEmpty(task.Input))
            {
                Console.WriteLine("Task input is empty. Please set the Input field to be the To email address. You can optionally put the email content in the FilePath or the next line of the Input field.");
                return task;
            }

            var credential = new ClientSecretCredential(Settings.AzureTenantId, Settings.AzureClientId, Settings.AzureSecretId);
            var graphClient = new GraphServiceClient(new AzureIdentityAuthenticationProvider(credential));

            var emailLines = task.Input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var toEmailsString = emailLines.FirstOrDefault(line => line.StartsWith("to:"))?[3..]?.Trim()!;
            var ccEmailsString = emailLines.FirstOrDefault(line => line.StartsWith("cc:"))?[3..]?.Trim()!;
            var emailSubject = emailLines.FirstOrDefault(line => line.StartsWith("subject:"))?[8..]?.Trim()!;
            var emailBody = emailLines.FirstOrDefault(line => line.StartsWith("body:"))?[5..]?.Trim()!;

            if (string.IsNullOrEmpty(toEmailsString) || string.IsNullOrEmpty(emailSubject) || string.IsNullOrEmpty(emailBody))
            {
                Console.WriteLine("Missing required email fields in the input.");
                return task;
            }

            var toEmails = toEmailsString.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();
            var ccEmails = string.IsNullOrEmpty(ccEmailsString) ? new List<string>() : ccEmailsString.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();

            if (toEmails.Count == 0)
            {
                Console.WriteLine("No email addresses found in the input.");
                return task;
            }

            try
            {
                await SendEmailAsync(graphClient, toEmails, ccEmails, emailSubject, emailBody);
                task.Output = $"Email sent successfully to {string.Join(",", toEmails)}.";
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"EmailSend error: {ex.Message}");
                task.Output = $"EmailSend Error sending email: {ex.Message}";
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Error code: {ex.Message}");
                throw;
            }

            return task;
        }

        private async Task SendEmailAsync(GraphServiceClient graphClient, List<string> toEmails, List<string> ccEmails, string subject, string body)
        {
            const string from = "mipmapper@digital-dreams.uk";

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = body
                },
                From = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = from
                    }
                },
                ToRecipients = toEmails.Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = email
                    }
                }).ToList(),
                CcRecipients = ccEmails.Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = email
                    }
                }).ToList()
            };

            var sendMailRequestBody = new SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = true
            };

            await graphClient.Users[from].SendMail.PostAsync(sendMailRequestBody);

            Console.WriteLine($"Email sent successfully to {string.Join(",", toEmails)}.");
        }
    }
}
