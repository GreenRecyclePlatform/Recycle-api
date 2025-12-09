using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain.Entities
{
    //just model --static in RAM
    public class KnowledgeItem
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
    }
}
