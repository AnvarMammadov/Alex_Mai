using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alex_Mai.Models
{
    public class ChatMessage
    {
        public string Sender { get; set; } // "Alex" or "Mai"
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSentByUser { get; set; } // True if sender is "Alex"
    }
}
