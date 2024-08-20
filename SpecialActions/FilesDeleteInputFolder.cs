using System.Runtime.InteropServices;
using skill_composer.Helper;
using skill_composer.Models;

namespace skill_composer.SpecialActions
{
    public class FilesDeleteInputFolder : ISpecialAction
    {
        private const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x4;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);

        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var inputDirectory = FilePathHelper.GetDataInputDirectory();
            var inputFiles = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);

            int filesDeleted = 0;
            int filesFailed = 0;

            foreach (var inputFile in inputFiles.Where(x=>!x.Contains(".git")))
            {
                try
                {
                    File.SetAttributes(inputFile, FileAttributes.Normal);
                    File.Delete(inputFile);
                    filesDeleted++;
                }
                catch (IOException ioEx) when ((ioEx.HResult & 0xFFFF) == 32) // ERROR_SHARING_VIOLATION
                {
                    if (MoveFileEx(inputFile, null, MOVEFILE_DELAY_UNTIL_REBOOT))
                    {
                        filesDeleted++;
                        Console.WriteLine($"File {inputFile} scheduled for deletion on next reboot.");
                    }
                    else
                    {
                        filesFailed++;
                        Console.WriteLine($"Failed to schedule {inputFile} for deletion. Error: {ioEx.Message}");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    filesFailed++;
                    Console.WriteLine($"UnauthorizedAccessException: Unable to delete {inputFile}. Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    filesFailed++;
                    Console.WriteLine($"Exception: Unable to delete {inputFile}. Error: {ex.Message}");
                }
            } 

            var inputDirectories = Directory.GetDirectories(inputDirectory, "*", SearchOption.AllDirectories);

            foreach (var directory in inputDirectories.OrderBy(x => x.Length).Where(x => !x.Contains(".git")))
            {
                try
                {
                    Directory.Delete(directory, recursive: true); // Delete directory and its contents
                }
                catch (UnauthorizedAccessException ex)
                {
                    filesFailed++;
                    Console.WriteLine($"UnauthorizedAccessException: Unable to delete directory {directory}. Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    filesFailed++;
                    Console.WriteLine($"Exception: Unable to delete directory {directory}. Error: {ex.Message}");
                }
            }
            
            await System.Threading.Tasks.Task.Delay(1000);

            task.Output = $"Deleted {filesDeleted} files from the output directory: {inputDirectory}. Failed to delete {filesFailed} files/directories.";

            return task;
        }
    }
}
