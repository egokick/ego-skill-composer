using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace skill_composer.Models
{
    public class Task
    {
        // The ordinal number of the task, this is used to reference the output of previous tasks with this syntax {{AIResponse[2]}}
        [JsonProperty(Order = 1)]
        public int Number { get; set; }

        [JsonProperty(Order = 2)]
        public string Name { get; set; }

        [JsonProperty(Order = 3)]
        public string Mode { get; set; } // Internal, AI, User
        
        [JsonProperty(Order = 4)]
        public string Input { get; set; }
        
        [JsonProperty(Order = 5)]
        public string SpecialAction { get; set; }
                
        public int? BestOf { get; set; }

        public string Output { get; set; }
        
 
        public string FilePath { get; set; }
        public bool? HaltProcessing { get; set; } = false;
        public bool? PrintOutput { get; set; }

        [JsonIgnore]
        public ConcurrentQueue<string> AiResponseShared { get; set; } = null;

        [JsonIgnore]
        public ConcurrentQueue<string> UserResponseShared { get; set; } = null;

        [JsonIgnore]
        public CancellationTokenSource cancellationToken { get; set; } = null;

    }
}
