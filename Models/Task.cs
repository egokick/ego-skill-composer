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

        // LabelInputFiles, MoveOutputFilesToInputFolder, , ConcatenateFilesByLabel
        // GetInputFile = Gets a random input file from the input folder, if you have specified a filepath in the "Input" field it will get that file
        // MoveOutputFilesToInputFolder = Moves all files in the output folder to the input folder. TODO: if SpecialActionInput contains filepaths, it will move those files to the input folder

        // GetUniqueLabelsFromInputFiles = Gets a list of labels from the file names in the input folder and sets the task.Output to a distinct comma separated list of labels
        // MoveFileToOutput = moves the file in the input field to the data/output folder
        public string SpecialAction { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string Mode { get; set; } // Internal, AI, User
 
        public string FilePath { get; set; }
        public bool HaltProcessing { get; set; } = false;
    }
}
