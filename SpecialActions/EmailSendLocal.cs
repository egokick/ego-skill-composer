using Microsoft.Graph;
using skill_composer.Models;
using Task = System.Threading.Tasks.Task;
using skill_composer.Helper;
using Microsoft.Graph.Models;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// This will pop the browser for user outlook authentication. The emaill will be sent from the user that logs in. 
    /// This is useful for automation because it only requires the user to auth once
    /// all subsequente calls to this will automatically re use the authentication.
    /// </summary>
    public class EmailSendLocal : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if (string.IsNullOrEmpty(task.Input))
            {
                Console.WriteLine("Task input is empty. Please set the Input field to be the To email address. You can optionally put the email content in the FilePath or the next line of the Input field.");
                return task;
            }

            var interactiveBrowserCredential = AuthenticationHelper.GetInteractiveBrowserCredential(Settings.AzureClientId);

            var graphClient = new GraphServiceClient(interactiveBrowserCredential);

            var emailLines = task.Input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string toEmailsString = emailLines.FirstOrDefault(line => line.StartsWith("to:"))?.Substring(3).Trim();
            string ccEmailsString = emailLines.FirstOrDefault(line => line.StartsWith("cc:"))?.Substring(3).Trim();
            string emailSubject = emailLines.FirstOrDefault(line => line.StartsWith("subject:"))?.Substring(8).Trim();
            string emailBody = emailLines.FirstOrDefault(line => line.StartsWith("body:"))?.Substring(5).Trim();

            if (toEmailsString == null || emailSubject == null || emailBody == null)
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
                    ContentType = BodyType.Html,
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
                }).ToList(),
                CcRecipients = ccEmails.Select(email => new Recipient
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
