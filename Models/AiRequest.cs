using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skill_composer.Models
{
    /// <summary>
    /// Ai Request.
    /// </summary>
    public class AiRequest
    {
        /// <summary>
        /// Business Identity.
        /// </summary>
        public int BusinessId { get; set; }

        /// <summary>
        /// Question to ask the AI.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Ongoing Conversation.
        /// </summary>
        public IList<BasicConversation> OnGoingConversation { get; set; }
    }
}
