# Skill Composer

Skill Composer is a console application designed to facilitate AI-driven skill generation and debugging for developers. Utilizing OpenAI's API, it allows users to generate skills based on predefined templates and debug them through a simple and interactive console interface.

## Features

- **AI Skill Generation:** Generate new skills by interacting with an AI model, providing a seamless way to create complex skill sets for your projects.
- **Debugging Support:** Easily debug and iterate over generated skills, allowing for fine-tuning and improvements.
- **Flexible Skill Management:** Manage and modify skills with options to regenerate AI responses, view AI-prompt interpolations, and edit prompts and responses.

## Getting Started

### Prerequisites

- .net 6.0 or greater
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



# Special Actions
# Summary of ISpecialAction Implementations

This document provides a summary of each class that implements the `ISpecialAction` interface, detailing the purpose of each class and the fields from the `Task` class they read from and set.

## ConcatenateFilesByLabel
**Purpose**: Concatenates files based on a label.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/ConcatenateFilesByLabel.cs)

## ConcatenateAllInputFiles
**Purpose**: Concatenates all input files into one.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/ConcatenateAllInputFiles.cs)

## CopyInputToOutput
**Purpose**: Copies the input content to the output.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/CopyInputToOutput.cs)

## DelayInSeconds
**Purpose**: Delays the task execution by a specified number of seconds.
- **Reads**: None
- **Sets**: None

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/DelayInSeconds.cs)

## EmailDownloadAllFromFolder
**Purpose**: Downloads all emails from a specified folder.
- **Reads**: `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/EmailDownloadAllFromFolder.cs)

## EmailDownloadFromFolder
**Purpose**: Downloads emails from a specified folder based on certain criteria.
- **Reads**: `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/EmailDownloadFromFolder.cs)

## EmailSend
**Purpose**: Sends an email with the given input as the message content.
- **Reads**: `Input`
- **Sets**: None

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/EmailSend.cs)

## FileConvertHeicToJpg
**Purpose**: Converts HEIC files to JPG format.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileConvertHeicToJpg.cs)

## FileConvertHeicToPng
**Purpose**: Converts HEIC files to PNG format.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileConvertHeicToPng.cs)

## FileGetPath
**Purpose**: Retrieves the file path for the given input.
- **Reads**: `Input`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileGetPath.cs)

## FileGetPathByFileType
**Purpose**: Retrieves file paths by specified file types.
- **Reads**: `Input`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileGetPathByFileType.cs)

## FileMoveToOutput
**Purpose**: Moves a file to the output location.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileMoveToOutput.cs)

## FileRead
**Purpose**: Reads the content of a file.
- **Reads**: `FilePath`
- **Sets**: `Input`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileRead.cs)

## FileRotateImage
**Purpose**: Rotates an image by a specified degree.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileRotateImage.cs)

## FileWriteInput
**Purpose**: Writes the input content to a file.
- **Reads**: `Input`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileWriteInput.cs)

## FileWriteOutput
**Purpose**: Writes the output content to a file.
- **Reads**: `Output`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileWriteOutput.cs)

## FileWriteSplitOutput
**Purpose**: Writes split output content to multiple files.
- **Reads**: `Output`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileWriteSplitOutput.cs)

## FilesMoveToInputFolder
**Purpose**: Moves files to the input folder.
- **Reads**: `FilePath`
- **Sets**: `Input`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FilesMoveToInputFolder.cs)

## FilesMoveToInputFolderByFileType
**Purpose**: Moves files of specified types to the input folder.
- **Reads**: `FilePath`
- **Sets**: `Input`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FilesMoveToInputFolderByFileType.cs)

## FilesMoveToOutputFolder
**Purpose**: Moves files to the output folder.
- **Reads**: `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FilesMoveToOutputFolder.cs)

## GetUniqueLabelsFromInputFiles
**Purpose**: Retrieves unique labels from input files.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/GetUniqueLabelsFromInputFiles.cs)

## QRCodeCreate
**Purpose**: Creates a QR code from the input data.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/QRCodeCreate.cs)

## RenameFile
**Purpose**: Renames a file based on the provided input.
- **Reads**: `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/RenameFile.cs)

## RunDatabaseQuery
**Purpose**: Executes a database query using the input data.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/RunDatabaseQuery.cs)

## RunSkill
**Purpose**: Executes a specific skill defined by the input.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/RunSkill.cs)

## SpeechToTextRealTime
**Purpose**: Converts speech to text in real-time.
- **Reads**: None
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/SpeechToTextRealTime.cs)

## SpeechToTextTranslateToEnglish
**Purpose**: Translates speech to English text.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/SpeechToTextTranslateToEnglish.cs)

## TextToSpeech
**Purpose**: Converts text to speech.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/TextToSpeech.cs)

## Available Models
Available values : gpt-35-turbo-0613, gpt-35-turbo-16k-0613, gpt-4-0613, gpt-4-32k-0613
 

## How to use

When creating skills in the skills.json file, follow this convention:
-- The task.Output field is intended to be set by the AI or SpecialActions, so avoid directly writing in the Output field in the task (this is merely for consistency and simplicity). However, you can reference the output of previous tasks with this syntax {{Output[1]}} (this is the intended way to use the Output field)..

