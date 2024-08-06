using skill_composer.Models;


using Newtonsoft.Json;
using skill_composer.Services;
using System.Data;


namespace skill_composer.SpecialActions
{
    public class DatabaseSaveConversation : ISpecialAction
    {  
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var cleanedInput = task.Input.Replace("[]","").Replace("][", ",");

            cleanedInput = cleanedInput.Replace("}[", ", \"Conversation\":[");
            cleanedInput += "}";

            var callData = JsonConvert.DeserializeObject<CallData>(cleanedInput);
            var conversation = new Conversation();

            foreach (var dialogue in callData.Conversation)
            {
                // Insert AI dialgoue
                if (!string.IsNullOrEmpty(dialogue.AI))
                {
                    conversation = new Conversation()
                    {
                        ConversationId = 0,
                        ConversationMetaId = callData.CustomParameters.ConversationMetaId,
                        BusinessId = callData.CustomParameters.BusinessId,
                        CallSid = callData.CallSid,
                        Role = "assistant",
                        Speech = dialogue.AI,
                        MediaType = "PhoneCall",
                        CompletionTokens = 0,
                        PromptTokens = 0,
                        TotalTokens = 0,
                        TalkedOn = DateTime.Now
                    };

                    conversation = await InsertConversation(conversation);
                }

                // Insert user dialogue
                if (!string.IsNullOrEmpty(dialogue.User))
                {
                    conversation = new Conversation()
                    {
                        ConversationId = 0,
                        ConversationMetaId = callData.CustomParameters.ConversationMetaId,
                        BusinessId = callData.CustomParameters.BusinessId,
                        CallSid = callData.CallSid,
                        Role = "user",
                        Speech = dialogue.User,
                        MediaType = "PhoneCall",
                        CompletionTokens = 0,
                        PromptTokens = 0,
                        TotalTokens = 0,
                        TalkedOn = DateTime.Now
                    };

                    conversation = await InsertConversation(conversation);
                }
            }
            
            task.Output = JsonConvert.SerializeObject(conversation);

            return task;
        }

       
        public async Task<Conversation> InsertConversation(Conversation conversation)
        {
            var databaseService = new DatabaseService(Program._settings);

            var parameters = DatabaseService.GetParameters(conversation);

            DataTable dt = await databaseService.ExecuteCommand("secretary.Conversation_Insert", parameters);
            if (dt?.Rows == null || dt.Rows.Count <= 0) return conversation;

            conversation.ConversationId = databaseService.GetInteger(dt.Rows[0], "ConversationId");

            return conversation;
        }
        public class CallData
        {
            public string AccountSid { get; set; }
            public string StreamSid { get; set; }
            public string CallSid { get; set; }
            public List<string> Tracks { get; set; }
            public CustomParameters CustomParameters { get; set; }
            public List<ConversationEntry> Conversation { get; set; }
        }

        public class CustomParameters
        {
            public int BusinessPhoneNumberId { get; set; }
            public int BusinessId { get; set; }
            public string PhoneNumber { get; set; }
            public double MonthlyCost { get; set; }
            public DateTime PurchasedOn { get; set; }
            public string Language { get; set; }
            public string Voice { get; set; }
            public string Introduction { get; set; }
            public string TwilioSid { get; set; }
            public string CustomerPhoneNumber { get; set; }
            public string CompanyName { get; set; }
            public bool IsWarmTransfer { get; set; }
            public string ConnectingCallSid { get; set; }
            public string ConferenceName { get; set; }
            public string SkillName { get; set; }
            public int ConversationMetaId { get; set; }
        }

        public class ConversationEntry
        {
            public string AI { get; set; }
            public string User { get; set; }
        }
    }
}
