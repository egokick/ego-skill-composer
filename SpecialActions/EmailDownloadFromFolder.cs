using Microsoft.Graph;
using System.Text.RegularExpressions;
using skill_composer.Models;
using skill_composer.Helper;
using Azure.Identity;
using Task = System.Threading.Tasks.Task;
using Microsoft.Graph.Me.Messages.Item.Move;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Downloads a single email from a specified origin folder
    /// and moves it to a specified destination folder within a user's mailbox.
    /// The user provides the origin and destination folder names in the format 'Origin->Destination',
    /// and the email content is saved to a local directory before being moved.
    /// The result of the operation is set in the output field of the provided task object.
    /// TODO: Only ask for email sign in authentication once at the start.
    /// TODO: Currently this only works for email folders at the top folder level. Add support for an email folder at any level of nesting.
    /// TODO: Currently this only works for outlook, add support for gmail, yahoo and protonmail
    /// </summary> 
    public class EmailDownloadFromFolder : ISpecialAction
    {
        public async Task<Models.Task> ExecuteAsync(Models.Task task, Skill selectedSkill, Settings settings)
        {
            if (string.IsNullOrEmpty(task.Input))
            {
                Console.WriteLine("Task input is empty. Please provide the origin and destination folder names formatted as 'Origin->Destination'.");
                return task;
            }

            var folders = task.Input.Split("->");
            if (folders.Length != 2)
            {
                Console.WriteLine("Invalid folder format. Please provide the origin and destination folder names formatted as 'Origin->Destination'.");
                return task;
            }

            string originFolderName = folders[0];
            string destinationFolderName = folders[1];

            var interactiveBrowserCredential = AuthenticationHelper.GetInteractiveBrowserCredential(Program._settings.AzureClientId);
            var publicClientApplication = AuthenticationHelper.GetPublicClientApplication(Program._settings.AzureClientId);

            var accounts = await publicClientApplication.GetAccountsAsync();
            AuthenticationResult result;
            try
            {
                result = await publicClientApplication.AcquireTokenSilent(new[] { "User.Read", "Mail.Read", "Mail.ReadWrite" }, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await publicClientApplication.AcquireTokenInteractive(new[] { "User.Read", "Mail.Read", "Mail.ReadWrite" })
                    .ExecuteAsync();
            }

            var graphClient = new GraphServiceClient(interactiveBrowserCredential);

            var inputDirectory = FilePathHelper.GetDataInputDirectory();

            task.FilePath = await DownloadEmailFromFolderAsync(graphClient, originFolderName, destinationFolderName, inputDirectory);

            return task;
        }

        private async Task<string> DownloadEmailFromFolderAsync(GraphServiceClient graphClient, string originFolderName, string destinationFolderName, string inputDirectory)
        {
            var filePath = "";

            var originFolder = await GetFolderByNameAsync(graphClient, originFolderName);
            if (originFolder == null)
            {
                Console.WriteLine($"The origin folder '{originFolderName}' was not found.");
                return filePath;
            }

            var destinationFolder = await GetFolderByNameAsync(graphClient, destinationFolderName);
            if (destinationFolder == null)
            {
                Console.WriteLine($"The destination folder '{destinationFolderName}' was not found.");
                return filePath;
            }

            var messagePage = await graphClient.Me.MailFolders[originFolder.Id].Messages.GetAsync();

            int emailCount = 0;

            if (messagePage.Value.Count > 0)
            {
                var message = messagePage.Value.First();
                var safeMailSubjectName = SanitizeFileName(message.Subject);
                string fileName = $"{safeMailSubjectName}.txt";
                filePath = Path.Combine(inputDirectory, fileName);
                await File.WriteAllTextAsync(filePath, message.Body.Content);

                // Move the message to the destination folder
                var moveRequest = new MovePostRequestBody
                {
                    DestinationId = destinationFolder.Id
                };
                await graphClient.Me.Messages[message.Id].Move.PostAsync(moveRequest);

                Console.WriteLine("Downloaded and moved 1 email.");
                emailCount++;
            }
            else
            {
                Console.WriteLine("No emails found in the origin folder.");
            }

            Console.WriteLine($"Downloaded and moved {emailCount} emails.");

            return filePath;
        }

        private async Task<MailFolder> GetFolderByNameAsync(GraphServiceClient graphClient, string folderName)
        {
            var mailFoldersPage = await graphClient.Me.MailFolders.GetAsync();

            while (mailFoldersPage.Value.Count > 0)
            {
                var folder = mailFoldersPage.Value.FirstOrDefault(f => f.DisplayName.Equals(folderName, StringComparison.OrdinalIgnoreCase));
                if (folder != null)
                {
                    return folder;
                }

                if (mailFoldersPage.OdataNextLink != null)
                {
                    mailFoldersPage = await graphClient.Me.MailFolders.WithUrl(mailFoldersPage.OdataNextLink).GetAsync();
                }
                else
                {
                    break;
                }
            }

            return null;
        }

        private string SanitizeFileName(string fileName)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            string invalidRegex = string.Format("[{0}]", Regex.Escape(invalidChars));
            return Regex.Replace(fileName, invalidRegex, "_");
        }
    }
}
