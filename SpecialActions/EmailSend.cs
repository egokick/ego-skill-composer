using Microsoft.Graph;
using Azure.Identity;
using skill_composer.Models;
using Task = System.Threading.Tasks.Task;
using skill_composer.Helper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Identity.Client;
using Microsoft.Graph.Models;
using System.Linq;

namespace skill_composer.SpecialActions
{
    public class EmailSend : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            if (string.IsNullOrEmpty(task.Input))
            {
                Console.WriteLine("Task input is empty. Please set the Input field to be the To email address. You can optionally put the email content in the FilePath or the next line of the Input field.");
                return task;
            }

            var interactiveBrowserCredential = AuthenticationHelper.GetInteractiveBrowserCredential(Program._settings.AzureClientId);
            var publicClientApplication = AuthenticationHelper.GetPublicClientApplication(Program._settings.AzureClientId);

            var accounts = await publicClientApplication.GetAccountsAsync();
            AuthenticationResult result;
            try
            {
                result = await publicClientApplication.AcquireTokenSilent(new[] { "User.Read", "Mail.Send" }, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await publicClientApplication.AcquireTokenInteractive(new[] { "User.Read", "Mail.Send" })
                    .ExecuteAsync();
            }

            var graphClient = new GraphServiceClient(interactiveBrowserCredential);

            string emailContent;
            if (!string.IsNullOrEmpty(task.FilePath) && File.Exists(task.FilePath))
            {
                emailContent = await File.ReadAllTextAsync(task.FilePath);
            }
            else
            {
                emailContent = task.Input;
            }

            var emailLines = task.Input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var toEmails = emailLines[0].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();

            string emailSubject = emailLines.Length > 1 ? emailLines[1].Trim() : "SubjectLineIsSetByTheSecondLineOfTheInputField";

            string emailBody;
            if (!string.IsNullOrEmpty(task.FilePath) && File.Exists(task.FilePath))
            {
                emailBody = await File.ReadAllTextAsync(task.FilePath);
            }
            else
            {
                emailBody = string.Join(Environment.NewLine, emailLines.Skip(2)); // Start from the third line for the body
            }

            if (toEmails.Count == 0)
            {
                Console.WriteLine("No email addresses found in the input.");
                return task;
            }

            try
            {
                await SendEmailAsync(graphClient, toEmails, emailSubject, emailBody);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Error code: {ex.Message}");
                throw;
            }

            return task;
        }

        private async Task SendEmailAsync(GraphServiceClient graphClient, List<string> toEmails, string subject, string body)
        {
            var me = await graphClient.Me.GetAsync();

            var fromEmail = me.UserPrincipalName;

            if (!string.IsNullOrEmpty(me.Mail))
            {
                fromEmail = me.Mail;
            }

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = body
                },
                From = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = fromEmail
                    }
                },
                ToRecipients = toEmails.Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = email
                    }
                }).ToList()
            };

            var sendMailRequestBody = new Microsoft.Graph.Me.SendMail.SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = true
            };

            var pathParameters = new Dictionary<string, object>
            {
                { "user-id", "me" } // "me" is automatically switched out for the authenticated user's email address.
            };
            var sendMailRequestBuilder = new Microsoft.Graph.Me.SendMail.SendMailRequestBuilder(pathParameters, graphClient.RequestAdapter);

            await sendMailRequestBuilder.PostAsync(sendMailRequestBody);

            Console.WriteLine($"Email sent successfully to {string.Join(",", toEmails)}.");
        }
    }
}
