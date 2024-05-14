using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skill_composer.Models
{
    public class AssemblyAiTranscription
    {
        public string message_type { get; set; }
        public DateTime created { get; set; }
        public int audio_start { get; set; }
        public int audio_end { get; set; }
        public float confidence { get; set; }
        public string text { get; set; }
        public Word[] words { get; set; }
        public bool punctuated { get; set; }
        public bool text_formatted { get; set; }
    }

    public class Word
    {
        public int start { get; set; }
        public int end { get; set; }
        public float confidence { get; set; }
        public string text { get; set; }
    }
}
