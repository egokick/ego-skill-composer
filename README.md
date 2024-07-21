# Skill Composer

Skill Composer is a tool to create automation. Craft text prompts that chain together AI responses, software functions (SpecialActions) and user input.
The strengths of this tool are in the composability and chaining of these things.

Edit the skills.json file to create an automation

- task.**Number** - The order of the task, this is used to reference the Output field (the responses of the AI or the User) of other tasks.
- task.**Name** - The name of the task you create.
- task.**Input** - The prompt to ask the AI or the User.
- task.**Mode** (AI, User) -  When set to "AI" the Input will be sent to the AI. When set to "User" the Input will be presented to the User.
- task.**Output** - This is where the AI response or the User response is stored (you can reference this Output in other tasks using the {{Output[1]}} syntax)
- task.**SpecialAction** - Invoke one of the Special Actions - see the list of special actions and their behaviour below.

## Concepts

- You can use the special action "RunSkill" to invoke skills you have already created, this lets you create small skills that do specific things and then chain them together in different ways.
- Data processing - To process files a data/input and data/output folder is created. Use special action "FileGetPath" or "FileRead" to get a file from the input folder. Use special action "FileMoveToOutput" or "FilesMoveToOutputFolder" to move files to the output folder.
- Skill.**RepeatCount** - Set this to a large number if you want to run the automation in a loop. 

# Special Actions 
 
## ConcatenateFilesByLabel
**Purpose**: This class concatenates files based on a specific label.
- **Functionality**: It reads the list of input files and identifies files that match a given label. It then combines the content of these files into a single output file.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/ConcatenateFilesByLabel.cs)

## ConcatenateAllInputFiles
**Purpose**: This class concatenates all provided input files into a single file.
- **Functionality**: It reads all the input files specified and merges their contents into one output file. This is useful for combining multiple data sources into one comprehensive file.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/ConcatenateAllInputFiles.cs)

## CopyInputToOutput
**Purpose**: This class copies the content from the input field to the output field.
- **Functionality**: It reads the data from the input field and directly sets this data as the output, effectively duplicating the input to the output without any modification.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/CopyInputToOutput.cs)

## DelayInSeconds
**Purpose**: This class introduces a delay in the task execution.
- **Functionality**: It pauses the execution of the task for a specified number of seconds. This can be useful for timing purposes or for creating pauses between sequential tasks.
- **Reads**: None
- **Sets**: None

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/DelayInSeconds.cs)

## EmailDownloadAllFromFolder
**Purpose**: This class downloads all emails from a specified email folder.
- **Functionality**: It connects to the specified email folder and retrieves all the emails, saving their content as the output. This is useful for processing or archiving all emails in a folder.
- **Reads**: `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/EmailDownloadAllFromFolder.cs)

## EmailDownloadFromFolder
**Purpose**: This class downloads emails from a specified folder based on certain criteria.
- **Functionality**: It filters and retrieves emails from a folder that match specific criteria (e.g., sender, date range), then saves their content as the output. This helps in selectively processing relevant emails.
- **Reads**: `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/EmailDownloadFromFolder.cs)

## EmailSend
**Purpose**: This class sends an email with the provided input as the message content.
- **Functionality**: It takes the content from the input field, formats it into an email message, and sends it to specified recipients. Useful for automating email notifications or updates.
- **Reads**: `Input`
- **Sets**: None

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/EmailSend.cs)

## FileConvertHeicToJpg
**Purpose**: This class converts HEIC image files to JPG format.
- **Functionality**: It reads HEIC files specified in the input or file path, processes these images, and converts them into JPG format, saving the result in the output.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileConvertHeicToJpg.cs)

## FileConvertHeicToPng
**Purpose**: This class converts HEIC image files to PNG format.
- **Functionality**: Similar to the HEIC to JPG converter, it processes HEIC files and converts them to PNG format, then saves the converted images as output.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileConvertHeicToPng.cs)

## FileGetPath
**Purpose**: This class retrieves the file path for the given input.
- **Functionality**: It reads the input data to determine the relevant file paths, then sets these paths in the file path field, useful for subsequent file operations.
- **Reads**: `Input`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileGetPath.cs)

## FileGetPathByFileType
**Purpose**: This class retrieves file paths based on specified file types.
- **Functionality**: It scans the input directory for files of the specified types and records their paths in the file path field for further processing.
- **Reads**: `Input`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileGetPathByFileType.cs)

## FileMoveToOutput
**Purpose**: This class moves a file to the output location.
- **Functionality**: It reads the file specified in the input or file path, and moves it to the output directory, effectively transferring the file's location.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileMoveToOutput.cs)

