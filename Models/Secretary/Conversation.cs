 
namespace skill_composer.Models
{ 
    public class Conversation
    {
        public int ConversationId { get; set; }
        public int ConversationMetaId { get; set; }
        public int BusinessId { get; set; }
        public string CallSid { get; set; }
        public string Speech { get; set; }
        public string Role { get; set; }
        public DateTime TalkedOn { get; set; }
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
        public string MediaType { get; set; }
    }

}
