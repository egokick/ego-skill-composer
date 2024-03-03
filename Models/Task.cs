using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skill_composer.Models
{
    public class Task
    {
        // The ordinal number of the task, this is used to reference the output of previous tasks with this syntax {{AIResponse[2]}}
        public int Number { get; set; }
        
        // Can be null, Will ask the same question to the ai multiple times and ask the ai to select the "best" result, this is done by asking the ai to summarize the negatives and positives of each response,
        // then considering the negatives and positives, it selects the best of 2 responses (A and B), it continues selecting the best of 2 responses until it has picked "the best", i.e. until there are no other comparisons to make
        public int BestOf { get; set; }

        // The name of the task
        public string Name { get; set; }  

        // **ReadFile**: This action reads the content of a specified file. If no file path is provided in the task.Input, it automatically selects any file from an input folder. If no files are available in the input folder, it halts further processing for this task. The action then reads the content of the file, combines it with the file's title, and assigns this combination as the task's output.
        // **RenameFile**: This action renames a file based on the task.Input, which provides new labels to be included in the file name. It removes spaces and replaces commas with hyphens in the labels, and then strips non-alphanumeric characters from the new file name, except for hyphens, periods, and spaces. The file is then renamed with this new, cleaned-up label followed by its original name, and moved to its original directory.
        // **MoveFileToOutput**: This action moves a file to an output directory. The task.Input should contain the path of the file to be moved. After the move, the task's FilePath is updated to reflect the new location of the file in the output directory.
        // **MoveOutputFilesToInputFolder**: This action transfers all files from an output directory to an input directory. It overwrites any files in the input directory that have the same name as the moved files from the output directory.
        // **GetUniqueLabelsFromInputFiles**: This action scans all files in the input directory, extracts labels from the file names (assuming labels are separated by hyphens in the file names), and identifies labels that occur more than once across all files. It then combines these unique, repeating labels into a single string, separated by commas, and sets this string as the task's output.
        // **ConcatenateFilesByLabel**: This action concatenates the content of files based on provided labels. It first filters out empty or irrelevant labels from the task.Input, then searches for files in the input directory that contain any of the valid labels in their names. The contents of these files are concatenated and saved to a new file in the output directory, named after the label with a ".txt" extension. The path of this new file is then stored in task.FilePath, and a message is printed to indicate the completion of the file creation.
        public string SpecialAction { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string Mode { get; set; } // Internal, AI, User
 
        public string FilePath { get; set; }
        public bool HaltProcessing { get; set; } = false;
    }
}
