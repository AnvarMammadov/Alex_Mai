using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alex_Mai.Models
{
    public class Location
    {
        public string Name { get; set; } // "Kitchen"
        public string PlaceId { get; set; } // "kitchen"
        public bool IsEnabled { get; set; } = true; // Həmin məkana getmək mümkündürmü?
    }
}
