using Newtonsoft.Json;

namespace skill_composer.Models
{
    /// <summary>
    /// Ai Message
    /// </summary>
    public class AiMessage
    {
        /// <summary>
        /// Role.
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// Content.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="content"></param>
        /// <param name="name"></param>
        public AiMessage(string role, string content, string name = null)
        {
            Role = role;
            Name = name;
            Content = content;
        }
    }
}