## FileRead
**Purpose**: This class reads the content of a file.
- **Functionality**: It opens the file at the specified file path and reads its contents into the input field, enabling subsequent data processing tasks.
- **Reads**: `FilePath`
- **Sets**: `Input`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileRead.cs)

## FileRotateImage
**Purpose**: This class rotates an image by a specified degree.
- **Functionality**: It takes an image file from the input or file path, rotates it by the specified angle, and saves the rotated image as output.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileRotateImage.cs)

## FileWriteInput
**Purpose**: This class writes the input content to a file.
- **Functionality**: It takes the data from the input field and writes it to a file specified by the file path, effectively saving the input data to a file.
- **Reads**: `Input`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileWriteInput.cs)

## FileWriteOutput
**Purpose**: This class writes the output content to a file.
- **Functionality**: It takes the data from the output field and writes it to a file specified by the file path, effectively saving the output data to a file.
- **Reads**: `Output`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileWriteOutput.cs)

## FileWriteSplitOutput
**Purpose**: This class writes split output content to multiple files.
- **Functionality**: It splits the data from the output field and writes each part to separate files specified by the file path, useful for dividing large data sets into manageable files.
- **Reads**: `Output`
- **Sets**: `FilePath`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FileWriteSplitOutput.cs)

## FilesMoveToInputFolder
**Purpose**: This class moves files to the input folder.
- **Functionality**: It takes files specified by the file path and transfers them to the designated input folder, organizing files for input processing.
- **Reads**: `FilePath`
- **Sets**: `Input`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FilesMoveToInputFolder.cs)

## FilesMoveToInputFolderByFileType
**Purpose**: This class moves files of specified types to the input folder.
- **Functionality**: It identifies files of certain types within a directory and moves them to the input folder, organizing files for specific input processing tasks.
- **Reads**: `FilePath`
- **Sets**: `Input`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FilesMoveToInputFolderByFileType.cs)

## FilesMoveToOutputFolder
**Purpose**: This class moves files to the output folder.
- **Functionality**: It takes files specified by the file path and transfers them to the designated output folder, organizing files

 for output processing.
- **Reads**: `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/FilesMoveToOutputFolder.cs)

## GetUniqueLabelsFromInputFiles
**Purpose**: This class retrieves unique labels from input files.
- **Functionality**: It reads through the input files to identify unique labels, which it then compiles and saves to the output field. Useful for categorizing or filtering data based on labels.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/GetUniqueLabelsFromInputFiles.cs)

## QRCodeCreate
**Purpose**: This class creates a QR code from the input data.
- **Functionality**: It takes the input data and generates a QR code image, saving the image to the output field. Useful for encoding data in a QR code for easy sharing or scanning.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/QRCodeCreate.cs)

## RenameFile
**Purpose**: This class renames a file based on the provided input.
- **Functionality**: It reads the file specified in the file path and renames it according to the input, then updates the output field with the new file name.
- **Reads**: `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/RenameFile.cs)

## RunDatabaseQuery
**Purpose**: This class executes a database query using the input data.
- **Functionality**: It takes a query from the input field, runs it against a database, and saves the query results to the output field. This allows for dynamic data retrieval and manipulation based on query results.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/RunDatabaseQuery.cs)

## RunSkill
**Purpose**: This class executes a specific skill defined by the input.
- **Functionality**: It takes the input data, processes it according to a predefined skill, and saves the result to the output field. Useful for modular skill execution in a larger workflow.
- **Reads**: `Input`, `FilePath`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/RunSkill.cs)

## SpeechToTextRealTime
**Purpose**: This class converts speech to text in real-time.
- **Functionality**: It captures spoken words and converts them into text format, updating the output field with the transcribed text as it is spoken. Useful for real-time transcription services.
- **Reads**: None
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/SpeechToTextRealTime.cs)

## SpeechToTextTranslateToEnglish
**Purpose**: This class translates speech to English text.
- **Functionality**: It takes audio input in any language, transcribes it to text, then translates this text to English, saving the result in the output field. Useful for multilingual transcription and translation services.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/SpeechToTextTranslateToEnglish.cs)

## TextToSpeech
**Purpose**: This class converts text to speech.
- **Functionality**: It reads the input text and synthesizes it into spoken words, saving the audio output. Useful for creating audio versions of text content.
- **Reads**: `Input`
- **Sets**: `Output`

[Source](https://github.com/egokick/ego-skill-composer/blob/main/SpecialActions/TextToSpeech.cs)


## Tips

When creating skills in the skills.json file, follow this convention:
-- The task.Output field is intended to be set by the AI or SpecialActions, so avoid directly writing in the Output field in the task (this is merely for consistency and simplicity). However, you can reference the output of previous tasks with this syntax {{Output[1]}} (this is the intended way to use the Output field)..

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
  
