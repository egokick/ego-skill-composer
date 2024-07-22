# Skill Composer

Skill Composer is a tool to create automation. 

Craft text and image prompts, chain AI responses, software functions (SpecialActions) and user inputs.
The composability of these things are the main strengths of this tool.

Demo Convert Spanish book to English audio
[Demo Convert Spanish book to English audio](https://private-user-images.githubusercontent.com/580550/350785085-df4b316e-0347-4088-84e9-992a1e310cb0.mp4?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3MjE1ODIzNTQsIm5iZiI6MTcyMTU4MjA1NCwicGF0aCI6Ii81ODA1NTAvMzUwNzg1MDg1LWRmNGIzMTZlLTAzNDctNDA4OC04NGU5LTk5MmExZTMxMGNiMC5tcDQ_WC1BbXotQWxnb3JpdGhtPUFXUzQtSE1BQy1TSEEyNTYmWC1BbXotQ3JlZGVudGlhbD1BS0lBVkNPRFlMU0E1M1BRSzRaQSUyRjIwMjQwNzIxJTJGdXMtZWFzdC0xJTJGczMlMkZhd3M0X3JlcXVlc3QmWC1BbXotRGF0ZT0yMDI0MDcyMVQxNzE0MTRaJlgtQW16LUV4cGlyZXM9MzAwJlgtQW16LVNpZ25hdHVyZT1lMGU2Y2Y5NWJhNzYxOWY2YmFlZDFiNzczYzA1MTJlOThhMTJlMGQ1MTI4MDYyYWQ0ZGUzZTkwOGViZGRkZDMyJlgtQW16LVNpZ25lZEhlYWRlcnM9aG9zdCZhY3Rvcl9pZD0wJmtleV9pZD0wJnJlcG9faWQ9MCJ9.q8fuV0Tovupoqcwf45S3_bY0OaX6dc7_kUUczLmPA6Y)
 
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
- The skills.json file design doesn't support "if" statements because they make things too complicated. Instead, try these simpler strategies. Ask the AI for output based on a condition: In your prompt to the AI, you can say: "Output 'true' if {your condition}, else output 'false'." Use different file names for conditions:Create files for each condition (true or false). In the next step, use the SpecialAction 'RunSkill' to run a skill that processes the appropriate file for your condition. This design allows you to simply read the skills.json file in order and known that everything is going to be executed in exactly the same order.

# Special Actions 
## ConcatenateFilesByLabel

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the input files specified in `task.Input`.
2. Group the files by their labels.
3. Concatenate the files within each group.
4. Write the concatenated content to the output specified in `task.Output`.

## ConcatenateAllInputFiles

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read all input files specified in `task.Input`.
2. Concatenate all the input files into a single file.
3. Write the concatenated content to the output specified in `task.Output`.

## CopyInputToOutput

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the input file specified in `task.Input`.
2. Copy the content of the input file to the output specified in `task.Output`.

## DelayInSeconds

**Fields Read:**
- `task.Input`

**Fields Set:**
- None

**Steps:**
1. Read the delay duration from `task.Input`.
2. Pause execution for the specified duration.

## EmailDownloadAllFromFolder

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Connect to the email server using the credentials in `task.Input`.
2. Download all emails from the specified folder.
3. Save the emails to the location specified in `task.Output`.

## EmailDownloadFromFolder

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Connect to the email server using the credentials in `task.Input`.
2. Download emails from the specified folder based on certain criteria.
3. Save the emails to the location specified in `task.Output`.

## EmailSend

**Fields Read:**
- `task.Input`

**Fields Set:**
- None

**Steps:**
1. Read the email details (recipient, subject, body, attachments) from `task.Input`.
2. Connect to the email server using the credentials in `task.Input`.
3. Send the email.

## FileConvertHeicToJpg

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the HEIC file specified in `task.Input`.
2. Convert the HEIC file to JPG format.
3. Save the converted file to the location specified in `task.Output`.

## FileConvertHeicToPng

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the HEIC file specified in `task.Input`.
2. Convert the HEIC file to PNG format.
3. Save the converted file to the location specified in `task.Output`.

## FileGetPath

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the input file or directory path from `task.Input`.
2. Retrieve the absolute path.
3. Save the path to `task.Output`.

## FileGetPathByFileType

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the directory and file type from `task.Input`.
2. Retrieve paths of all files of the specified type.
3. Save the paths to `task.Output`.

## FileMoveToOutput

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the input file path from `task.Input`.
2. Move the file to the location specified in `task.Output`.

## FileRead

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the file specified in `task.Input`.
2. Read the content of the file.
3. Save the content to `task.Output`.

## FileRotateImage

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the image file from `task.Input`.
2. Rotate the image by a specified angle.
3. Save the rotated image to `task.Output`.

## FileWriteInput

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the content from `task.Input`.
2. Write the content to a file specified in `task.Output`.

## FileWriteOutput

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the content from `task.Output`.
2. Write the content to a file specified in `task.Output`.

## FileWriteSplitOutput

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the content from `task.Output`.
2. Split the content based on a delimiter.
3. Write each part to separate files.

## FilesMoveToInputFolder

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the files specified in `task.Input`.
2. Move each file to the input folder specified.

## FilesMoveToInputFolderByFileType

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the file type and directory from `task.Input`.
2. Move all files of the specified type to the input folder.

## FilesMoveToOutputFolder

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the files specified in `task.Input`.
2. Move each file to the output folder specified.

## GetUniqueLabelsFromInputFiles

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the input files specified in `task.Input`.
2. Extract unique labels from the files.
3. Save the unique labels to `task.Output`.

## QRCodeCreate

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the data from `task.Input`.
2. Generate a QR code from the data.
3. Save the QR code image to the location specified in `task.Output`.

## RenameFile

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- None

**Steps:**
1. Read the current file path and new name from `task.Input`.
2. Rename the file to the new name.
3. Save the renamed file to `task.Output`.

## RunDatabaseQuery

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the database connection details and query from `task.Input`.
2. Connect to the database.
3. Execute the query.
4. Save the query results to `task.Output`.

## RunSkill

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the skill details from `task.Input`.
2. Execute the skill.
3. Save the results to `task.Output`.

## SpeechToTextRealTime

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the audio input details from `task.Input`.
2. Perform real-time speech-to-text conversion.
3. Save the transcribed text to `task.Output`.

## SpeechToTextTranslateToEnglish

**Fields Read:**
- `task.Input`
- `task.Output`

**Fields Set:**
- `task.Output`

**Steps:**
1. Read the audio input details from `task.Input`.
2. Perform speech-to-text conversion.
3. Translate the transcribed text to English.
4. Save the translated text to `task.Output`.

## TextToSpeech 

**Steps:**
1. Read the text data from `task.Input`.
2. Convert the text to speech.
3. Save the speech audio to the location specified in `task.Output`.
 
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
  
