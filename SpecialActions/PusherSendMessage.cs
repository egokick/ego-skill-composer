using skill_composer.Models; 
using PusherServer;
using Newtonsoft.Json;

namespace skill_composer.SpecialActions
{
    public class PusherSendMessage : ISpecialAction
    { 
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var inputLines = task.Input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        
            var channel = inputLines.FirstOrDefault(line => line.StartsWith("channel:"))?["channel:".Length..]?.Trim();
            var eventName = inputLines.FirstOrDefault(line => line.StartsWith("eventName:"))?["eventName:".Length..]?.Trim();
            var message = inputLines.FirstOrDefault(line => line.StartsWith("message:"))?["message:".Length..]?.Trim();

            var result = await SendMessage(channel, eventName, message);

            var resultString = JsonConvert.SerializeObject(result);

            task.Output = resultString;
            
            return task;
        }

        public async Task<ITriggerResult> SendMessage(string channel, string eventName, string message)
        {
            var options = new PusherOptions
            {
                Cluster = "eu",
                Encrypted = true
            };

            var pusher = new Pusher(Settings.PusherAppId, Settings.PusherAppKey, Settings.PusherAppSecret, options);
            var result = await pusher.TriggerAsync(channel, eventName, new { message });

            return result;
        }

    }
}
