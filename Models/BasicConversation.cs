using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skill_composer.Models
{
    /// <summary>
    /// Basic Conversation.
    /// </summary>
    public class BasicConversation
    {
        /// <summary>
        /// Conversation Identity.
        /// </summary>
        public int ConversationId { get; set; }

        /// <summary>
        /// Speech.
        /// </summary>
        public string Speech { get; set; }

        /// <summary>
        /// Role in string from from AiRole.Role
        /// </summary>
        public string Role { get; set; }
    }
}
