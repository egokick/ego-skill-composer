using Microsoft.Graph;
using System.Text.RegularExpressions;
using skill_composer.Models;
using skill_composer.Helper;
using Task = System.Threading.Tasks.Task;
using Microsoft.Graph.Me.Messages.Item.Move;
using Microsoft.Graph.Models;

namespace skill_composer.SpecialActions
{
    /// <summary>
    /// Downloads all emails from a specified origin folder
    /// and moves them to a specified destination folder within a user's mailbox.
    /// The user provides the origin and destination folder names in the format 'Origin->Destination',
    /// and each email's content is saved to a local directory before being moved.
    /// The result of the operation, including the number of emails processed, is set in the output field of the provided task object.
    /// TODO: Currently this only works for email folders at the top folder level. Add support for an email folder at any level of nesting.
    /// TODO: Currently this only works for outlook, add support for gmail, yahoo and protonmail
    /// </summary>
    public class EmailDownloadAllFromFolder : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
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

            var interactiveBrowserCredential = AuthenticationHelper.GetInteractiveBrowserCredential(Settings.AzureClientId);

            var graphClient = new GraphServiceClient(interactiveBrowserCredential);

            var inputDirectory = FilePathHelper.GetDataInputDirectory();
            await DownloadEmailsFromFolderAsync(graphClient, originFolderName, destinationFolderName, inputDirectory);

            return task;
        }

        private async Task DownloadEmailsFromFolderAsync(GraphServiceClient graphClient, string originFolderName, string destinationFolderName, string inputDirectory)
        {
            var originFolder = await GetFolderByNameAsync(graphClient, originFolderName);
            if (originFolder == null)
            {
                Console.WriteLine($"The origin folder '{originFolderName}' was not found.");
                return;
            }

            var destinationFolder = await GetFolderByNameAsync(graphClient, destinationFolderName);
            if (destinationFolder == null)
            {
                Console.WriteLine($"The destination folder '{destinationFolderName}' was not found.");
                return;
            }

            var messagePage = await graphClient.Me.MailFolders[originFolder.Id].Messages.GetAsync();

            int emailCount = 0;

            while (messagePage.Value.Count > 0)
            {
                foreach (var message in messagePage.Value)
                {
                    emailCount++;
                    var safeMailSubjectName = SanitizeFileName(message.Subject);
                    string fileName = $"{safeMailSubjectName}_{emailCount}.txt";
                    string filePath = Path.Combine(inputDirectory, fileName);
                    await File.WriteAllTextAsync(filePath, message.Body.Content);

                    // Move the message to the destination folder
                    var moveRequest = new MovePostRequestBody
                    {
                        DestinationId = destinationFolder.Id
                    };
                    await graphClient.Me.Messages[message.Id].Move.PostAsync(moveRequest);
                }

                if (messagePage.OdataNextLink != null)
                {
                    messagePage = await graphClient.Me.MailFolders[originFolder.Id].Messages.WithUrl(messagePage.OdataNextLink).GetAsync();
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine($"Downloaded and moved {emailCount} emails.");
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
