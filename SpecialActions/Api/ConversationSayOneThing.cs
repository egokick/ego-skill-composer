using skill_composer.Models;
using skill_composer.Helper;
using Newtonsoft.Json;

namespace skill_composer.SpecialActions
{
    public class ConversationSayOneThing : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if(task.AiResponseShared is null) throw new NullReferenceException("task.AiResponseShared is null, you must construct and set in the task property when calling the SpecialAction ConversationModeStream");
            
            if (Program.api is null) Program.api = new ApiHandler();

            task.AiResponseShared.Enqueue(task.Input);
                
            // Wait for the AI to stop talking - this is handled in the WebsocketService PlayAudio method.
            while (task.AiResponseShared.TryPeek(out var _))
            {
                await System.Threading.Tasks.Task.Delay(10);

                // prevents known transcription bug with twilio websocket where the AI introduction audio playing over speaker is incorrectly received as coming
                // from the end user channel, this is only a problem when the phone is in Speaker mode.
                task.UserResponseShared.TryDequeue(out var _);  
            }

            var conversationList = new Dictionary<string, string> { { "AI", task.Input } };

            task.Output = JsonConvert.SerializeObject(conversationList, Formatting.Indented);

            return task;
        }
    }
}