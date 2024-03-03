# Skill Composer

Skill Composer is a console application designed to facilitate AI-driven skill generation and debugging for developers. Utilizing OpenAI's API, it allows users to generate skills based on predefined templates and debug them through a simple and interactive console interface.
 

https://github.com/egokick/ego-skill-composer/assets/580550/062102fc-9628-40d3-a88e-079001576525


## Features

- **AI Skill Generation:** Generate new skills by interacting with an AI model, providing a seamless way to create complex skill sets for your projects.
- **Debugging Support:** Easily debug and iterate over generated skills, allowing for fine-tuning and improvements.
- **Flexible Skill Management:** Manage and modify skills with options to regenerate AI responses, view AI-prompt interpolations, and edit prompts and responses.

## Getting Started

### Prerequisites

- .net 6.0
- An OpenAI API key

### Installation
 
1. Edit the `appsettings.json` file to include your OpenAI API key and other necessary settings:

   ```json
   {
     "OpenAiKey": "YOUR_OPENAI_API_KEY",
     "OpenAiModel": "gpt-4-0125-preview"     
   }
   ```

2. Run the application:

   ```sh
   dotnet run
   ```

## Usage

After running the application, follow the on-screen prompts to either generate new AI skills or debug previous outputs. Select the desired operation by entering the corresponding number:

1. **AI Skill Generation:** Generate a new skill set based on user inputs and AI responses.
2. **Load/Debug Previous Output:** Load and debug previously saved skill outputs for refinement.
 
**ReadFile**: This action reads the content of a specified file. If no file path is provided in the task.Input, it automatically selects any file from an input folder. If no files are available in the input folder, it halts further processing for this task. The action then reads the content of the file, combines it with the file's title, and assigns this combination as the task's output.

**RenameFile**: This action renames a file based on the task.Input, which provides new labels to be included in the file name. It removes spaces and replaces commas with hyphens in the labels, and then strips non-alphanumeric characters from the new file name, except for hyphens, periods, and spaces. The file is then renamed with this new, cleaned-up label followed by its original name, and moved to its original directory.

**MoveFileToOutput**: This action moves a file to an output directory. The task.Input should contain the path of the file to be moved. After the move, the task's FilePath is updated to reflect the new location of the file in the output directory.

**MoveOutputFilesToInputFolder**: This action transfers all files from an output directory to an input directory. It overwrites any files in the input directory that have the same name as the moved files from the output directory.

**GetUniqueLabelsFromInputFiles**: This action scans all files in the input directory, extracts labels from the file names (assuming labels are separated by hyphens in the file names), and identifies labels that occur more than once across all files. It then combines these unique, repeating labels into a single string, separated by commas, and sets this string as the task's output.

**ConcatenateFilesByLabel**: This action concatenates the content of files based on provided labels. It first filters out empty or irrelevant labels from the task.Input, then searches for files in the input directory that contain any of the valid labels in their names. The contents of these files are concatenated and saved to a new file in the output directory, named after the label with a ".txt" extension. The path of this new file is then stored in task.FilePath, and a message is printed to indicate the completion of the file creation.

 
