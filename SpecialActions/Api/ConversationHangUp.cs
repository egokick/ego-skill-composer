using skill_composer.Models;
using skill_composer.Helper;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net.WebSockets;
using System.Text;

namespace skill_composer.SpecialActions
{
    public class ConversationHangUp : ISpecialAction
    {
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            if(task.AiResponseShared is null || task.UserResponseShared is null)
            {
                throw new NullReferenceException("task.AiResponseShared and or task.UserResponseShared is null, you must construct and set these in the task property when calling the SpecialAction ConversationModeStream");
            }

            while (true)
            {
                task.AiResponseShared.Enqueue("{{HANGUP}}");

                await System.Threading.Tasks.Task.Delay(10); 

                break;
            }

            return task;
        } 
    }
}