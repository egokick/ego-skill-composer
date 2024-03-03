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
 

 
