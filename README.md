# Skill Composer

Skill Composer is a console application designed to facilitate AI-driven skill generation and debugging for developers. Utilizing OpenAI's API, it allows users to generate skills based on predefined templates and debug them through a simple and interactive console interface.

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
     "OpenAiModel": "gpt-4o",
     ...
   }
   ```

2. Run the application:

   ```sh
   dotnet run
   ```

## Usage

After running the application, follow the on-screen prompts to either generate new AI skills or debug previous outputs. Select the desired operation by entering the corresponding number:

1. **AI Skill Generation:** Generate a new skill set based on user inputs and AI responses. 

# Special Actions

## 1. ConcatenateFilesByLabel
- **Reads**: `task.InputFiles`, `task.Labels`
- **Sets**: `task.OutputFile`
- **Description**: Concatenates input files based on specific labels and stores the result in an output file.

## 2. ConcatenateAllInputFiles
- **Reads**: `task.InputFiles`
- **Sets**: `task.OutputFile`
- **Description**: Concatenates all input files into a single output file.

## 3. CopyInputToOutput
- **Reads**: `task.InputFiles`
- **Sets**: `task.OutputFiles`
- **Description**: Copies the input files directly to the output.

## 4. DelayInSeconds
- **Reads**: `task.DelaySeconds`
- **Sets**: None
- **Description**: Introduces a delay for a specified number of seconds before continuing with the next action.

## 5. EmailDownloadAllFromFolder
- **Reads**: `task.EmailCredentials`, `task.FolderName`
- **Sets**: `task.OutputFiles`
- **Description**: Downloads all files from a specified email folder.

## 6. EmailDownloadFromFolder
- **Reads**: `task.EmailCredentials`, `task.FolderName`, `task.FileName`
- **Sets**: `task.OutputFile`
- **Description**: Downloads a specific file from a specified email folder.

## 7. EmailSend
- **Reads**: `task.EmailCredentials`, `task.Recipients`, `task.Subject`, `task.Body`, `task.Attachments`
- **Sets**: None
- **Description**: Sends an email with the specified subject, body, and attachments.

## 8. FileConvertHeicToJpg
- **Reads**: `task.InputFiles`
- **Sets**: `task.OutputFiles`
- **Description**: Converts HEIC files to JPG format.

## 9. FileConvertHeicToPng
- **Reads**: `task.InputFiles`
- **Sets**: `task.OutputFiles`
- **Description**: Converts HEIC files to PNG format.

## 10. FileGetPath
- **Reads**: `task.InputFile`
- **Sets**: `task.OutputPath`
- **Description**: Retrieves the path of a specified input file.

## 11. FileGetPathByFileType
- **Reads**: `task.InputFiles`, `task.FileType`
- **Sets**: `task.OutputPaths`
- **Description**: Retrieves paths of input files that match a specified file type.

## 12. FileMoveToOutput
- **Reads**: `task.InputFiles`
- **Sets**: `task.OutputFiles`
- **Description**: Moves input files to the output location.

## 13. FileRead
- **Reads**: `task.InputFile`
- **Sets**: `task.FileContent`
- **Description**: Reads the content of a specified input file.

## 14. FileRotateImage
- **Reads**: `task.InputFile`, `task.RotationAngle`
- **Sets**: `task.OutputFile`
- **Description**: Rotates an image by a specified angle.

## 15. FileWriteInput
- **Reads**: `task.FileContent`
- **Sets**: `task.OutputFile`
- **Description**: Writes specified content to an input file.

## 16. FileWriteOutput
- **Reads**: `task.FileContent`
- **Sets**: `task.OutputFile`
- **Description**: Writes specified content to an output file.

## 17. FileWriteSplitOutput
- **Reads**: `task.FileContent`
- **Sets**: `task.OutputFiles`
- **Description**: Writes specified content to multiple output files.

## 18. FilesMoveToInputFolder
- **Reads**: `task.InputFiles`, `task.InputFolder`
- **Sets**: `task.OutputFiles`
- **Description**: Moves input files to a specified input folder.

## 19. FilesMoveToInputFolderByFileType
- **Reads**: `task.InputFiles`, `task.FileType`, `task.InputFolder`
- **Sets**: `task.OutputFiles`
- **Description**: Moves files of a specified type to a specified input folder.

## 20. FilesMoveToOutputFolder
- **Reads**: `task.InputFiles`, `task.OutputFolder`
- **Sets**: `task.OutputFiles`
- **Description**: Moves input files to a specified output folder.

## 21. GetUniqueLabelsFromInputFiles
- **Reads**: `task.InputFiles`
- **Sets**: `task.OutputLabels`
- **Description**: Extracts unique labels from input files.

## 22. QRCodeCreate
- **Reads**: `task.Data`
- **Sets**: `task.OutputFile`
- **Description**: Creates a QR code from specified data and saves it as an output file.

## 23. RenameFile
- **Reads**: `task.InputFile`, `task.NewFileName`
- **Sets**: `task.OutputFile`
- **Description**: Renames a specified input file.

## 24. RunDatabaseQuery
- **Reads**: `task.DatabaseCredentials`, `task.Query`
- **Sets**: `task.QueryResult`
- **Description**: Executes a database query and stores the result.

## 25. RunSkill
- **Reads**: `task.SkillName`, `task.SkillParameters`
- **Sets**: `task.SkillResult`
- **Description**: Runs a specified skill with given parameters and stores the result.

## 26. SpeechToTextRealTime
- **Reads**: `task.AudioStream`
- **Sets**: `task.Transcript`
- **Description**: Converts real-time speech from an audio stream to text.

## 27. SpeechToTextTranslateToEnglish
- **Reads**: `task.AudioFile`, `task.SourceLanguage`
- **Sets**: `task.Transcript`
- **Description**: Converts speech from an audio file to English text.

## 28. TextToSpeech
- **Reads**: `task.Text`
- **Sets**: `task.AudioFile`
- **Description**: Converts specified text to speech and saves it as an audio file.

## Available Models
Available values : gpt-35-turbo-0613, gpt-35-turbo-16k-0613, gpt-4-0613, gpt-4-32k-0613
 

## How to use

When creating skills in the skills.json file, follow this convention:
-- The task.Output field is intended to be set by the AI or SpecialActions, so avoid directly writing in the Output field in the task (this is merely for consistency and simplicity). However, you can reference the output of previous tasks with this syntax {{Output[1]}} (this is the intended way to use the Output field)..

