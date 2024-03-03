using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skill_composer.Models
{
    /// <summary>
    /// Ai Roles.
    /// </summary>
    public static class AiRole
    {
        /// <summary>
        /// User, used to represent the customer.
        /// </summary>
        public const string User = "user";

        /// <summary>
        /// Assistant, used to represent the AI.
        /// </summary>
        public const string Assistant = "assistant";

        /// <summary>
        /// System, used to hold AI knowledge.
        /// </summary>
        public const string System = "system";

        /// <summary>
        /// Function, used to hold function definitions.
        /// </summary>
        public const string Function = "function";
    }
}
