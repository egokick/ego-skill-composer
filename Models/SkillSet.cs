using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skill_composer.Models
{
    public class SkillSet // Wrapper class to match the top-level JSON structure
    {
        public List<Skill> Skills { get; set; }
    }
}
