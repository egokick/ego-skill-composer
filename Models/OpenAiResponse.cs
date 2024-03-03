using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skill_composer.Models
{
    /// <summary>
    /// Represents the response received from the OpenAI API after making a call.
    /// </summary>
    public class OpenAiResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier associated with the response.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of object returned in the response.
        /// </summary>
        [JsonProperty("object")]
        public string Object { get; set; }

        /// <summary>
        /// Gets or sets the timestamp indicating when the response was created.
        /// </summary>
        [JsonProperty("created")]
        public long Created { get; set; }

        /// <summary>
        /// Gets or sets the name of the model used for generating the response.
        /// </summary>
        [JsonProperty("model")]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets a list of choices generated in the response.
        /// </summary>
        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; }

        /// <summary>
        /// Gets or sets information about token usage in the response.
        /// </summary>
        [JsonProperty("usage")]
        public UsageInfo Usage { get; set; }

        /// <summary>
        /// Represents a message role and its content in the response.
        /// </summary>
        public class Message
        {
            /// <summary>
            /// Gets or sets the role associated with the message.
            /// </summary>
            [JsonProperty("role")]
            public string Role { get; set; }

            /// <summary>
            /// Gets or sets the content of the message.
            /// </summary>
            [JsonProperty("content")]
            public string Content { get; set; }

            /// <summary>
            /// Gets or sets the function call, if any are determined to be called.
            /// </summary>
            [JsonProperty("function_call")]
            public FunctionCall FunctionCall { get; set; }
        }

        /// <summary>
        /// Represents the function call and its Arguments.
        /// </summary>
        public class FunctionCall
        {
            /// <summary>
            /// Name of the function call.
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Arguments to pass to the function call.
            /// </summary>
            [JsonProperty("arguments")]
            public string Arguments { get; set; }
        }

        /// <summary>
        /// Represents a choice generated in the response.
        /// </summary>
        public class Choice
        {
            /// <summary>
            /// Gets or sets the index of the choice.
            /// </summary>
            [JsonProperty("index")]
            public int Index { get; set; }

            /// <summary>
            /// Gets or sets the message associated with the choice.
            /// </summary>
            [JsonProperty("message")]
            public Message Message { get; set; }

            /// <summary>
            /// Gets or sets the reason indicating the finishing condition of the choice.
            /// If the finish reason is 'function_call' then function call property will
            /// contain the function name and arguments to pass to the function.
            /// </summary>
            [JsonProperty("finish_reason")]
            public string FinishReason { get; set; }
        }

        /// <summary>
        /// Represents token usage information in the response.
        /// </summary>
        public class UsageInfo
        {
            /// <summary>
            /// Gets or sets the number of tokens used for prompts.
            /// </summary>
            [JsonProperty("prompt_tokens")]
            public int PromptTokens { get; set; }

            /// <summary>
            /// Gets or sets the number of tokens used for completions.
            /// </summary>
            [JsonProperty("completion_tokens")]
            public int CompletionTokens { get; set; }

            /// <summary>
            /// Gets or sets the total number of tokens used.
            /// </summary>
            [JsonProperty("total_tokens")]
            public int TotalTokens { get; set; }
        }
    }
}
